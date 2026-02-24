using Firebase.Database;
using Firebase.Extensions;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameManager : NetworkBehaviour, IPlayerLeft
{
  
    public TMP_Text[] playerNameTexts;

    private static PlayerNameManager _instance;

   
    private Dictionary<int, string> playerNames = new Dictionary<int, string>();

    private void Awake()
    {
        _instance = this;
    }

   
    public void FetchAndBroadcastMyName(int playerId, string token)
    {
        var dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("rooms")
             .Child(UserSession.RoomId)
             .Child("players")
             .Child(token)
             .Child("username")
             .GetValueAsync()
             .ContinueWithOnMainThread(task =>
             {
                 string username = $"Player{playerId}";
                 if (task.IsCompleted && task.Result.Exists)
                 {
                     username = task.Result.Value.ToString();
                 }

                 Debug.Log($"[CLIENT] PlayerId={playerId} username={username} → gửi RPC");

                 
                 RPC_SetPlayerName(playerId, username);

                 
                 RPC_RequestAllNames(playerId);
             });
    }

    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SetPlayerName(int playerId, string username)
    {
        Debug.Log($"[RPC] Set name {username} cho PlayerId={playerId}");

        playerNames[playerId] = username;

        if (playerId >= 1 && playerId <= playerNameTexts.Length)
        {
            playerNameTexts[playerId - 1].text = username;
        }
    }

    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_RequestAllNames(int newPlayerId)
    {
        
        foreach (var kvp in playerNames)
        {
            int pid = kvp.Key;
            string uname = kvp.Value;

            if (!string.IsNullOrEmpty(uname))
            {
                RPC_SetPlayerName(pid, uname);
            }
        }
    }

    
    public static void SetMyName(int playerId, string token)
    {
        if (_instance != null)
        {
            _instance.FetchAndBroadcastMyName(playerId, token);
        }
    }

   
    public void PlayerLeft(PlayerRef player)
    {
        if (playerNames.ContainsKey(player.PlayerId))
        {
            Debug.Log($"[Fusion] PlayerId={player.PlayerId} rời phòng → xoá tên");

            playerNames.Remove(player.PlayerId);

            if (player.PlayerId >= 1 && player.PlayerId <= playerNameTexts.Length)
            {
                playerNameTexts[player.PlayerId - 1].text = $"Empty Slot {player.PlayerId}";
            }
        }
    }
    public static void ClearNames()
    {
        if (_instance != null)
        {
            _instance.playerNames.Clear();

            for (int i = 0; i < _instance.playerNameTexts.Length; i++)
            {
                _instance.playerNameTexts[i].text = $"Empty Slot {i + 1}";
            }
        }
    }


   
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }



}
