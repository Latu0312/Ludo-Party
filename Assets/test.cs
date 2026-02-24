using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Debug.Log("✅ UnityMainThreadDispatcher đang hoạt động.");
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
