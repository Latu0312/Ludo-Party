using UnityEngine;
using TMPro;

public class ChatMessageUI : MonoBehaviour
{
   
    public TMP_Text messageText;

    public void SetText(string content)
    {
        if (messageText) messageText.text = content;
    }
}
