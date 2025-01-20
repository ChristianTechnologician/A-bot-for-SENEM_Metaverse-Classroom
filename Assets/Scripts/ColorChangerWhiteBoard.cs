using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class ColorChangerWhiteBoard : MonoBehaviour
{
    public Renderer objectRenderer;  // Il renderer dell'oggetto a cui cambiare colore
    private Color currentColor;      // Memorizza il colore attuale dell'oggetto

    // Funzione chiamata quando ricevi la trascrizione del testo
    public void ColorFinder(string transcription)
    {
        string userInput = transcription.ToLower(); // Trasformiamo la trascrizione in minuscolo
        Debug.Log("Trascrizione ricevuta: " + transcription);

        // Pattern regex per trovare i colori all'interno della frase
        string[] colors = { "rosso", "verde", "blu", "giallo", "bianco", "nero", "grigio" };
        string colorPattern = @"\b(rosso|verde|blu|giallo|bianco|nero|grigio)\b";

        // Trova tutti i colori nella frase
        MatchCollection matches = Regex.Matches(userInput, colorPattern);

        if (matches.Count == 0)
        {
            Debug.Log("Nessun colore riconosciuto nella frase.");
            return;
        }

        // Se trova un solo colore, è il nuovo colore con cui cambiare
        if (matches.Count == 1)
        {
            string newColorName = matches[0].Value;
            Color newColor = GetColorFromString(newColorName);
            Debug.Log("Colore trovato: " + newColorName);
            ChangeObjectColor(newColor);
        }
        // Se trova due colori, il primo è quello attuale, il secondo è il nuovo
        else if (matches.Count >= 2)
        {
            string currentColorName = matches[0].Value;
            string newColorName = matches[1].Value;

            // Verifica se il colore attuale corrisponde a quello dell'oggetto
            Color expectedCurrentColor = GetColorFromString(currentColorName);
            if (currentColor == expectedCurrentColor)
            {
                Debug.Log($"Il colore attuale è già {currentColorName}. Cambiamo in {newColorName}.");
                Color newColor = GetColorFromString(newColorName);
                ChangeObjectColor(newColor);
            }
            else
            {
                Debug.Log($"Colore attuale ({currentColorName}) non corrisponde a quello effettivo ({currentColor}). Cambiamo comunque.");
                Color newColor = GetColorFromString(newColorName);
                ChangeObjectColor(newColor);
            }
        }
    }

    // Funzione per cambiare il colore dell'oggetto
    void ChangeObjectColor(Color newColor)
    {
        currentColor = newColor;
        //objectRenderer.material.color = newColor;
        objectRenderer.material.SetColor("_Color", newColor);
    }

    // Funzione di supporto per ottenere un colore da una stringa
    Color GetColorFromString(string colorName)
    {
        switch (colorName)
        {
            case "rosso": return Color.red;
            case "verde": return Color.green;
            case "blu": return Color.blue;
            case "giallo": return Color.yellow;
            case "bianco": return Color.white;
            case "nero": return Color.black;
            case "grigio": return Color.gray;
            default: return Color.gray; // Colore di default
        }
    }
}
