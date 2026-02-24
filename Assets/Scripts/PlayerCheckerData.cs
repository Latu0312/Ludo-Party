using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseUserDataChecker : MonoBehaviour
{
    private string userId;

    void Start()
    {
        userId = PlayerPrefs.GetString("userId");
        CheckUserData();
    }

    void CheckUserData()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(userId)
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Lỗi khi truy cập dữ liệu từ Firebase.");
                    return;
                }

                if (!task.IsCompleted || task.Result == null || !task.Result.Exists)
                {
                    Debug.LogWarning("Không tìm thấy dữ liệu người dùng.");
                    return;
                }

                DataSnapshot snapshot = task.Result;

              
                Debug.Log("Dữ liệu người dùng:\n" + snapshot.GetRawJsonValue());

               
                if (snapshot.HasChild("username"))
                {
                    string username = snapshot.Child("username").Value.ToString();
                    Debug.Log("Tên người dùng: " + username);
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy username.");
                }

                if (snapshot.HasChild("currency/softCurrency"))
                {
                    string softCurrency = snapshot.Child("currency").Child("softCurrency").Value.ToString();
                    Debug.Log("SoftCurrency: " + softCurrency);
                }
                else
                {
                    Debug.LogWarning("Không có dữ liệu softCurrency.");
                }

                if (snapshot.HasChild("currency/hardCurrency"))
                {
                    string hardCurrency = snapshot.Child("currency").Child("hardCurrency").Value.ToString();
                    Debug.Log("HardCurrency: " + hardCurrency);
                }
                else
                {
                    Debug.LogWarning("Không có dữ liệu hardCurrency.");
                }
            });
    }
}
