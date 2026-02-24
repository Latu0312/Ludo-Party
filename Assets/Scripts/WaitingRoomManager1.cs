using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WaitingRoomClient : MonoBehaviour
{
    private DatabaseReference dbRef;
    private string gameRoomID;
    private string playerToken;

    public TextMeshProUGUI playerListText;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        playerToken = System.IO.File.ReadAllText(Application.persistentDataPath + "/usertoken.txt");
        gameRoomID = System.IO.File.ReadAllText(Application.persistentDataPath + "/config.txt");

        JoinRoomAsClient();
        ListenForPlayerChanges(); 
        ListenForStartGame();     
    }

    private void JoinRoomAsClient()
    {
        dbRef.Child("users").Child(playerToken).Child("name").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result != null)
            {
                string playerName = task.Result.Value.ToString();
                dbRef.Child("rooms").Child(gameRoomID).Child("players").Child(playerToken).Child("name").SetValueAsync(playerName);
                dbRef.Child("rooms").Child(gameRoomID).Child("players").Child(playerToken).Child("isHost").SetValueAsync(false);
            }
        });
    }

    private void ListenForPlayerChanges()
    {
        dbRef.Child("rooms").Child(gameRoomID).Child("players").ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null) return;

            string displayText = "Danh sách người chơi:\n";
            foreach (var child in args.Snapshot.Children)
            {
                if (child.Child("name").Value != null)
                {
                    displayText += "- " + child.Child("name").Value.ToString() + "\n";
                }
            }

            Debug.Log(displayText);
            playerListText.text = displayText;
        };
    }

    private void ListenForStartGame()
    {
        dbRef.Child("rooms").Child(gameRoomID).Child("startGame").ValueChanged += (sender, args) =>
        {
            if (args.Snapshot != null && args.Snapshot.Exists && args.Snapshot.Value.ToString() == "true")
            {
                SceneManager.LoadScene("New Scene");
            }
        };
    }

    private void OnDestroy()
    {
        dbRef.Child("rooms").Child(gameRoomID).Child("players").Child(playerToken).RemoveValueAsync();
    }
}
