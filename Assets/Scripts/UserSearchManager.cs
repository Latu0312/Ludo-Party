using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;

public class UserSearchManager : MonoBehaviour
{
    
    public TMP_InputField usernameInputField;
    public GameObject resultPanel;
    public TMP_Text usernameText;
    public TMP_Text experienceText;

    private DatabaseReference dbRef;
    private string myUserId;

   
    private string pendingUsername = "";
    private string pendingExperience = "";
    private bool showResultPanel = false;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        myUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        resultPanel.SetActive(false);
    }

    void Update()
    {
        
        if (showResultPanel)
        {
            resultPanel.SetActive(true);
            usernameText.text = $"Tên: {pendingUsername}";
            experienceText.text = $"Kinh nghiệm: {pendingExperience}";
            showResultPanel = false;
        }
    }

    public void OnSearchButtonPressed()
    {
        string searchUsername = usernameInputField.text.Trim();
        resultPanel.SetActive(false);

        if (string.IsNullOrEmpty(searchUsername))
        {
            Debug.Log("Bạn chưa nhập tên người chơi.");
            return;
        }

        dbRef.Child("users")
            .OrderByChild("username")
            .EqualTo(searchUsername)
            .GetValueAsync()
            .ContinueWith(task =>
            {
                if (task.IsFaulted || task.Exception != null)
                {
                    Debug.LogError("Lỗi khi tìm kiếm Firebase.");
                    return;
                }

                if (task.Result.Exists)
                {
                    foreach (var userSnapshot in task.Result.Children)
                    {
                        string foundUserId = userSnapshot.Key;

                        if (foundUserId == myUserId)
                        {
                            Debug.Log("Bạn không thể tìm chính mình.");
                            return;
                        }

                        string foundUsername = userSnapshot.Child("username").Value?.ToString() ?? "Không rõ";
                        string experience = userSnapshot.Child("experience").Value?.ToString() ?? "0";

                        
                        pendingUsername = foundUsername;
                        pendingExperience = experience;
                        showResultPanel = true;

                        return;
                    }
                }
                else
                {
                    Debug.Log("Không tìm thấy người chơi.");
                }
            });
    }
}
