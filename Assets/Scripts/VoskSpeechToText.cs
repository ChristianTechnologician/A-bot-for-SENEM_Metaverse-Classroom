using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vosk;

public class VoskSpeechToText : MonoBehaviour
{
    [Tooltip("Location of the model, relative to the Streaming Assets folder.")]
    public string ModelPath = "vosk-model-small-it-0.22.zip";

    [Tooltip("The source of the microphone input.")]
    public VoiceProcessor VoiceProcessor;
    [Tooltip("The Max number of alternatives that will be processed.")]
    public int MaxAlternatives = 3;

    [Tooltip("How long should we record before restarting?")]
    public float MaxRecordLength = 5;

    [Tooltip("Should the recognizer start when the application is launched?")]
    public bool AutoStart = true;

    [Tooltip("The phrases that will be detected. If left empty, all words will be detected.")]
    public List<string> KeyPhrases = new List<string>();

    // Cached version of the Vosk Model.
    private Model _model;

    // Cached version of the Vosk recognizer.
    private VoskRecognizer _recognizer;

    // Conditional flag to see if a recognizer has already been created.
    // TODO: Allow for runtime changes to the recognizer.
    private bool _recognizerReady;

    // Holds all of the audio data until the user stops talking.
    private readonly List<short> _buffer = new List<short>();

    // Called when the state of the controller changes.
    public Action<string> OnStatusUpdated;

    // Called after the user is done speaking and vosk processes the audio.
    public Action<string> OnTranscriptionResult;

    // The absolute path to the decompressed model folder.
    private string _decompressedModelPath;

    // A string that contains the keywords in Json Array format
    private string _grammar = "";

    // Flag that is used to wait for the model file to decompress successfully.
    private bool _isDecompressing;

    // Flag that is used to wait for the script to start successfully.
    private bool _isInitializing;

    // Flag that is used to check if Vosk was started.
    private bool _didInit;

    // Threading Logic

    // Flag to signal we are ending
    private bool _running;

    // Thread safe queue of microphone data.
    private readonly ConcurrentQueue<short[]> _threadedBufferQueue = new ConcurrentQueue<short[]>();

    // Thread safe queue of results
    private readonly ConcurrentQueue<string> _threadedResultQueue = new ConcurrentQueue<string>();

    static readonly ProfilerMarker voskRecognizerCreateMarker = new ProfilerMarker("VoskRecognizer.Create");
    static readonly ProfilerMarker voskRecognizerReadMarker = new ProfilerMarker("VoskRecognizer.AcceptWaveform");

    // If Auto start is enabled, starts Vosk speech-to-text.
    private VoskResultText _voskResultText;

    // Tasto per attivare/disattivare
    public KeyCode activationKey = KeyCode.Space;
    private string _lastResult = string.Empty;
    public Scriba scriba;
    int type;

    void Start()
    {
        _voskResultText = GetComponent<VoskResultText>();

        if (AutoStart)
        {
            StartVoskStt();
        }
        //scriba.Initialize();
        // Carica e processa il file audio
        //string audioFilePath = Path.Combine(Application.persistentDataPath, "recordedAudio.wav");
        //ProcessAudioFile(audioFilePath);

    }

    void Update()
    {
        // Rileva la pressione del tasto di attivazione
        if (Input.GetKeyDown(activationKey))
        {
            ToggleRecognizer();
        }

        // Processa i risultati dalla coda
        if (_threadedResultQueue.TryDequeue(out string voiceResult))
        {
            OnTranscriptionResult?.Invoke(voiceResult);
        }
    }

    private void ToggleRecognizer()
    {
        if (_running)
        {
            StopRecognizer();
        }
        else
        {
            StartRecognizer(0);
        }
    }

    public void StartRecognizer(int type)
    {
        this.type = type;
        Debug.Log("Start Recognizer");
        _running = true;
        if (!VoiceProcessor.IsRecording)
        {
            VoiceProcessor.StartRecording();
        }
        Task.Run(ThreadedWork).ConfigureAwait(false);
    }

    public void StopRecognizer()
    {
        Debug.Log("Stop Recognizer");
        _running = false;
        if (VoiceProcessor.IsRecording)
        {
            VoiceProcessor.StopRecording();
        }
    }

    // Metodo già esistente
    private void VoiceProcessorOnOnFrameCaptured(short[] samples)
    {
        _threadedBufferQueue.Enqueue(samples);
        Debug.Log($"VoskSpeechToText: Data received from VoiceProcessor. Length: {samples.Length}");
    }

    // Metodo già esistente
    private void VoiceProcessorOnOnRecordingStop()
    {
        Debug.Log("Stopped");
        _running = false;
    }

