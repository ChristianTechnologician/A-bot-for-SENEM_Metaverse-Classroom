using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class VoskResultText : MonoBehaviour 
{
    public TMP_Text bestTranscription;
    public VoskSpeechToText VoskSpeechToText;
    public TranscriptionSync transcriptionSync;
    public FastTextIntentRecognizer fastTextIntentRecognizer;
    public ColorChanger colorChanger;
    public ColorChangerWhiteBoard colorChangerWhiteBoard;
    public TextChat textChat;
    private string lastTranscription = "";

    void Awake()
    {
        if (VoskSpeechToText != null)
        {
            VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
            Debug.Log("VoskResultText: VoskSpeechToText assegnato correttamente.");
        }
        else
        {
            Debug.LogError("VoskSpeechToText non è stato assegnato.");
        }
    }

    public void OnTranscriptionResult(string obj)
    {
        Debug.Log("VoskResultText: Ricevuto risultato di trascrizione: " + obj);

        if (string.IsNullOrEmpty(obj))
        {
            Debug.LogWarning("Transcription result is null or empty. Skipping.");
            return;
        }

        bestTranscription.text = "";
        var result = new RecognitionResult(obj);
        bestTranscription.text += "";
        bestTranscription.text += result.GetBestPhrase() + "\n";
        bestTranscription.text += "\n";

        if (string.IsNullOrEmpty(bestTranscription.text))
        {
            Debug.LogWarning("Best transcription is null or empty. Skipping.");
            return;
        }

        if (bestTranscription.text == lastTranscription)
        {
            Debug.Log("Duplicate transcription detected. Skipping.");
            return;
        }

        Debug.Log("Testo originale: " + bestTranscription.text);

        // Espressione regolare per trovare frasi consecutive ripetute
        string pattern = @"(\b.+?[.!?])(\s+\1)+";

        // Sostituzione delle ripetizioni
        string testoCorretto = "";
        testoCorretto = Regex.Replace(bestTranscription.text, pattern, "$1").Trim();
        
        Debug.Log("Testo senza ripetizioni: " + testoCorretto);

        string intent = fastTextIntentRecognizer.PredictIntent(testoCorretto);
        Debug.Log("intent: " + intent);
        Debug.Log("Siamo al bivio");
        if(intent.Contains("__label__coloreBot")){
            colorChanger.ColorFinder(testoCorretto);
            Debug.Log("VoskResultText: Migliore trascrizione inviata a ColorChanger: " + testoCorretto);
            bestTranscription.text = "";
            testoCorretto = "";
        }
        if(intent.Contains("__label__coloreLavagna")){
            colorChangerWhiteBoard.ColorFinder(testoCorretto);
            Debug.Log("VoskResultText: Migliore trascrizione inviata a ColorChangerWhiteBoard: " + testoCorretto);
            bestTranscription.text = "";
            testoCorretto = "";
        }
        if(intent.Contains("__label__grandezzaFont")){
            textChat.changeFontSize(testoCorretto);
            Debug.Log("VoskResultText: Migliore trascrizione inviata a changeFontSize: " + testoCorretto);
            bestTranscription.text = "";
            testoCorretto = "";
        }
        if(intent.Equals("__label__trascrizione")){
            //lastTranscription = bestTranscription.text;
            //transcriptionSync.OnTranscriptionResult(testoCorretto); // Invio a TranscriptionSync
            textChat.SendTranscriptionRpc(testoCorretto);
            Debug.Log("VoskResultText: Migliore trascrizione inviata a TranscriptionSync: " + testoCorretto);
            bestTranscription.text = "";
            testoCorretto = "";
        }
    }
}
