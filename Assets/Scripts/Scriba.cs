/*using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
//using vosk;
using Photon.Voice.Unity;
using UnityEngine.UI;

public class Scriba : MonoBehaviour
{
    public VoskSpeechToText voskSpeechToText;
    private List<string> recognizedWords = new List<string>();
    //public Text statusText;
    private bool isMicrophoneClosed = false;
    public AudioRecorder audioRecorder;
    private PhotonView view;

    void Start()
    {
        //view = GameObject.Find("PlayerVoiceController").GetComponent<PhotonView>();
        // Il riconoscitore vocale parte solo se avviato da un'altra classe
        //voiceCommandBot.Initialize();
        //Initialize();
    }

    public void Initialize()
    {
        /*Configura il riconoscitore vocale Vosk
        recognizer = new SpeechRecognizer();
        recognizer.SetMicrophone(true);
        recognizer.Recognize += OnRecognize;
        recognizer.Start();//
        int type = 1;
        voskSpeechToText.StartRecognizer(type);

    }

    /*void OnRecognize(object sender, SpeechResult result)
    {
        recognizedWords.Clear();
        foreach (var word in result.Text)
        {
            recognizedWords.Add(word);
        }
        ProcessCommand();
    }//

    public int ProcessCommand(string recognizedWords)
    {
        Debug.Log("riconoscimento");
        if (recognizedWords.Contains("ok scriba") /*&& isMicrophoneClosed//)
        {
            Debug.Log("riconosciuto");
            voskSpeechToText.StopRecognizer();
            StartAudioRecorder();
            return 1;
        }
        return 0;
    }

    void StartAudioRecorder()
    {
        Debug.Log("Esprimi ciò che vuoi venga trascritto");
        //StartCoroutine(StartRecordingCoroutine());
        StartRecordingCoroutine();
    }

    /*IEnumerator// void StartRecordingCoroutine()
    {
        // Avvia la registrazione audio
        audioRecorder.StartRecording();
        //yield return new WaitForSeconds(20);
        audioRecorder.StopRecording();
        Debug.Log("Registrazione terminata! Ripeti 'Ok Sesa' per iniziarne un'altra");
    }

   /* void Update()
    {
        // Verifica lo stato del microfono
        if (view.Instance.Client.State != ExitGames.Client.Photon.LoadBalancing.ClientState.Connected)
        {
            isMicrophoneClosed = true;
        }
        else
        {
            isMicrophoneClosed = false;
        }
    }*/

   /* void OnDestroy()
    {
        recognizer.Stop();
        recognizer.Dispose();
    }//
}
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vosk;
using System.IO;
using System.Text;

public class Scriba : MonoBehaviour
{
    private VoskRecognizer recognizer;
    private AudioSource audioSource;
    private AudioClip recordedClip;
    private bool isListening = false;
    private bool isRecording = false;
    private string command = "ok scriba";

    [SerializeField] private AudioRecorder audioRecorder;

    void Start()
    {
        Vosk.Vosk.SetLogLevel(0); // Disables the Vosk logs
        string modelPath = Path.Combine(Application.streamingAssetsPath, "vosk-model-small-it-0.22");
        if (Directory.Exists(modelPath))
        {
            recognizer = new VoskRecognizer(new Model(modelPath), 16000f);
            recognizer.SetMaxAlternatives(0);
            recognizer.SetWords(true);
        }
        else
        {
            Debug.LogError("Model path not found: " + modelPath);
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isListening && !isRecording)
        {
            StartCoroutine(RecognizeSpeech());
        }
    }

    IEnumerator RecognizeSpeech()
    {
        isRecording = true;
        recordedClip = Microphone.Start(null, false, 5, 16000);
        yield return new WaitForSeconds(5);

        float[] samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        byte[] bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)(samples[i] * short.MaxValue);
            bytes[i * 2] = (byte)(s & 0x00ff);
            bytes[i * 2 + 1] = (byte)((s & 0xff00) >> 8);
        }
        if (recognizer.AcceptWaveform(bytes, bytes.Length))
        {
            var result = recognizer.Result();
            if (result.Contains(command))
            {
                Debug.Log("Command recognized: " + command);
                audioRecorder.StartRecording();
                audioRecorder.StopRecording();
            }
        }

        isRecording = false;
    }

    public void StartListening()
    {
        isListening = true;
        Debug.Log("Started Listening for the command: " + command);
    }

    public void StopListening()
    {
        isListening = false;
        Debug.Log("Stopped Listening.");
    }
}
