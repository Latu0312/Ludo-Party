using System.Collections;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UsernameSetter : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public GameObject errorText;
    public string nextScene = "mainMenu";
    CanvasManager CanvasManager; 

    private string token;
    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        
        StartCoroutine(WaitForToken());
    }
    private void Update()
    {
        if (CanvasManager == null)
        {
            CanvasManager = GameObject.FindFirstObjectByType<CanvasManager>();
        }
    }

    private IEnumerator WaitForToken()
    {
      
        while (string.IsNullOrEmpty(UserSession.Token))
        {
            Debug.Log("Chưa có token trong UserSession, đang chờ...");
            yield return null; 
        }

        token = UserSession.Token;
        Debug.Log("Token đã có: " + token);
    }

    public void OnConfirmButtonClicked()
    {
        string enteredUsername = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(enteredUsername))
        {
            Debug.LogWarning("Tên không được để trống.");
            return;
        }

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Không thể lưu vì chưa có token.");
            return;
        }

        StartCoroutine(CheckAndSaveUsername(enteredUsername));
    }

    IEnumerator CheckAndSaveUsername(string username)
    {
        var checkTask = dbRef.Child("users").OrderByChild("username").EqualTo(username).GetValueAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        if (checkTask.Result.Exists)
        {
            errorText?.SetActive(true);
            yield break;
        }

        var setTask = dbRef.Child("users").Child(token).Child("username").SetValueAsync(username);
        yield return new WaitUntil(() => setTask.IsCompleted);

        Debug.Log("Lưu tên thành công");
        errorText?.SetActive(false);
        CanvasManager.SwitchPanelByIndex(1);
    }
}
