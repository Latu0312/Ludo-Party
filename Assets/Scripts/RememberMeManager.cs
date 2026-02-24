using UnityEngine;
using System.IO;

public class RememberMeManager : MonoBehaviour
{
    public static RememberMeManager Instance;

    private const string RememberMeKey = "RememberMe";
    private string filePath;

    void Awake()
    {
     
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "usertoken.txt");
        }
        else
        {
            Destroy(gameObject);
        }
    }

   
    public void OnRememberMeButtonClicked()
    {
        PlayerPrefs.SetInt(RememberMeKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Ghi nhớ người chơi!");
    }

   
    public void ClearRememberMeAndToken()
    {
        if (!IsRemembered())
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("Đã xóa usertoken.txt vì không ghi nhớ.");
            }
        }
        else
        {
            Debug.Log("Không xóa token vì người chơi đã ghi nhớ.");
        }

        PlayerPrefs.DeleteKey(RememberMeKey);
        PlayerPrefs.Save();
    }

    public bool IsRemembered()
    {
        return PlayerPrefs.GetInt(RememberMeKey, 0) == 1;
    }
}
