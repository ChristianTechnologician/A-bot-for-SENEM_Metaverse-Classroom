using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotInteractionPrompt : MonoBehaviour
{
    //public GameObject promptUI; // Il testo UI del prompt ("Premi E per interagire")
    public GameObject menuUI; // Il piccolo menu da aprire
    //public TMP_Text promptText;
    public float interactionDistance = 3f; // Distanza alla quale il giocatore può interagire
    //private bool isPlayerNearby = false; // Se il giocatore è vicino all'oggetto

    private void Start()
    {
        // Nascondi il prompt e il menu all'inizio
        //promptUI.SetActive(false);
        menuUI.SetActive(false);
    }

    /*private void Update()
    {
        // Calcola la distanza tra il giocatore e l'oggetto
        float distance = 1f;// Vector3.Distance(player.position, transform.position);

        // Se il giocatore è entro la distanza di interazione
        if (distance < interactionDistance)
        {
            // Mostra il prompt
            promptUI.SetActive(true);
            //isPlayerNearby = true;

            // Cambia il testo del prompt
            promptText.text = "Premi E per interagire";

            // Se il giocatore preme il tasto E, apri il menu
            //if (Input.GetKeyDown(KeyCode.E))
            //{
                //OpenMenu();
            //}
        }
        else
        {
            // Nascondi il prompt se il giocatore si allontana
            promptUI.SetActive(false);
            //isPlayerNearby = false;
        }
    }*/

    public void OpenMenu()
    {
        // Nascondi il prompt e mostra il menu
       //promptUI.SetActive(false);
        menuUI.SetActive(true);
    }

    public void CloseMenu()
    {
        // Funzione per chiudere il menu e tornare alla normale interazione
        menuUI.SetActive(false);
    }

    // Rilevamento del trigger (se vuoi usare i Collider invece della distanza)
    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            promptUI.SetActive(true);
            promptText.text = "Premi E per interagire";
            //isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            promptUI.SetActive(false);
            //isPlayerNearby = false;
        }
    }*/

}
