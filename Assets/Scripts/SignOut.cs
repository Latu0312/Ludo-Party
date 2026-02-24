using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;
    public bool rememberMe = false;

    private string folderPath;
    private string filePath;

#if UNITY_EDITOR
    private string editorPath = @"C:\Users\latu0\Desktop\editorTest\userToken.txt";
#endif

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        folderPath = Path.Combine(Application.persistentDataPath, "config");
        filePath = Path.Combine(folderPath, "userToken.txt");

        rememberMe = PlayerPrefs.GetInt("rememberMe", 0) == 1;

        CheckAutoLogin();

        Application.quitting += OnApplicationQuit;
    }

    public void SaveToken(string token)
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        File.WriteAllText(filePath, token);
        PlayerPrefs.SetString("userToken", token);
        PlayerPrefs.Save();
    }

    void CheckAutoLogin()
    {
        if (rememberMe && File.Exists(filePath))
        {
            string token = File.ReadAllText(filePath).Trim();
            if (!string.IsNullOrEmpty(token))
            {
                PlayerPrefs.SetString("userToken", token);
                PlayerPrefs.Save();
                SceneManager.LoadScene("mainMenu");
            }
        }
    }

    public void OnClickRememberMe()
    {
        rememberMe = !rememberMe;
        PlayerPrefs.SetInt("rememberMe", rememberMe ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SignOutNow()
    {
        Debug.Log("Gọi SignOutNow từ button...");
        StartCoroutine(HandleLogoutOrExit(false));
    }

    private void OnApplicationQuit()
    {
        TryDeleteTokenFile("OnApplicationQuit");
    }

    private void OnDestroy()
    {
        TryDeleteTokenFile("OnDestroy");
    }

    private void TryDeleteTokenFile(string context)
    {
#if UNITY_EDITOR
        
        if (File.Exists(editorPath))
        {
            try
            {
                File.Delete(editorPath);
                Debug.Log($"[EDITOR] Đã xóa file token ở {editorPath} trong {context}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EDITOR] Lỗi khi xóa file trong {context}: {e.Message}");
            }
        }
#else
        // 🔹 Xóa file token ở build path
        if (!rememberMe && File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Đã xóa file token trong {context} vì không ghi nhớ.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi khi xóa file trong {context}: {e.Message}");
            }
        }
#endif
    }

    public IEnumerator HandleLogoutOrExit(bool isExit)
    {
        Debug.Log("Bắt đầu xử lý đăng xuất hoặc thoát game...");

#if UNITY_EDITOR
       
        if (File.Exists(editorPath))
        {
            try
            {
                File.Delete(editorPath);
                Debug.Log("[EDITOR] Đã xóa file userToken.txt");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EDITOR] Lỗi khi xóa file userToken.txt: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[EDITOR] Không tìm thấy file userToken.txt để xóa.");
        }
#else
        // 🔹 Xóa file token trong Build
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log("Đã xóa file userToken.txt");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi khi xóa file userToken.txt: {e.Message}");
            }
        }
        else
        {
            Debug.Log("Không tìm thấy file userToken.txt để xóa.");
        }
#endif

       
        PlayerPrefs.DeleteKey("userToken");
        PlayerPrefs.DeleteKey("rememberMe");
        PlayerPrefs.Save();
        Debug.Log("Đã xóa PlayerPrefs");

        yield return new WaitForSeconds(0.2f);

        if (isExit)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
        {
            Debug.Log("Chuyển về LoginScene...");
            SceneManager.LoadScene("LoginScene");
        }
    }
}
