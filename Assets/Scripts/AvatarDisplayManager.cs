using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class AvatarDisplayManager : MonoBehaviour
{
    
   
    public Image avatarImage;      
    public Text playerNameText;      
    public Sprite[] avatarSprites;   
    [SerializeField] private Sprite defaultAvatar;    

    void Start()
    {
        
        int savedIndex = PlayerPrefs.GetInt("SelectedAvatar", -1);
        if (savedIndex >= 0 && savedIndex < avatarSprites.Length)
        {
            avatarImage.sprite = avatarSprites[savedIndex];
        }
        else if (avatarSprites.Length > 0)
        {
            avatarImage.sprite = avatarSprites[0]; 
        }
    }

   
    public void Setup(PlayerRef? player, int avatarIndex = -1, string playerName = "")
    {
        if (player == null)
        {
            
            avatarImage.sprite = defaultAvatar;
            if (playerNameText != null) playerNameText.text = "";
            return;
        }

       
        if (avatarIndex >= 0 && avatarIndex < avatarSprites.Length)
        {
            avatarImage.sprite = avatarSprites[avatarIndex];
        }
        else
        {
            avatarImage.sprite = defaultAvatar; 
        }

        if (playerNameText != null)
        {
            playerNameText.text = string.IsNullOrEmpty(playerName) ? $"Player {player.Value.PlayerId}" : playerName;
        }
    }

}
