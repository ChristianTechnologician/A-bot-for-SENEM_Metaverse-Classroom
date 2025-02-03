using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BotInfoHandler : MonoBehaviour
{
    private Image background;
    private TMP_Text text;

    void Start()
    {
        background = gameObject.GetComponent<Image>();
        text = gameObject.GetComponentInChildren<TMP_Text>();

        background.enabled = false;
        text.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
      // Controlla se il pulsante del mouse sinistro viene premuto
        if (Input.GetMouseButtonDown(0))
        {
            HideMessage(); // Nascondi il popup se viene cliccato il mouse
        }
    }

    void HideMessage()
    {
        background.enabled = false;
        text.enabled = false;
    }

    System.Collections.IEnumerator HideMessageAfterTime(float seconds)
    {
        // Aspetta il numero di secondi specificato
        yield return new WaitForSeconds(seconds);

        // Nascondi il popup
        HideMessage();
    }

    public void sendErrorMessagge(int type)
    {
        if(type == 0){
            background.enabled = true;
            text.text = "Nessun tipo di intent rilevato.\nRiformula il comando per il bot";
            text.enabled = true;
            // Avvia la coroutine che aspetta 3 secondi e chiude il popup
            StartCoroutine(HideMessageAfterTime(3.0f));
        }else if(type == 1){
            background.enabled = true;
            text.text = "Nessun colore rilevato.\nRiformula il comando per il bot esprimendo un colore tra quelli disponibili(rosso,verde,blu,giallo,bianco,nero,grigio)";
            text.enabled = true;
            // Avvia la coroutine che aspetta 3 secondi e chiude il popup
            StartCoroutine(HideMessageAfterTime(3.0f));
        }else if(type == 2){
            background.enabled = true;
            text.text = "Nessun comando di incremento o diminuzione del font individuato oppure nessun numero valido trovato nel testo o aumento generico/di tot.";
            text.enabled = true;
            // Avvia la coroutine che aspetta 3 secondi e chiude il popup
            StartCoroutine(HideMessageAfterTime(3.0f));
        }
    }
}

