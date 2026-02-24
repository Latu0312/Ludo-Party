using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseLoginManager : MonoBehaviour
{
    
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    private FirebaseAuth auth;
    
    public GameObject errorPanel;
    

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
        });

        if (errorPanel != null) errorPanel.SetActive(false);
    }
   

    public void OnLoginButtonPressed()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Vui lòng nhập đầy đủ Email và Mật khẩu.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                ShowError("Đăng nhập thất bại. Kiểm tra lại thông tin.");
            }
            else if (task.IsCompleted)
            {
                FirebaseUser user = task.Result.User;
                string token = user.UserId;

                PlayerPrefs.SetString("token", token);
                PlayerPrefs.Save();

                Debug.Log("Đăng nhập thành công. Token: " + token);

                SceneManager.LoadScene("NhapTenScene"); 
            }
        });
    }

    void ShowError(string message)
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(true);
        }

        if (errorText != null)
        {
            errorText.text = message;
        }

        Debug.LogError(message);
    }

   
}
