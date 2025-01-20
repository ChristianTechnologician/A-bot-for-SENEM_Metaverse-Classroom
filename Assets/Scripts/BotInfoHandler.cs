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
        if(Input.GetKeyUp(KeyCode.E)) {
            background.enabled = !background.enabled;
            text.enabled = !text.enabled;
        }
    }
}

