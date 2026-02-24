using Firebase.Database;
using TMPro;
using UnityEngine;

public class PlayerListManager : MonoBehaviour
{
    public Transform playerListContainer;       
    public GameObject playerItemPrefab;         
    private DatabaseReference playersRef;

    void Start()
    {
       
        string roomId = UserSession.RoomId;

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("Không tìm thấy roomId trong UserSession!");
            return;
        }

        playersRef = FirebaseDatabase.DefaultInstance
            .GetReference("rooms")
            .Child(roomId)
            .Child("players");

        playersRef.ValueChanged += OnPlayersChanged;
    }

    private void OnPlayersChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Firebase error: " + args.DatabaseError.Message);
            return;
        }

       
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

       
        foreach (var playerSnapshot in args.Snapshot.Children)
        {
            string username = playerSnapshot.Child("username").Value?.ToString();
            if (!string.IsNullOrEmpty(username))
            {
                GameObject item = Instantiate(playerItemPrefab, playerListContainer);
                item.GetComponentInChildren<TextMeshProUGUI>().text = username;
            }
        }
    }

    private void OnDestroy()
    {
        if (playersRef != null)
            playersRef.ValueChanged -= OnPlayersChanged;
    }
}
