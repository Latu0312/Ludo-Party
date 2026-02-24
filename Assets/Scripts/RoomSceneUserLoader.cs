using UnityEngine;

public class TokenLoader : MonoBehaviour
{
    public static string userToken;

    void Awake()
    {
        userToken = PlayerPrefs.GetString("userToken", "");
        if (string.IsNullOrEmpty(userToken))
        {
            Debug.LogError("Không tìm thấy token trong PlayerPrefs. Có thể người chơi chưa đăng nhập.");
        }
        else
        {
            Debug.Log("Đã lấy token người chơi: " + userToken);
        }
    }
}
