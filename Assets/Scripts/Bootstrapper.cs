using UnityEngine;

public class SessionBootstrapper : MonoBehaviour
{
    public GameObject sessionManagerPrefab;

    void Awake()
    {
        if (SessionManager.Instance == null)
        {
            GameObject sm = Instantiate(sessionManagerPrefab);
            sm.name = "SessionManager";
        }
    }
}
