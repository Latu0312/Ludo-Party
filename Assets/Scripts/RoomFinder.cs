using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomFinder : MonoBehaviour
{
    public Transform roomListContainer;
    public GameObject roomItemPrefab;
    public TMP_InputField searchRoomInput;
    public GameObject errorPanel;
    public TMP_Text errorText;
    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.GetReference("rooms");
        LoadAllRooms(); 
    }


    public void LoadAllRooms()
    {
        ClearRoomList();
        StartCoroutine(FetchRoomsCoroutine());
    }

    public void SearchRoomByName()
    {
        string keyword = searchRoomInput.text.Trim();
        if (string.IsNullOrEmpty(keyword))
        {
            ShowError("Please enter the room name to search");
            return;
        }

        ClearRoomList();
        StartCoroutine(FetchRoomsCoroutine(keyword));
    }

    IEnumerator FetchRoomsCoroutine(string searchKeyword = "")
    {
        var dbTask = dbRef.GetValueAsync();
        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null || dbTask.Result == null)
        {
            ShowError("Cannot connect to Firebase");
            yield break;
        }

        DataSnapshot snapshot = dbTask.Result;
        bool foundAny = false;

        foreach (var child in snapshot.Children)
        {
            RoomData room = JsonUtility.FromJson<RoomData>(child.GetRawJsonValue());

            if (!string.IsNullOrEmpty(searchKeyword) &&
                !room.roomName.ToLower().Contains(searchKeyword.ToLower()))
                continue;

            GameObject roomItem = Instantiate(roomItemPrefab, roomListContainer);
            RoomItemUI itemUI = roomItem.GetComponent<RoomItemUI>();
            itemUI.SetRoomInfo(room);

            foundAny = true;
        }

        if (!foundAny)
        {
            ShowError("No matching room found");
        }
        else
        {
            errorPanel.SetActive(false);
        }
    }

    void ClearRoomList()
    {
        foreach (Transform child in roomListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void ShowError(string message)
    {
        if (errorPanel && errorText)
        {
            errorText.text = message;
            errorPanel.SetActive(true);
        }
    }

    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomName;
        public string roomPassword;
        public string hostToken;
    }
}
