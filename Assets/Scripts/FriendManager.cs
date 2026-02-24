using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SocialManager : MonoBehaviour
{
    
    public RectTransform PanelFriendChat;
    public RectTransform panelFriendList;
    public RectTransform panelFriendAdd;
    public GameObject PanelChat;
    public GameObject PanelList;
    public GameObject PanelAdd;

  
    public GameObject friendItemPrefab;       
    public GameObject addFriendItemPrefab;    
    public Transform friendListContainer;
    public Transform addFriendListContainer;
    public GameObject messagePrefab;          
    public Transform messageContainer;

  
    public TMP_InputField messageInput;       
    public TMP_InputField searchInput;       

    
    public Button findButton;                 
    public Button sendButton;                

    private DatabaseReference db;
    private FirebaseAuth auth;
    private DatabaseReference messagesRef;

    private bool chatListening = false;

    private string currentChatId;
    private string currentChatFriendToken;
    private string currentChatFriendUsername;

    private Dictionary<string, GameObject> friendItems = new Dictionary<string, GameObject>();

    void Start()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;

        ListenForFriends();

        
        if (findButton != null)
        {
            findButton.onClick.RemoveAllListeners();
            findButton.onClick.AddListener(FindPlayerByUsername);
        }

        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(SendMessage);
        }
    }

   
    private void ListenForFriends()
    {
        string userId = GetMyUserId(); 
        var friendsRef = db.Child("users").Child(userId).Child("friends");
        friendsRef.ChildAdded += OnFriendAdded;
    }

    private void OnFriendAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot == null || !e.Snapshot.Exists) return;

        string friendToken = e.Snapshot.Key;
        if (friendItems.ContainsKey(friendToken)) return;

        
        string uname = e.Snapshot.Child("username").Value?.ToString() ?? "Unknown";

     
        var go = Instantiate(friendItemPrefab, friendListContainer);
        friendItems[friendToken] = go;

       
        var nameText = go.transform.Find("UsernameText")?.GetComponent<TMP_Text>();
        var chatBtn = go.transform.Find("ChatButton")?.GetComponent<Button>();

        if (nameText) nameText.text = uname;

        if (chatBtn)
        {
            chatBtn.onClick.RemoveAllListeners();
            chatBtn.onClick.AddListener(() => OpenChat(friendToken, uname));
        }
    }

   
    public void ShowPotentialFriend(string friendToken, string friendUsername = null)
    {
        var go = Instantiate(addFriendItemPrefab, addFriendListContainer);

       
        var nameText = go.transform.Find("UsernameText")?.GetComponent<TMP_Text>();
        var addBtn = go.transform.Find("AddButton")?.GetComponent<Button>();

        if (string.IsNullOrEmpty(friendUsername))
        {
            db.Child("users").Child(friendToken).Child("username").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result != null && task.Result.Exists)
                {
                    string uname = task.Result.Value.ToString();
                    if (nameText) nameText.text = uname;

                    if (addBtn)
                    {
                        addBtn.onClick.RemoveAllListeners();
                        addBtn.onClick.AddListener(() => SendFriendRequest(friendToken, uname));
                    }
                }
            });
        }
        else
        {
            if (nameText) nameText.text = friendUsername;

            if (addBtn)
            {
                addBtn.onClick.RemoveAllListeners();
                addBtn.onClick.AddListener(() => SendFriendRequest(friendToken, friendUsername));
            }
        }
    }

    public void SendFriendRequest(string friendToken, string friendUsername)
    {
        string myId = GetMyUserId();

        db.Child("users").Child(myId).Child("username").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            string myName = "Me";
            if (task.IsCompleted && task.Result != null && task.Result.Exists)
            {
                myName = task.Result.Value.ToString();
            }

            
            var friendData = new Dictionary<string, object> { { "username", friendUsername } };
            db.Child("users").Child(myId).Child("friends").Child(friendToken).SetValueAsync(friendData);

            
            var meData = new Dictionary<string, object> { { "username", myName } };
            db.Child("users").Child(friendToken).Child("friends").Child(myId).SetValueAsync(meData);
        });
    }

    
    public void SendFriendRequestByUsername()
    {
        string searchName = searchInput.text.Trim();
        if (string.IsNullOrEmpty(searchName)) return;

        string myId = GetMyUserId();

        
        db.Child("users").Child(myId).Child("username").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            string myName = "Me";
            if (task.IsCompleted && task.Result != null && task.Result.Exists)
                myName = task.Result.Value.ToString();

            
            db.Child("users").GetValueAsync().ContinueWithOnMainThread(task2 =>
            {
                if (!(task2.IsCompleted && task2.Result != null && task2.Result.Exists))
                    return;

                var candidates = new List<DataSnapshot>();
                CollectUserNodes(task2.Result, candidates, false); 

                foreach (var userNode in candidates)
                {
                    string uid = userNode.Key;
                    string uname = userNode.Child("username").Value?.ToString() ?? "";

                    if (string.Equals(uname, searchName, System.StringComparison.OrdinalIgnoreCase) && uid != myId)
                    {
                        
                        var friendData = new Dictionary<string, object> { { "username", uname } };
                        db.Child("users").Child(myId).Child("friends").Child(uid).SetValueAsync(friendData);

                        var meData = new Dictionary<string, object> { { "username", myName } };
                        db.Child("users").Child(uid).Child("friends").Child(myId).SetValueAsync(meData);

                        Debug.Log("Friend request sent successfully!");
                        
                        ShowPotentialFriend(uid, uname);
                        break;
                    }
                }
            });
        });
    }

   
    public void FindPlayerByUsername()
    {
        string searchName = searchInput.text.Trim();
        if (string.IsNullOrEmpty(searchName)) return;

      
        foreach (Transform child in addFriendListContainer)
            Destroy(child.gameObject);

        string myId = GetMyUserId();

       
        db.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!(task.IsCompleted && task.Result != null && task.Result.Exists))
                return;

            var candidates = new List<DataSnapshot>();
            CollectUserNodes(task.Result, candidates, false);

            foreach (var userNode in candidates)
            {
                string uid = userNode.Key;
                string uname = userNode.Child("username").Value?.ToString() ?? "";

                if (string.Equals(uname, searchName, System.StringComparison.OrdinalIgnoreCase) && uid != myId)
                {
                    ShowPotentialFriend(uid, uname);
                    break;
                }
            }
        });
    }

    private HashSet<string> spawnedMessages = new HashSet<string>(); 
   
    public void OpenChat(string friendToken, string friendUsername)
    {
        currentChatFriendToken = friendToken;
        currentChatFriendUsername = friendUsername;
        currentChatId = GenerateChatId(GetMyUserId(), friendToken);

        ShowPanelFriendChat();

       
        foreach (Transform child in messageContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedMessages.Clear(); 

        messagesRef = db.Child("chatRooms").Child(currentChatId).Child("messages");

        
        if (chatListening && messagesRef != null)
        {
            messagesRef.ChildAdded -= HandleMessageAdded;
            chatListening = false;
        }

        
        messagesRef.ChildAdded += HandleMessageAdded;
        chatListening = true;

      
        messagesRef.GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.Result != null && t.Result.Exists)
            {
                foreach (var m in t.Result.Children.OrderBy(x => long.Parse(x.Child("timestamp").Value?.ToString() ?? "0")))
                    DisplayMessage(m);
            }
        });
    }

    private void HandleMessageAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot == null || !e.Snapshot.Exists) return;
        DisplayMessage(e.Snapshot);
    }

    private void DisplayMessage(DataSnapshot snapshot)
    {
        string messageId = snapshot.Key; 
        if (spawnedMessages.Contains(messageId)) return; 
        spawnedMessages.Add(messageId); 

        string senderId = snapshot.Child("senderId").Value?.ToString() ?? "";
        string text = snapshot.Child("text").Value?.ToString() ?? "";

        var go = Instantiate(messagePrefab, messageContainer);
        var txt = go.transform.Find("MessageText")?.GetComponent<TMP_Text>();
        if (txt) txt.text = "Loading...: " + text; 
        if (!string.IsNullOrEmpty(senderId))
        {
            
            db.Child("users").Child(senderId).Child("username").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                string senderName = "Unknown";
                if (task.IsCompleted && task.Result != null && task.Result.Exists)
                    senderName = task.Result.Value.ToString();

                if (txt) txt.text = senderName + ": " + text;
            });
        }
    }

    public void SendMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;

       
        string myId = GetMyUserId();
        if (string.IsNullOrEmpty(myId))
        {
            Debug.LogError("Không lấy được UID người dùng.");
            return;
        }

        
        db.Child("users").Child(myId).Child("username").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            string myName = "Me";
            if (task.IsCompleted && task.Result != null && task.Result.Exists)
            {
                myName = task.Result.Value.ToString();
            }

            var msg = new Dictionary<string, object>
            {
                { "senderId", myId },
                { "text", messageInput.text },
                { "timestamp", System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
            };

            messagesRef.Push().SetValueAsync(msg);
            messageInput.text = "";
        });
    }

    public void CloseChat()
    {
        if (messagesRef != null && chatListening)
        {
            messagesRef.ChildAdded -= HandleMessageAdded;
            chatListening = false;
        }

        currentChatFriendToken = null;
        currentChatFriendUsername = null;
        currentChatId = null;
        ShowPanelFriendList();

    }
    
    private string GenerateChatId(string userA, string userB)
    {
        return string.CompareOrdinal(userA, userB) < 0 ? userA + "_" + userB : userB + "_" + userA;
    }

    
    private string GetMyUserId()
    {
        string myId = "";

        
        if (!string.IsNullOrEmpty(UserSession.Token))
            myId = UserSession.Token;

       
        if (string.IsNullOrEmpty(myId) && auth != null && auth.CurrentUser != null)
            myId = auth.CurrentUser.UserId;

        return myId;
    }

    
    private void CollectUserNodes(DataSnapshot root, List<DataSnapshot> outList, bool insideFriends)
    {
        foreach (var child in root.Children)
        {
            bool nextInsideFriends = insideFriends || child.Key == "friends";

           
            if (!nextInsideFriends && child.HasChild("username"))
                outList.Add(child);

            
            CollectUserNodes(child, outList, nextInsideFriends);
        }
    }
    public void ShowPanelFriendList()
    {
        PanelList.SetActive(true);
        PanelFriendChat.SetAsFirstSibling(); 
        panelFriendList.SetAsLastSibling(); 
        panelFriendAdd.SetAsFirstSibling();
    }
    public void ShowPanelFriendChat()
    {
        PanelChat.SetActive(true);
        PanelList.SetActive(true);
        panelFriendList.SetAsFirstSibling(); 
        PanelFriendChat.SetAsLastSibling(); 
        panelFriendAdd.SetAsFirstSibling(); 
    }
    public void ShowPanelFriendAdd()
    {
        PanelAdd.SetActive(true);
        panelFriendList.SetAsFirstSibling(); 
        panelFriendAdd.SetAsLastSibling(); 
        PanelFriendChat.SetAsFirstSibling(); 
    }

}
