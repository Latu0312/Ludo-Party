using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RoomStartManager : MonoBehaviour
{
    public Button startButton;
    public Button backButton;
    public string gameSceneName = "GameScene";

    private string roomId;
    private string userToken;

    private DatabaseReference dbRef;

    private void Awake()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += HandleEditorPlayModeChanged;
#endif
    }

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

       
        roomId = UserSession.RoomId;
        userToken = UserSession.Token;

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(userToken))
        {
            Debug.LogError("Missing roomId or userToken from static session.");
            return;
        }

       
        ListenForStartGame();

        Application.quitting += OnApplicationQuit;
        CheckIfHostAndSetupUI();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonPressed);
    }

    void CheckIfHostAndSetupUI()
    {
        dbRef.Child("rooms").Child(roomId).Child("hostToken").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogError("Failed to get hostToken");
                return;
            }

            string hostToken = task.Result.Value.ToString();
            bool isHost = hostToken == userToken;

            dbRef.Child("rooms").Child(roomId).Child("players").GetValueAsync().ContinueWith(playersTask =>
            {
                if (!playersTask.IsCompletedSuccessfully || !playersTask.Result.Exists)
                    return;

                bool isPlayerInRoom = playersTask.Result.Children.Any(child => child.Key == userToken);

                if (isHost)
                {
                    Debug.Log("You're the host");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        startButton.gameObject.SetActive(true);
                    });
                }
                else
                {
                    Debug.Log("You're a client");
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        startButton.gameObject.SetActive(false);
                    });
                }

              
                ListenForStartGame();
            });
        });
    }

    public void OnStartButtonPressedFromInspector()
    {
        OnStartButtonClicked();
    }

    public void OnStartButtonClicked()
    {
        dbRef.Child("rooms").Child(roomId).Child("players").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                var players = task.Result.Children.Select(child => child.Key).ToList();

                if (players.Count <= 4 && players.Contains(userToken))
                {
                    dbRef.Child("rooms").Child(roomId).Child("startGame").SetValueAsync(true);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        LoadingScreen.LoadScene(gameSceneName);
                    });
                }
                else
                {
                    Debug.LogWarning("Too many players or user not in room.");
                }
            }
        });
    }

    void ListenForStartGame()
    {
        Debug.Log(" Listening for startGame...");

        dbRef.Child("rooms").Child(roomId).Child("startGame").ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null || args.Snapshot == null || !args.Snapshot.Exists)
            {
                Debug.LogWarning("Error listening startGame or node missing.");
                return;
            }

            string value = args.Snapshot.Value.ToString().Trim().ToLower();
            Debug.Log($"startGame: {value}");

            if (value == "true")
            {
                Debug.Log("startGame received.");

               
                dbRef.Child("rooms").Child(roomId).Child("hostToken").GetValueAsync().ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully || !task.Result.Exists)
                    {
                        Debug.LogWarning("Could not verify host status.");
                        return;
                    }

                    string hostToken = task.Result.Value.ToString();
                    bool isHost = hostToken == userToken;

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (isHost)
                        {
                            Debug.Log("Host loading game immediately...");
                            LoadingScreen.LoadScene(gameSceneName);
                        }
                        else
                        {
                            Debug.Log("Client will load game after 5s delay...");
                            StartCoroutine(DelayLoadScene(5f));
                        }
                    });
                });
            }
        };
    }

    private IEnumerator DelayLoadScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadingScreen.LoadScene(gameSceneName);
    }

    public void OnBackButtonPressed()
    {
        dbRef.Child("rooms").Child(roomId).Child("hostToken").GetValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully || !task.Result.Exists)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.LogError("❌ Failed to retrieve hostToken, returning to mainMenu anyway.");
                    
                    LoadingScreen.LoadScene("mainMenu");
                });
                return;
            }

            string hostToken = task.Result.Value.ToString();
            bool isHost = hostToken == userToken;

            if (isHost)
            {
                dbRef.Child("rooms").Child(roomId).RemoveValueAsync().ContinueWith(_ =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Debug.Log("👑 Host exited. Room deleted.");
                        
                        LoadingScreen.LoadScene("mainMenu");
                    });
                });
            }
            else
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log("🙋 Client exited. Room preserved.");
                    
                    LoadingScreen.LoadScene("mainMenu");
                });
            }
        });
    }

    void OnApplicationQuit()
    {
        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(userToken))
            return;

        
        var hostTokenRef = dbRef.Child("rooms").Child(roomId).Child("hostToken");
        hostTokenRef.GetValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully || !task.Result.Exists)
                return;

            string hostToken = task.Result.Value.ToString();
            bool isHost = userToken == hostToken;

            if (isHost)
            {
                dbRef.Child("rooms").Child(roomId).RemoveValueAsync().ContinueWith(_ =>
                {
                    Debug.Log("❌ Application quit: Host, deleted room from Firebase.");
                });
            }
            else
            {
                Debug.Log("❌ Application quit: Client, just leaving room.");
            }
        });
    }

#if UNITY_EDITOR
    void HandleEditorPlayModeChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("🛑 PlayMode stopped. Cleared static roomId.");
            UserSession.RoomId = null;
        }
    }
#endif
}
