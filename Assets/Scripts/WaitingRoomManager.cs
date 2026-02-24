using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WaitingRoomHost : MonoBehaviour
{
    private DatabaseReference dbRef;
    private string gameRoomID;
    private string playerToken;
    private string playerName;
    private bool isHost = false;
    private bool gameStarted = false;

    private DatabaseReference playersRef;
    private DatabaseReference startRef;

    public TextMeshProUGUI countdownText;
    public GameObject startButton;

    public Transform playerListContainer;
    public GameObject playerItemPrefab;

    private string configPath => Path.Combine(Application.persistentDataPath, "config", "roomId.txt");
    private string tokenPath => Path.Combine(Application.persistentDataPath, "userToken.txt");

    IEnumerator Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

       
        while (!File.Exists(tokenPath))
            yield return null;

        playerToken = File.ReadAllText(tokenPath);

     
        if (File.Exists(configPath))
        {
            gameRoomID = File.ReadAllText(configPath).Trim();
        }
        else
        {
            Debug.LogError("Không tìm thấy file roomId.txt");
            yield break;
        }

        JoinRoom();
        ListenForPlayerChanges();
        ListenForStartGame();
    }

    private void JoinRoom()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").Child(playerToken).Child("username")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result != null)
                {
                    playerName = task.Result.Value.ToString();

                    dbRef.Child("rooms").Child(gameRoomID).Child("hostToken")
                        .GetValueAsync().ContinueWithOnMainThread(hostTask =>
                        {
                            if (hostTask.IsCompleted && hostTask.Result != null)
                            {
                                string hostToken = hostTask.Result.Value.ToString();
                                isHost = (hostToken == playerToken);

                                var playerRef = dbRef.Child("rooms").Child(gameRoomID).Child("players").Child(playerToken);
                                playerRef.Child("username").SetValueAsync(playerName);
                                playerRef.Child("token").SetValueAsync(playerToken);
                                playerRef.Child("isHost").SetValueAsync(isHost);

                                startButton.SetActive(isHost); 
                            }
                        });
                }
            });
    }

    private void ListenForPlayerChanges()
    {
        playersRef = dbRef.Child("rooms").Child(gameRoomID).Child("players");

        playersRef.ChildAdded += (sender, args) =>
        {
            string token = args.Snapshot.Key;
            string username = args.Snapshot.Child("username").Value?.ToString();
            bool isPlayerHost = args.Snapshot.Child("isHost").Value?.ToString() == "true";

            if (!string.IsNullOrEmpty(username))
            {
                GameObject playerItem = Instantiate(playerItemPrefab, playerListContainer);
                playerItem.name = "Player_" + token;

                var text = playerItem.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = username + (isPlayerHost ? " (Chủ phòng)" : "");
            }
        };

        playersRef.ChildRemoved += (sender, args) =>
        {
            string token = args.Snapshot.Key;
            Transform toRemove = playerListContainer.Find("Player_" + token);
            if (toRemove != null)
                Destroy(toRemove.gameObject);
        };
    }

    private void ListenForStartGame()
    {
        startRef = dbRef.Child("rooms").Child(gameRoomID).Child("startGame");

        startRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null) return;

            if (args.Snapshot != null && args.Snapshot.Exists && args.Snapshot.Value.ToString() == "true" && !gameStarted)
            {
                gameStarted = true;
               
                LoadingScreen.LoadScene("New Scene");
            }
        };
    }

    public void OnStartGameButtonClicked()
    {
        if (isHost && !gameStarted)
        {
            gameStarted = true;
            dbRef.Child("rooms").Child(gameRoomID).Child("startGame").SetValueAsync(true);
        }
    }

    public void OnBackButtonPressed()
    {
        LeaveRoom();
      
        LoadingScreen.LoadScene("mainMenu");
    }

    private void LeaveRoom()
    {
        if (string.IsNullOrEmpty(playerToken) || string.IsNullOrEmpty(gameRoomID))
            return;

        var roomRef = dbRef.Child("rooms").Child(gameRoomID);

     
        if (isHost)
        {
            roomRef.RemoveValueAsync();
        }
        else
        {
            roomRef.Child("players").Child(playerToken).RemoveValueAsync();
        }

      
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
            Debug.Log(" Đã xóa file roomId.txt");
        }
    }

    private void OnApplicationQuit()
    {
        LeaveRoom(); 
    }

    private void OnDestroy()
    {
        LeaveRoom();
        if (playersRef != null) playersRef.ValueChanged -= null;
        if (startRef != null) startRef.ValueChanged -= null;
    }
}
