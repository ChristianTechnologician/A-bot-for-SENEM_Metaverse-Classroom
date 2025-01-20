using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferManager : MonoBehaviour
{
    private Queue<short[]> bufferQueue;
    private int bufferSize = 512; // Dimensione aumentata
    private bool isProcessing = false;

    void Start()
    {
        Debug.Log("BufferManager: Inizializzazione completata.");
        bufferQueue = new Queue<short[]>(bufferSize); // Inizializzazione con dimensione fissa
        StartCoroutine(ProcessBuffer());
    }

    public void AddToBuffer(short[] data)
    {
        // Rimosso controllo per dato vuoto o null

        if (bufferQueue.Count >= bufferSize)
        {
            Debug.LogWarning("BufferManager: Buffer pieno. Rimuovo il dato più vecchio.");
            bufferQueue.Dequeue(); // Rimuove il dato più vecchio se il buffer è pieno
        }

        bufferQueue.Enqueue(data);
        Debug.Log("BufferManager: Dato aggiunto al buffer.");
    }

    IEnumerator ProcessBuffer()
    {
        while (true)
        {
            if (bufferQueue.Count > 0 && !isProcessing)
            {
                isProcessing = true;
                short[] data = bufferQueue.Dequeue();
                /* Processo il dato
                for (int i = 0; i < data.Length; i++)
                {
                    Debug.Log("BufferManager: Processando dato: " + data[i]);
                }*/

                Debug.Log("BufferManager: Buffer processato e svuotato.");
                isProcessing = false;
            }
            yield return null; // Attendere un frame prima di ripetere
        }
    }
}
