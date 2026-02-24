using UnityEngine;
using Firebase.Database;
using System.Collections;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

   
    public GameplayMode selectedGameMode = GameplayMode.Classic;

    public GameObject HintPanelFunnyMode;

    private DatabaseReference dbRef;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        
        StartCoroutine(LoadRoomGameMode());
    }

    private IEnumerator LoadRoomGameMode()
    {
        
        string roomId = UserSession.RoomId;

        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogError("Không tìm thấy roomId trong UserSession!");
            yield break;
        }

        Debug.Log("Đã lấy roomId từ UserSession: " + roomId);

        
        var getTask = dbRef.Child("rooms").Child(roomId).Child("gameMode").GetValueAsync();
        yield return new WaitUntil(() => getTask.IsCompleted);

        if (getTask.IsFaulted || getTask.Result == null || !getTask.Result.Exists)
        {
            Debug.LogError("Không thể lấy gameMode từ Firebase!");
            yield break;
        }

        string modeStr = getTask.Result.Value.ToString();
        Debug.Log("Game mode trên Firebase: " + modeStr);

       
        if (modeStr == "funny")
            selectedGameMode = GameplayMode.Funny;
        else
            selectedGameMode = GameplayMode.Classic;
    }

    public bool IsClassicMode()
    {
        HintPanelFunnyMode.SetActive(false); 
        return selectedGameMode == GameplayMode.Classic;
    }

    public bool IsFunnyMode()
    {
        HintPanelFunnyMode.SetActive(true); 
        return selectedGameMode == GameplayMode.Funny;
    }
}

public enum GameplayMode
{
    Classic, 
    Funny    
}
