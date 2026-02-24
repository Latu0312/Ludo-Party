using UnityEngine;

public class ClearPlayerPrefsOnly : MonoBehaviour
{
    void Start()
    {
        
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Đã xóa toàn bộ PlayerPrefs.");
    }
}
