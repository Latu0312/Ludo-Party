using UnityEngine;
using Firebase.Database;
using Fusion;
using Fusion.Sockets;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class Bootstrapper : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runnerPrefab;

    private string roomId;
    private string userToken;

    private async void Start()
    {
        roomId = LoadTextFromConfig("roomId.txt");
        userToken = LoadTextFromConfig("userToken.txt");

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(userToken))
        {
            Debug.LogError("Không đọc được roomId hoặc userToken từ file config.");
            return;
        }

        bool isHost = await CheckIfUserIsHost(roomId, userToken);

        Debug.Log($"roomId: {roomId} | userToken: {userToken} | Vai trò: {(isHost ? "HOST" : "CLIENT")}");

        StartFusionRunner(isHost);
    }

    string LoadTextFromConfig(string fileName)
    {
#if UNITY_EDITOR
       
        string fullPath = Path.Combine(@"C:\Users\latu0\Desktop\editorTest", fileName);
#else
    // 👉 Đường dẫn bình thường cho build thật
    string fullPath = Path.Combine(Application.persistentDataPath, "config", fileName);
#endif

        return File.Exists(fullPath) ? File.ReadAllText(fullPath).Trim() : null;
    }

    async Task<bool> CheckIfUserIsHost(string roomId, string token)
    {
        var dataSnapshot = await FirebaseDatabase.DefaultInstance
            .GetReference("rooms").Child(roomId).Child("hostToken").GetValueAsync();

        return dataSnapshot.Exists && dataSnapshot.Value.ToString() == token;
    }

    void StartFusionRunner(bool isHost)
    {
        NetworkRunner runner = Instantiate(runnerPrefab);
        runner.name = "FusionRunner_" + roomId;
        runner.ProvideInput = true;

        var sceneManager = runner.GetComponent<NetworkSceneManagerDefault>();
        var currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        var args = new StartGameArgs()
        {
            GameMode = isHost ? GameMode.Host : GameMode.Client,
            SessionName = roomId,
            SceneManager = sceneManager
           
        };

        runner.StartGame(args).ContinueWith(task =>
        {
            if (task.Result.Ok)
                Debug.Log($"Fusion {(isHost ? "Host" : "Client")} started successfully for room {roomId}");
            else
                Debug.LogError("Fusion StartGame Failed: " + task.Result.ShutdownReason);
        });
    }

 
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player joined: " + player.PlayerId);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player left: " + player.PlayerId);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    
}

