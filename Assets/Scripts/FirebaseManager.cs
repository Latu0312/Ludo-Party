using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    FirebaseApp app;
    FirebaseAuth auth;
    DatabaseReference database;
    FirebaseUser user;
    CanvasManager CanvasManager; 

    public GameObject errorPanel;
    public GameObject createErrorPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (CanvasManager == null)
        {
            CanvasManager = GameObject.FindFirstObjectByType<CanvasManager>();
        }
    }

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                database = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    
    public void SignIn(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Đăng nhập thất bại: " + task.Exception?.Flatten().InnerException?.Message);
                if (errorPanel != null)
                    errorPanel.SetActive(true);
                StartCoroutine(HideErrol());
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            user = result.User;

            if (user != null)
            {
                string token = user.UserId;

              
                UserSession.Token = token;

                Debug.Log("Đăng nhập thành công. Token: " + token);

              
                LoadingScreen.LoadScene("mainMenu");

            }
        });
    }

   
    public void CreateUser(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Tạo tài khoản thất bại: " + task.Exception?.Flatten().InnerException?.Message);
                if (createErrorPanel != null)
                    createErrorPanel.SetActive(true);
                StartCoroutine(HideErrol());
                return;
            }

            FirebaseUser newUser = task.Result?.User;

            if (newUser != null)
            {
                string userId = newUser.UserId;

              
                UserSession.Token = userId;

                Debug.Log("Đăng ký thành công. Token: " + userId);

                
                StartCoroutine(SaveTokenAndProceed(userId));
            }
            else
            {
                Debug.LogError("Người dùng tạo mới là null!");
            }
            CanvasManager.SwitchPanelByIndex(4);
        });
    }

   
    private IEnumerator SaveTokenAndProceed(string userId)
    {
        if (database != null)
        {
            PlayerData newUserData = new PlayerData();
            string json = JsonUtility.ToJson(newUserData);

            var dataTask = database.Child("users").Child(userId).SetRawJsonValueAsync(json);
            yield return new WaitUntil(() => dataTask.IsCompleted);

            if (dataTask.IsFaulted)
            {
                Debug.LogError("Lỗi khi lưu dữ liệu mặc định: " + dataTask.Exception?.Flatten().Message);
                yield break;
            }

            Debug.Log("Đã khởi tạo dữ liệu mặc định cho người dùng.");
            CanvasManager.SwitchPanelByIndex(4);
        }
        else
        {
            Debug.LogError("Database chưa được khởi tạo. Kiểm tra lại Firebase Initialization.");
        }
    }

    
    public void SignOut()
    {
        auth.SignOut();
        UserSession.Token = null; 
        Debug.Log("Đã đăng xuất");
    }

    public void DetectAcc()
    {
        user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
            Debug.Log("Đang đăng nhập với: " + user.Email);
        else
            Debug.Log("Chưa có ai đăng nhập.");
    }

    public void PassReset(string email)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Gửi email khôi phục thất bại: " + task.Exception?.Message);
                return;
            }

            Debug.Log("Đã gửi email khôi phục.");
        });
    }

    private IEnumerator HideErrol()
    {
        yield return new WaitForSeconds(2f);
        if (errorPanel != null)
            errorPanel.SetActive(false);
        if (createErrorPanel != null)
            createErrorPanel.SetActive(false);
    }

   
    public string GetUserToken()
    {
        return UserSession.Token;
    }
}
