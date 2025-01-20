using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class FastTextIntentRecognizer : MonoBehaviour
{
    // Path all'eseguibile fasttext e al modello addestrato
    private string fastTextPath = Application.streamingAssetsPath + "/fasttext";
    private string modelPath = Application.streamingAssetsPath + "/model_intent.bin";

    // Funzione per ottenere l'intento dalla frase
    public string PredictIntent(string userInput)
    {
        // Crea un nuovo processo per eseguire FastText
        UnityEngine.Debug.Log($"input per intent: {userInput}");
        Process process = new Process();
        process.StartInfo.FileName = fastTextPath; // Eseguibile di FastText
        process.StartInfo.Arguments = $"predict {modelPath} -"; // Modello e input dinamico
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        // Avvia il processo
        process.Start();

        // Scrivi la frase di input nel processo FastText
        process.StandardInput.WriteLine(userInput);
        process.StandardInput.Flush();
        process.StandardInput.Close();

        // Leggi l'intento predetto dal processo
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        string[] lines = result.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length > 1)
        {
            UnityEngine.Debug.LogWarning("FastText ha restituito più intent, ne prendo solo il primo.");
            result = lines[0]; // Prendi solo il primo intent
        }

        UnityEngine.Debug.Log($"Intento rilevato: {result}");

        // Restituisci l'intento predetto (rimuovi eventuali caratteri di nuova linea)
        return result.Trim();
    }

    /*Esempio di utilizzo
    void Start()
    {
        string userInput = "ciao come va";
        string predictedIntent = PredictIntent(userInput);
        UnityEngine.Debug.Log($"Intento rilevato: {predictedIntent}");
    }*/
}
