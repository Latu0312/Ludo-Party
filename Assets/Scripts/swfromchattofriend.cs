using UnityEngine;

public class chatManager : MonoBehaviour
{
    
    public GameObject chatPanel;
    public GameObject friendListPanel;

  
    public void SwitchToFriendList()
    {
        if (chatPanel != null) chatPanel.SetActive(false);
        if (friendListPanel != null) friendListPanel.SetActive(true);
    }

  
    public void SwitchToChat()
    {
        if (friendListPanel != null) friendListPanel.SetActive(false);
        if (chatPanel != null) chatPanel.SetActive(true);
    }
}