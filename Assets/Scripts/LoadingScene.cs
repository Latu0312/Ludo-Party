using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static string sceneToLoad;   

   
    public GameObject loadingPanel;
    public Slider loadingBar;
    public TextMeshProUGUI loadingText;

    private float progressValue = 0f;

    void Start()
    {
       
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            loadingPanel.SetActive(true);
            StartCoroutine(LoadAsynchronously(sceneToLoad)); 
        }
    }

    public static void LoadScene(string targetScene)
    {
       
        sceneToLoad = targetScene;
        
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float target = Mathf.Clamp01(operation.progress / 0.9f);
            progressValue = Mathf.MoveTowards(progressValue, target, Time.deltaTime);

            loadingBar.value = progressValue;
            loadingText.text = Mathf.RoundToInt(progressValue * 100f) + "%";

            if (progressValue >= 1f)
            {
                yield return new WaitForSeconds(1f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