    // Metodo già esistente
    private async Task ThreadedWork()
    {
        Debug.Log("siamo nel recognizer");
        voskRecognizerCreateMarker.Begin();
        if (!_recognizerReady)
        {
            UpdateGrammar();
            Debug.Log("Il crash è qui");
            if (string.IsNullOrEmpty(_grammar))
            {
                if (_model == null) { Debug.LogError("Il modello Vosk (_model) non è inizializzato."); return; }
                Debug.Log("problema su dichiarzione?");
                _recognizer = new VoskRecognizer(_model, 16000.0f);
                Debug.Log("se grammar è vuoto/nullo");
            }
            else
            {
                _recognizer = new VoskRecognizer(_model, 16000.0f, _grammar);
                Debug.Log("se grammar non è vuoto/nullo");
            }
            Debug.Log("qui?");
            _recognizer.SetMaxAlternatives(MaxAlternatives);
            _recognizerReady = true;

            Debug.Log("Recognizer ready");
        }
        voskRecognizerCreateMarker.End();
        Debug.Log("forse qui");
        voskRecognizerReadMarker.Begin();

        while (_running)
        {
            if (_threadedBufferQueue.TryDequeue(out short[] voiceResult))
            {
                Debug.Log($"VoskSpeechToText: Processing buffer with length: {voiceResult.Length}");
                if (_recognizer.AcceptWaveform(voiceResult, voiceResult.Length))
                {
                    var result = _recognizer.Result();
                    if (result != _lastResult) // Controlla se è uguale al risultato precedente
                    {
                        _lastResult = result;
                        _threadedResultQueue.Enqueue(result);
                        Debug.Log($"VoskSpeechToText: Enqueued transcription result: {result}");
                        _voskResultText.OnTranscriptionResult(result); // Assicurati che sia pubblico
                    }
                    else
                    {
                        Debug.Log("VoskSpeechToText: Ignoring duplicate transcription result.");
                    }
                    /*recognizedWords.Clear();
                    foreach (var word in result)
                    {
                        recognizedWords.Add(word);
                    }
                    scriba.ProcessCommand(recognizedWords);
                    if (recognizedWords.Contains("ok scriba"))
                    {
                        startcoroutine
                    }**/
                    /*if(type==1)
                    {
                        if(scriba.ProcessCommand(result)==1){
                            return;
                        }else{
                            break;
                        }
                    }*/
                    //var result = _recognizer.Result();
                    //_threadedResultQueue.Enqueue(result);
                    //Debug.Log($"VoskSpeechToText: Enqueued transcription result: {result}");
                    //Debug.Log("abbiamo inviato");
                    //_voskResultText.OnTranscriptionResult(result); // Assicurati che sia pubblico
                }
            }
            else
            {
                Debug.Log("VoskSpeechToText: Buffer is empty, waiting...");
                await Task.Delay(100);
            }
        }
        voskRecognizerReadMarker.End();
        /*recognizer.Stop();
        recognizer.Dispose();*/
    }


    /// <summary>
    /// Start Vosk Speech to text
    /// </summary>
    /// <param name="keyPhrases">A list of keywords/phrases. Keywords need to exist in the models dictionary, so some words like "webview" are better detected as two more common words "web view".</param>
    /// <param name="modelPath">The path to the model folder relative to StreamingAssets. If the path has a .zip ending, it will be decompressed into the application data persistent folder.</param>
    /// <param name="startMicrophone">"Should the microphone after vosk initializes?</param>
    /// <param name="maxAlternatives">The maximum number of alternative phrases detected</param>
    public void StartVoskStt(List<string> keyPhrases = null, string modelPath = default, bool startMicrophone = false, int maxAlternatives = 3)
    {
        if (_isInitializing)
        {
            Debug.LogError("Initializing in progress!");
            return;
        }
        if (_didInit)
        {
            Debug.LogError("Vosk has already been initialized!");
            return;
        }

        if (!string.IsNullOrEmpty(modelPath))
        {
            ModelPath = modelPath;
        }

        if (keyPhrases != null)
        {
            KeyPhrases = keyPhrases;
        }

        MaxAlternatives = maxAlternatives;
        StartCoroutine(DoStartVoskStt(startMicrophone));
    }

    private IEnumerator DoStartVoskStt(bool startMicrophone)
    {
        _isInitializing = true;
        yield return WaitForMicrophoneInput();

        yield return Decompress();

        OnStatusUpdated?.Invoke("Loading Model from: " + _decompressedModelPath);
        // Vosk.Vosk.SetLogLevel(0);
        _model = new Model(_decompressedModelPath);

        yield return null;

        OnStatusUpdated?.Invoke("Initialized");
        VoiceProcessor.OnFrameCaptured += VoiceProcessorOnOnFrameCaptured;
        VoiceProcessor.OnRecordingStop += VoiceProcessorOnOnRecordingStop;

        if (startMicrophone)
            VoiceProcessor.StartRecording();

        _isInitializing = false;
        _didInit = true;
        //scriba.Initialize();
    }

