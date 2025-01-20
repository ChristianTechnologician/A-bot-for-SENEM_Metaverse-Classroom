using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic; // Aggiungi questo

public class TranscriptionSync : MonoBehaviourPunCallbacks
{
    public TMP_Text ResultText;
    private string transcriptionResult = "";
    private PhotonView transcriptionPhotonView;
    private readonly ConcurrentQueue<string> transcriptionQueue = new ConcurrentQueue<string>();

    private void Start()
    {
        transcriptionPhotonView = GetComponent<PhotonView>();

        if (ResultText == null)
        {
            ResultText = GameObject.Find("TranscriptionText").GetComponent<TMP_Text>();
        }

        Debug.Log("TranscriptionSync: Inizializzazione completata.");
    }

    private void Update()
    {
        if (transcriptionQueue.TryDequeue(out string result))
        {
            UnityMainThreadDispatcher.Initialize();
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (ResultText != null)
                {
                    ResultText.text +=  result + "\n";
                    Debug.Log("TranscriptionSync: Text updated to " + result);
                }
                else
                {
                    Debug.LogWarning("ResultText is null.");
                }
            });
        }
    }

    public void OnTranscriptionResult(string result)
    {
        transcriptionResult = result;

        if (PhotonNetwork.InRoom)
        {
            transcriptionPhotonView.RPC("SendTranscriptionRpc", RpcTarget.All, result);
            Debug.Log("TranscriptionSync: RPC sent with result " + result);
        }
        else if (transcriptionPhotonView.IsMine)
        {
            SendTranscriptionRpc(result);
            Debug.LogWarning("TranscriptionSync: Offline mode. Transcription visible only locally.");
        }
    }

    [PunRPC]
    public void SendTranscriptionRpc(string result)
    {
        transcriptionResult = result; 
        // Modifica il formato del messaggio qui 
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss"); 
        string message = $"<color=\"yellow\">{PhotonNetwork.NickName}</color> [{timestamp}]: {result}";
        
        transcriptionQueue.Enqueue(message);
        Debug.Log("TranscriptionSync: Received RPC with result " + result);
        LogManager.Instance.LogInfo($"{PhotonNetwork.NickName} wrote in the chat at {timestamp}: \"{result}\"");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!transcriptionPhotonView.IsMine) return;
        transcriptionPhotonView.RPC("SendTranscriptionRpc", RpcTarget.AllBuffered, transcriptionResult);
        Debug.Log("TranscriptionSync: Sent buffered RPC with result " + transcriptionResult);
    }
}

