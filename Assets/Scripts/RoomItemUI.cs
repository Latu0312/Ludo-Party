using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class RoomItemUI : MonoBehaviour
{
   
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public TMP_Text passwordStatusText;
    public Button joinButton;

   
    public GameObject passwordPanel;
    public TMP_InputField passwordInputField;
    public Button confirmJoinButton;
    public TMP_Text errorText;

    private string roomId;
    private string roomPassword;

    private void Awake()
    {
        joinButton.onClick.AddListener(OnJoinClicked);
        confirmJoinButton.onClick.AddListener(OnConfirmPassword);
        passwordPanel.SetActive(false);
        errorText.text = "";
    }

    public void SetRoomInfo(RoomFinder.RoomData data)
    {
        roomId = data.roomId;
        roomPassword = data.roomPassword;

        roomNameText.text = $"Room name: {data.roomName}";
        passwordStatusText.text = string.IsNullOrEmpty(roomPassword) ? "No Password" : "Password";

        FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomId).Child("players")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    int count = (int)task.Result.ChildrenCount;
                    playerCountText.text = $"Players: {count} / 4";
                }
                else
                {
                    playerCountText.text = $"Players: 0 / 4";
                }
            });
    }

    private void OnJoinClicked()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomId).Child("players")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    int count = (int)task.Result.ChildrenCount;
                    if (count >= 4)
                    {
                        errorText.text = "Phòng đã đủ người!";
                        return;
                    }
                    ProceedJoin();
                }
                else
                {
                    ProceedJoin();
                }
            });
    }

    private void ProceedJoin()
    {
        if (string.IsNullOrEmpty(roomPassword))
        {
            JoinRoom();
        }
        else
        {
            passwordPanel.SetActive(true);
            passwordInputField.text = "";
            errorText.text = "";
        }
    }

    private void OnConfirmPassword()
    {
        if (passwordInputField.text.Trim() == roomPassword)
        {
            passwordPanel.SetActive(false);
            JoinRoom();
        }
        else
        {
            errorText.text = "Sai mật khẩu!";
        }
    }

    public void ClosePasswordPanel()
    {
        passwordPanel.SetActive(false);
        errorText.text = "";
    }

    public void JoinRoom()
    {
        
        string token = UserSession.Token;

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Không tìm thấy token trong UserSession.");
            return;
        }
        UserSession.RoomId = roomId;
        FirebaseDatabase.DefaultInstance
            .GetReference("users").Child(token).Child("username")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || !task.Result.Exists)
                {
                    Debug.LogError("Không thể lấy tên người chơi từ users.");
                    return;
                }
                string playerName = task.Result.Value.ToString();
                DatabaseReference playerRef = FirebaseDatabase.DefaultInstance
                    .GetReference("rooms").Child(roomId).Child("players").Child(token);
                playerRef.Child("username").SetValueAsync(playerName);
                playerRef.Child("isHost").SetValueAsync(false);
                playerRef.Child("token").SetValueAsync(token);
                Debug.Log("Người chơi đã join phòng, Token & RoomId được lưu trong UserSession.");
                LoadingScreen.LoadScene("WaitingRoomScene");
            });
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public string name;
        public bool isHost;

        public PlayerInfo(string name, bool isHost)
        {
            this.name = name;
            this.isHost = isHost;
        }
    }
}
