using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Collections.Generic;

public class TextChat : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputField;
    public bool isSelected = false;
    private GameObject commandInfo;

    // Campo Text da modificare
    public TMP_Text myText;
    // Range minimo e massimo per la grandezza del font
    public int minFontSize = 4;
    public int maxFontSize = 20;
    
    //Campo InputField da modificare
    private GameObject myInputField;

    //Per i messaggi di errore del bot
    private BotInfoHandler botInfoHandler;

    // Fattore di scala per adattare l'altezza della casella di input
    public float scaleFactor = 1.2f; 

    // Dizionario che converte parole in numeri
    private Dictionary<string, int> numberWords = new Dictionary<string, int>()
    {
        { "quattro", 4 },{ "cinque", 5 }, { "sei", 6 }, { "sette", 7 }, { "otto", 8 },
        { "nove", 9 }, { "dieci", 10 }, { "undici", 11 }, { "dodici", 12 },
        { "tredici", 13 }, { "quattordici", 14 }, { "quindici", 15 },
        { "sedici", 16 }, { "diciassette", 17 }, { "diciotto", 18 },
        { "diciannove", 19 }, { "venti", 20 }
    };

    private void Start()
    {
        commandInfo = GameObject.Find("CommandInfo");
    }

    public void LateUpdate()
    {
        if(Input.GetKeyUp(KeyCode.Return) && !isSelected)
        {
            isSelected = true;
            // Set the selected GameObject to the input field
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.caretPosition = inputField.text.Length;
            commandInfo.SetActive(false);
        }

        else if(Input.GetKeyUp(KeyCode.Escape) && isSelected)
        {
            isSelected = false;
            // Reset the selected GameObject 
            EventSystem.current.SetSelectedGameObject(null);
            commandInfo.SetActive(true);
        }

        else if (Input.GetKeyUp(KeyCode.Return) && isSelected && inputField.text != "")
        {
            photonView.RPC("SendMessageRpc", RpcTarget.AllBuffered, PhotonNetwork.NickName, inputField.text);
            inputField.text = "";
            isSelected = false;
            EventSystem.current.SetSelectedGameObject(null);
            commandInfo.SetActive(true);
        }
    }

    [PunRPC]
    public void SendMessageRpc(string sender, string msg)
    {
        string message = $"<color=\"yellow\">{sender}</color>: {msg}";
        Logger.Instance.LogInfo(message);
        LogManager.Instance.LogInfo($"{sender} wrote in the chat: \"{msg}\"");
    }

    [PunRPC]
    public void SendTranscriptionRpc(string msg)
    {
        string sender = PhotonNetwork.NickName;
        string message = $"<color=\"yellow\">{sender}</color>: {msg}";
        Logger.Instance.LogInfo(message);
        LogManager.Instance.LogInfo($"{sender} wrote in the chat: \"{msg}\"");
    }

    // Funzione per estrarre il numero (scritto in parole) dal testo
    int ExtractNumberFromText(string text)
    {
        // Converti il testo in minuscolo per rendere la ricerca non case-sensitive
        text = text.ToLower();

        // Scorri il dizionario per trovare la parola numerica
        foreach (KeyValuePair<string, int> entry in numberWords)
        {
            // Usa un'espressione regolare per trovare la parola nel testo
            if (Regex.IsMatch(text, @"\b" + entry.Key + @"\b"))
            {
                return entry.Value;
            }
        }

        // Se non è stata trovata nessuna parola numerica, restituisci -1
        return -1;
    }


    public void changeFontSize(string transcription){

        string text = transcription.ToLower();

        // Verifica se ci sono comandi come "ingrandisci" o "diminuisci"
        bool shouldIncrease = Regex.IsMatch(text, @"\bingrandisci\b|\baumenta\b");
        bool shouldDecrease = Regex.IsMatch(text, @"\bdiminuisci\b|\briduci\b");

        // Estrai il numero dal testo inserito
        int fontSize = ExtractNumberFromText(transcription);

        //myInputField = GameObject.Find("CommandInfo");
        // Ottieni il RectTransform della casella di input
        RectTransform inputRectTransform = inputField.GetComponent<RectTransform>();
        
        // Controlla se c'è una frase come "a venti" per impostare direttamente il font
        Match matchDirectSet = Regex.Match(text, @"\ba\s*(\b\w+\b)");
        if(!matchDirectSet.Success){
            if(shouldIncrease || shouldDecrease){
                int changeValue = 2;
        
                if (fontSize != -1)
                {
                    changeValue = fontSize;
                }
                // Se il comando è per ingrandire, aumenta la dimensione del font
                if (shouldIncrease)
                {
                    myText.fontSize = Mathf.Clamp(myText.fontSize + changeValue, minFontSize, maxFontSize);
                    // Calcola la nuova altezza in base alla dimensione del font e al fattore di scala
                    float newHeight = (myText.fontSize + changeValue) * scaleFactor;

                    // Applica la nuova altezza alla casella di input
                    inputRectTransform.sizeDelta = new Vector2(inputRectTransform.sizeDelta.x, newHeight);
                }

                // Se il comando è per ridurre, diminuisci la dimensione del font
                if (shouldDecrease)
                {
                    myText.fontSize = Mathf.Clamp(myText.fontSize - changeValue, minFontSize, maxFontSize);
                    // Calcola la nuova altezza in base alla dimensione del font e al fattore di scala
                    float newHeight = (myText.fontSize - changeValue) * scaleFactor;

                    // Applica la nuova altezza alla casella di input
                    inputRectTransform.sizeDelta = new Vector2(inputRectTransform.sizeDelta.x, newHeight);
                }
            }else{
                Debug.Log("Nessun comando valido trovato.");
                botInfoHandler = FindObjectOfType<BotInfoHandler>();
                botInfoHandler.sendErrorMessagge(2);
            }
        }else{
            // Estrai il numero dal testo inserito
            //int fontSize = ExtractNumberFromText(transcription);

            // Se è stato trovato un numero, cambia la grandezza del font
            if (fontSize != -1)
            {
                // Assicura che la dimensione sia compresa nel range specificato
                fontSize = Mathf.Clamp(fontSize, minFontSize, maxFontSize);

                // Imposta la grandezza del font
                myText.fontSize = fontSize;
                // Calcola la nuova altezza in base alla dimensione del font e al fattore di scala
                float newHeight = myText.fontSize * scaleFactor;

                // Applica la nuova altezza alla casella di input
                inputRectTransform.sizeDelta = new Vector2(inputRectTransform.sizeDelta.x, newHeight);
            }
            else
            {
                Debug.Log("Nessun numero valido trovato nel testo o aumento generico/di tot.");
                botInfoHandler = FindObjectOfType<BotInfoHandler>();
                botInfoHandler.sendErrorMessagge(2);
            }
        }
    }
}