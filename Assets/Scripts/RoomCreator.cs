using System.Collections;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomCreator : MonoBehaviour
{
    public TMP_InputField roomNameInput;
    public TMP_InputField passwordInput;
    public GameObject errorPanel;
    public TMP_Text errorText;
    private DatabaseReference dbRef;
    private string userToken;
    private bool roomCreated = false;

    
    private string selectedGameMode = "classic";

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.GetReference("rooms");

        
        if (!string.IsNullOrEmpty(UserSession.Token))
        {
            userToken = UserSession.Token;
            Debug.Log("Token từ UserSession: " + userToken);
        }
        else
        {
            ShowError("No player token found in UserSession");
        }
    }


    
    public void CreateClassicRoom()
    {
        selectedGameMode = "classic";
        CreateRoom();
    }

   
    public void CreateFunnyRoom()
    {
        selectedGameMode = "funny";
        CreateRoom();
    }

    private void CreateRoom()
    {
        if (roomCreated) return;

        string roomName = roomNameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            ShowError("Room name cannot be empty");
            return;
        }

        if (string.IsNullOrEmpty(userToken))
        {
            ShowError("Player token missing. Please log in again!");
            return;
        }

        StartCoroutine(CreateRoomCoroutine(roomName, password, selectedGameMode));
    }

    IEnumerator CreateRoomCoroutine(string roomName, string password, string gameMode)
    {
        var getTask = dbRef.GetValueAsync();
        yield return new WaitUntil(() => getTask.IsCompleted);

        if (getTask.IsFaulted || getTask.IsCanceled)
        {
            ShowError("Cannot connect to Firebase");
            yield break;
        }

        int newRoomId = 1;
        if (getTask.Result.Exists)
        {
            foreach (var child in getTask.Result.Children)
            {
                if (int.TryParse(child.Key, out int id))
                {
                    if (id >= newRoomId)
                        newRoomId = id + 1;
                }
            }
        }

        string roomIdStr = newRoomId.ToString();

       
        RoomData roomData = new RoomData
        {
            roomId = roomIdStr,
            roomName = roomName,
            roomPassword = password,
            hostToken = userToken,
            gameMode = gameMode
        };

        
        var setTask = dbRef.Child(roomIdStr).SetRawJsonValueAsync(JsonUtility.ToJson(roomData));
        yield return new WaitUntil(() => setTask.IsCompleted);

        if (setTask.IsFaulted || setTask.IsCanceled)
        {
            ShowError("Error saving room data to Firebase");
            yield break;
        }

       
        var userNameTask = FirebaseDatabase.DefaultInstance
            .GetReference("users").Child(userToken).Child("username")
            .GetValueAsync();
        yield return new WaitUntil(() => userNameTask.IsCompleted);

        if (userNameTask.IsFaulted || !userNameTask.Result.Exists)
        {
            ShowError("Cannot get player name from account");
            yield break;
        }

        string playerName = userNameTask.Result.Value.ToString();

       
        DatabaseReference playerRef = dbRef.Child(roomIdStr).Child("players").Child(userToken);
        playerRef.Child("username").SetValueAsync(playerName);
        playerRef.Child("token").SetValueAsync(userToken);
        playerRef.Child("isHost").SetValueAsync(true); 
        UserSession.RoomId = roomIdStr;
        roomCreated = true;
        LoadingScreen.LoadScene("WaitingRoomScene");
    }

    void ShowError(string message)
    {
        if (errorPanel != null && errorText != null)
        {
            errorPanel.SetActive(true);
            errorText.text = message;
        }
        Debug.LogError(message);
    }

    void OnApplicationQuit()
    {
        StartCoroutine(DeleteRoomIfHost());
    }

    void OnDestroy()
    {
        StartCoroutine(DeleteRoomIfHost());
    }

    IEnumerator DeleteRoomIfHost()
    {
        string roomId = UserSession.RoomId; 
        string hostToken = UserSession.Token; 

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(hostToken))
            yield break;

        var hostRefTask = dbRef.Child(roomId).Child("hostToken").GetValueAsync();
        yield return new WaitUntil(() => hostRefTask.IsCompleted);

        if (hostRefTask.Result != null && hostRefTask.Result.Exists)
        {
            string tokenInFirebase = hostRefTask.Result.Value.ToString();
            if (tokenInFirebase == hostToken)
            {
                var deleteTask = dbRef.Child(roomId).RemoveValueAsync();
                yield return new WaitUntil(() => deleteTask.IsCompleted);
            }
        }
    }

    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomName;
        public string roomPassword;
        public string hostToken;
        public string gameMode;
    }
}
