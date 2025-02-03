using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotInteractionPrompt : MonoBehaviour
{
    private Image background;
    private TMP_Text text;

    private void Start()
    {
        background = gameObject.GetComponent<Image>();
        text = gameObject.GetComponentInChildren<TMP_Text>();

        background.enabled = false;
        text.enabled = false;
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.E)) {
            background.enabled = !background.enabled;
            text.enabled = !text.enabled;
        }
         /*interazione con il bot
        if(Input.GetKeyUp(KeyCode.E) && isBotMenuOpen == false)
        {   
            background = GameObject.Find("BotCommand").GetComponent<Image>();
            text = GameObject.Find("BotCommand").GetComponentInChildren<TMP_Text>();
            if(isBotMenuOpen){
                background.enabled = false;
                text.enabled = false;
                isBotMenuOpen = false;
            }else{
                background.enabled = true;
                text.enabled = true;
                isBotMenuOpen = true;
            }*/
            /*
            if(!isNextBot){
                background.enabled = !background.enabled;
                text.enabled = !text.enabled;
                //bot = GetComponentInChildren<BotInteractionPrompt>().gameObject;
                //bot.GetComponent<BotInteractionPrompt>().OpenMenu(); 
            }else{
                background.enabled = !background.enabled;
                text.enabled = !text.enabled;
                //bot.GetComponent<BotInteractionPrompt>().OpenMenu();  
            }
            isBotMenuOpen = true;*/
    }
       

}
