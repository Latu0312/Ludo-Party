using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddFriendItemUI : MonoBehaviour
{

    public TMP_Text usernameText;
    public Button addButton;

    public void Bind(string targetUid, string targetUsername, System.Action onAddClicked)
    {
        if (usernameText) usernameText.text = targetUsername;
        if (addButton)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(() => onAddClicked?.Invoke());
        }
    }
}