    private void UpdateGrammar()
    {
        Debug.Log("updateGrammar 1");
        if (KeyPhrases.Count == 0)
        {
            _grammar = "";
            Debug.Log("updateGrammar gg");
            return;
        }
        Debug.Log("updateGrammar 2");
        JSONArray keywords = new JSONArray();
        foreach (string keyphrase in KeyPhrases)
        {
            keywords.Add(new JSONString(keyphrase.ToLower()));
        }

        keywords.Add(new JSONString("[unk]"));

        _grammar = keywords.ToString();
        Debug.Log("updateGrammar ok");
    }

    private IEnumerator Decompress()
    {
        if (!Path.HasExtension(ModelPath) ||
            Directory.Exists(Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(ModelPath))))
        {
            OnStatusUpdated?.Invoke("Using existing decompressed model.");
            _decompressedModelPath = Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(ModelPath));
            Debug.Log(_decompressedModelPath);
            yield break;
        }

        OnStatusUpdated?.Invoke("Decompressing model...");
        string dataPath = Path.Combine(Application.streamingAssetsPath, ModelPath);

        Stream dataStream;
        // Read data from the streaming assets path. You cannot access the streaming assets directly on Android.
        if (dataPath.Contains("://"))
        {
            UnityWebRequest www = UnityWebRequest.Get(dataPath);
            www.SendWebRequest();
            while (!www.isDone)
            {
                yield return null;
            }
            dataStream = new MemoryStream(www.downloadHandler.data);
        }
        // Read the file directly on valid platforms.
        else
        {
            dataStream = File.OpenRead(dataPath);
        }

        // Read the Zip File
        var zipFile = ZipFile.Read(dataStream);

        // Listen for the zip file to complete extraction
        zipFile.ExtractProgress += ZipFileOnExtractProgress;

        // Update status text
        OnStatusUpdated?.Invoke("Reading Zip file");

        // Start Extraction
        zipFile.ExtractAll(Application.persistentDataPath);

        // Wait until it's complete
        while (_isDecompressing == false)
        {
            yield return null;
        }
        // Override path given in ZipFileOnExtractProgress to prevent crash
        _decompressedModelPath = Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(ModelPath));

        // Update status text
        OnStatusUpdated?.Invoke("Decompressing complete!");
        // Wait a second in case we need to initialize another object.
        yield return new WaitForSeconds(1);
        // Dispose the zipfile reader.
        zipFile.Dispose();
    }

    // The function that is called when the zip file extraction process is updated.
    private void ZipFileOnExtractProgress(object sender, ExtractProgressEventArgs e)
    {
        if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
        {
            _isDecompressing = true;
            _decompressedModelPath = e.ExtractLocation;
        }
    }

    // Wait until microphones are initialized
    private IEnumerator WaitForMicrophoneInput()
    {
        while (Microphone.devices.Length <= 0)
            yield return null;
    }

    //file 
    public void ProcessAudioFile(string filePath)
    {
        string wavPath = filePath;
        byte[] audioData = LoadAudioData(filePath);
        if (audioData != null)
        {
            using (MemoryStream ms = new MemoryStream(audioData))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // Salta l'header del file WAV (44 bytes)
                    reader.BaseStream.Seek(44, SeekOrigin.Begin);

                    short[] buffer = new short[(audioData.Length - 44) / 2];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = reader.ReadInt16();
                    }
                    _running = true;
                    _threadedBufferQueue.Enqueue(buffer);
                    Task.Run(ThreadedWork).ConfigureAwait(false);
                    //if (_recognizer == null) { Debug.LogError("Recognizer is not initialized"); _running = false;return; }
                    //if (_recognizer.AcceptWaveform(buffer, buffer.Length)){
                        //var result = _recognizer.Result();
                        //Debug.Log("Recognition result: " + result);
                        StartCoroutine(StopProcessingAfterTime(2));
                        //_running = false;
                        // Cancella il file audio 
                        DeleteAudioFile(wavPath);
                    /*else{
                        _running = false;
                        Debug.Log("errore recognizer");
                    }*/
                    
                }
            }
        }
    }

    private IEnumerator StopProcessingAfterTime(float time) { yield return new WaitForSeconds(time); _running = false; Debug.Log("Processing stopped"); }

    byte[] LoadAudioData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Audio file not found: " + filePath);
            return null;
        }

        // Carica i dati audio dal file (assicurati che il formato sia corretto)
        return File.ReadAllBytes(filePath);
    }
    public void DeleteAudioFile(string filepath) { if (File.Exists(filepath)) { File.Delete(filepath); Debug.Log($"Audio file deleted from {filepath}"); } else { Debug.LogError("File not found to delete"); } }

}
