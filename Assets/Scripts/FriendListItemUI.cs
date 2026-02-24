using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendListItemUI : MonoBehaviour
{
   
    public TMP_Text usernameText;
    public Button chatButton;
    public void Bind(string friendUid, string friendUsername, System.Action onChatClicked)
    {
        if (usernameText) usernameText.text = friendUsername;
        if (chatButton)
        {
            chatButton.onClick.RemoveAllListeners();
            chatButton.onClick.AddListener(() => onChatClicked?.Invoke());
        }

    }
}