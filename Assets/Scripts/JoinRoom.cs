using Firebase.Database;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JoinRoomManager : MonoBehaviour
{
    
    public DatabaseReference dbReference;

    
    public TMP_InputField playerNameInput;
    public TMP_InputField passwordInputField;
    public GameObject passwordPanel;
    public Button confirmJoinButton;
    public GameObject roomListContent;
    public GameObject roomItemPrefab;

    private string selectedRoomId = "";
    private string selectedRoomPassword = "";
    private bool selectedRoomHasPassword = false;

    private void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadRooms();
        passwordPanel.SetActive(false);
        confirmJoinButton.onClick.AddListener(ValidatePasswordAndJoin);
    }

    void LoadRooms()
    {
        FirebaseDatabase.DefaultInstance.GetReference("rooms").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                foreach (Transform child in roomListContent.transform)
                    Destroy(child.gameObject);

                DataSnapshot snapshot = task.Result;

                foreach (var room in snapshot.Children)
                {
                    var data = room.Value as Dictionary<string, object>;
                    if (data == null || !data.ContainsKey("roomName")) continue;

                    string roomId = room.Key;
                    string roomName = data["roomName"].ToString();
                    string roomPassword = data.ContainsKey("password") ? data["password"].ToString() : "";

                    RoomFinder.RoomData roomData = new RoomFinder.RoomData
                    {
                        roomId = roomId,
                        roomName = roomName,
                        roomPassword = roomPassword
                    };

                    GameObject item = Instantiate(roomItemPrefab, roomListContent.transform);
                    RoomItemUI itemUI = item.GetComponent<RoomItemUI>();
                    itemUI.SetRoomInfo(roomData);

                   
                    Button joinBtn = item.GetComponent<Button>();
                    joinBtn.onClick.AddListener(() => OnClickRoom(roomData));
                }
            }
        });
    }

    void OnClickRoom(RoomFinder.RoomData roomData)
    {
        selectedRoomId = roomData.roomId;
        selectedRoomPassword = roomData.roomPassword;
        selectedRoomHasPassword = !string.IsNullOrEmpty(selectedRoomPassword);

        if (selectedRoomHasPassword)
        {
            passwordPanel.SetActive(true); 
        }
        else
        {
            JoinRoom(); 
        }
    }

    void ValidatePasswordAndJoin()
    {
        string enteredPassword = passwordInputField.text;

        if (enteredPassword == selectedRoomPassword)
        {
            passwordPanel.SetActive(false);
            JoinRoom();
        }
        else
        {
            Debug.LogWarning("Sai mật khẩu!");
        }
    }

    void JoinRoom()
    {
        string playerName = playerNameInput.text;
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Bạn chưa nhập tên người chơi!");
            return;
        }

        string playerId = System.Guid.NewGuid().ToString();

        JoinPlayerInfo playerInfo = new JoinPlayerInfo
        {
            playerId = playerId,
            playerName = playerName
        };

        string json = JsonUtility.ToJson(playerInfo);
        dbReference.Child("rooms").Child(selectedRoomId).Child("players").Child(playerId).SetRawJsonValueAsync(json);

        PlayerPrefs.SetString("playerName", playerName);
        PlayerPrefs.SetString("roomId", selectedRoomId);
        PlayerPrefs.SetString("playerId", playerId);

        SceneManager.LoadScene("WaitingRoomScene");
    }

    [System.Serializable]
    public class JoinPlayerInfo
    {
        public string playerId;
        public string playerName;
    }
}
