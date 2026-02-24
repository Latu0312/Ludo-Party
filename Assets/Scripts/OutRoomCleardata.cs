using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitRoom : MonoBehaviour
{
    public void OnExitRoom()
    {
       
        var runners = new List<NetworkRunner>(NetworkRunner.Instances);

        foreach (var runner in runners)
        {
            if (runner != null)
                runner.Shutdown();
        }

        
        PlayerSpawn.PlayerTokens.Clear();
        PlayerNameManager.ClearNames();

        
        LoadingScreen.LoadScene("mainMenu");
    }

}
