using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameListener : MonoBehaviour
{
    private string roomId;

    void Start()
    {
        
        roomId = UserSession.RoomId;

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("RoomSession.RoomId is null or empty!");
            return;
        }

       
        FirebaseDatabase.DefaultInstance
            .GetReference("rooms")
            .Child(roomId)
            .Child("startGame")
            .ValueChanged += OnStartGameChanged;

        Debug.Log($"[{roomId}] Listening for startGame...");
    }

    private void OnStartGameChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot != null && args.Snapshot.Exists && args.Snapshot.Value.ToString() == "true")
        {
            Debug.Log("startGame changed: True");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("Loading game scene");
                SceneManager.LoadScene("GameScene");
            });
        }
    }
}
