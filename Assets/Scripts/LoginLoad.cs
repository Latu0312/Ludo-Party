using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
   
    public void LoadSceneByName(string sceneName)
    {
        LoadingScreen.LoadScene(sceneName);
    }
}
