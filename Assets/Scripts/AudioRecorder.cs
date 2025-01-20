using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class AudioRecorder : MonoBehaviour
{
    public AudioClip audioClip;
    private string microphone;
    private bool isRecording = false;
    private Image background;
    private TMP_Text text;

    private Image recordingIndicator; // L'elemento UI (Testo o Immagine) che indica la registrazione
    public float blinkInterval = 0.5f;    // Intervallo per far lampeggiare l'indicatore
    private bool isBlinking = false;      // Stato del lampeggiamento
    private TMP_Text recState;

    [SerializeField] private SavWavVosk savWavVosk;
    public List<string> Devices { get; private set; }
    public int CurrentDeviceIndex { get; private set; }
    [SerializeField] private int MicrophoneIndex;
    public string CurrentDeviceName
        {
            get
            {
                if (CurrentDeviceIndex < 0 || CurrentDeviceIndex >= Microphone.devices.Length)
                    return string.Empty;
                return Devices[CurrentDeviceIndex];
            }
        }

void Awake()
    {
        UpdateDevices();
        Debug.Log("VoiceProcessor: Devices updated.");
    }

#if UNITY_EDITOR
    void Update()
    {
        if (CurrentDeviceIndex != MicrophoneIndex)
        {
            ChangeDevice(MicrophoneIndex);
            Debug.Log("VoiceProcessor: Device changed to " + CurrentDeviceName);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            recordingIndicator = GameObject.Find("MicBot").GetComponent<Image>();
            recState = GameObject.Find("MicBot").GetComponentInChildren<TMP_Text>();
            background = GameObject.Find("BotCommand").GetComponent<Image>();
            text = GameObject.Find("BotCommand").GetComponentInChildren<TMP_Text>();
            if(background.enabled == true && text.enabled == true){
                if (isRecording)
                {
                    StopRecording();
                }
                else
                {
                    StartRecording();
                }
            }
        }
    }
#endif

    /// <summary>
    /// Updates list of available audio devices
    /// </summary>
    public void UpdateDevices()
    {
        Devices = new List<string>();
        foreach (var device in Microphone.devices)
            Devices.Add(device);

        if (Devices == null || Devices.Count == 0)
        {
            CurrentDeviceIndex = -1;
            Debug.LogError("There is no valid recording device connected");
            return;
        }

        CurrentDeviceIndex = MicrophoneIndex;
    }

    /// <summary>
    /// Change audio recording device
    /// </summary>
    /// <param name="deviceIndex">Index of the new audio capture device</param>
    public void ChangeDevice(int deviceIndex)
    {
        if (deviceIndex < 0 || deviceIndex >= Devices.Count)
        {
            Debug.LogError(string.Format("Specified device index {0} is not a valid recording device", deviceIndex));
            return;
        }
            CurrentDeviceIndex = deviceIndex;
     
    }

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }

        if (savWavVosk == null) { Debug.LogError("savWavVosk is not assigned!"); }

        /*if (recordingIndicator != null)
        {
            recordingIndicator.SetActive(false);
            recState.enabled = false;
        }*/
    }

    /*void Update()
    {
        // Controlla l'input della tastiera (ad esempio, la barra spaziatrice)
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }
    }*/

    public void StartRecording()
    {
        if (microphone != null)
        {
            recordingIndicator.enabled = true; 
            recState.enabled = true;
            /*if (recordingIndicator != null)
            {
                recordingIndicator.SetActive(true); 
                recState.enabled = true;
                StartCoroutine(BlinkRecordingIndicator());
            }*/
            audioClip = Microphone.Start(CurrentDeviceName, true, 20, 16000);
            if (audioClip == null) { Debug.LogError("Microphone.Start returned null AudioClip."); }else{
            Debug.Log($"audioClip created: {audioClip != null}");}
            isRecording = true;
            Debug.Log("Recording started");
        }
    }

    public void StopRecording()
    {
        if (microphone != null)
        {
            Microphone.End(CurrentDeviceName);
            recordingIndicator.enabled = false;
            recState.enabled = false;
            /*if (recordingIndicator != null)
            {
                recordingIndicator.SetActive(false);
                recState.enabled = false;
                StopCoroutine(BlinkRecordingIndicator());
            }*/
            isRecording = false;
            Debug.Log("Recording stopped");
            SaveAudio();
        }
    }

    /* Coroutine per far lampeggiare l'indicatore di registrazione
    private IEnumerator BlinkRecordingIndicator()
    {
        isBlinking = true;
        while (isRecording)
        {
            // Cambia la visibilità dell'indicatore
            recordingIndicator.SetActive(!recordingIndicator.activeSelf);
            yield return new WaitForSeconds(blinkInterval); // Attendi per l'intervallo impostato
        }
        isBlinking = false;
    }*/

    public void SaveAudio()
    {
        if (audioClip == null)
        {
            Debug.LogError("No audio recorded");
            return;
        }
        // Verifica che savWavVosk non sia null 
        if (savWavVosk == null) { Debug.LogError("savWavVosk is not assigned!"); return; }

        string filePath = Path.Combine(Application.persistentDataPath,"recordedAudio.wav");
        Debug.Log("Saving audio to: " + filePath);
        if(savWavVosk.Save(filePath, audioClip))
        {
          Debug.Log($"operazione completata");  
        }
        else { Debug.LogError("Failed to save audio using savWavVosk."); }
        
    }
}

