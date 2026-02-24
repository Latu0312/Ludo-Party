using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpawn : SimulationBehaviour, IPlayerJoined, INetworkRunnerCallbacks
{
   
    public GameObject PlayerPrefab;

   
    public Transform RedSpawn;
    public Transform GreenSpawn;
    public Transform YellowSpawn;
    public Transform BlueSpawn;

   
    public GameObject RedPiecePrefab;
    public GameObject GreenPiecePrefab;
    public GameObject YellowPiecePrefab;
    public GameObject BluePiecePrefab;

   
    public Transform mapCenter;

    private Transform baseSpawn;
    private GameObject piecePrefab;

   
    public GameObject turnManagerPrefab;
    public GameObject diceManagerPrefab;
    public GameObject gameManagerPrefab;
    public GameObject playerTurnStatePrefab;
    public GameObject timeManager;

 
 
    public static Dictionary<int, string> PlayerTokens = new Dictionary<int, string>();

   
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("[Fusion] Runner shutdown -> clear PlayerTokens");
        PlayerTokens.Clear();
    }


    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            if (Runner.LocalPlayer.PlayerId == 1)
            {
               
                GameObject allManager = GameObject.Find("All Manager");
                if (allManager == null)
                {
                    allManager = new GameObject("All Manager");
                }

                if (turnManagerPrefab != null)
                {
                    var obj = Runner.Spawn(turnManagerPrefab, Vector3.zero, Quaternion.identity, player);
                    obj.transform.SetParent(allManager.transform);
                }

                if (diceManagerPrefab != null)
                {
                    var obj = Runner.Spawn(diceManagerPrefab, Vector3.zero, Quaternion.identity, player);
                    obj.transform.SetParent(allManager.transform);
                }

                if (gameManagerPrefab != null)
                {
                    var obj = Runner.Spawn(gameManagerPrefab, Vector3.zero, Quaternion.identity, player);
                    obj.transform.SetParent(allManager.transform);
                }

                if (playerTurnStatePrefab != null)
                {
                    var obj = Runner.Spawn(playerTurnStatePrefab, Vector3.zero, Quaternion.identity, player);
                    obj.transform.SetParent(allManager.transform);
                }

                if (timeManager != null)
                {
                    var obj = Runner.Spawn(timeManager, Vector3.zero, Quaternion.identity, player);
                    obj.transform.SetParent(allManager.transform);
                }
            }

           
            Runner.Spawn(PlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity,
            Runner.LocalPlayer, (runner, obj) =>
            {
                Debug.Log($"Spawned player with ID: {player.PlayerId}");
                Runner.SetPlayerObject(player, obj);
            });

            
            string token = UserSession.Token;
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("[TOKEN] UserSession.Token rỗng → dùng DEFAULT_TOKEN");
                token = "DEFAULT_TOKEN";
            }

            if (!PlayerTokens.ContainsKey(player.PlayerId))
            {
                PlayerTokens[player.PlayerId] = token;
            }
            Debug.Log($"[TOKEN] Gán token [{token}] cho PlayerId {player.PlayerId}");

            
            PlayerNameManager.SetMyName(player.PlayerId, token);
        }

        Debug.Log($"Player {player.PlayerId} spawned successfully. Total players: {Runner.ActivePlayers.Count()}");

        
        if (player == Runner.LocalPlayer)
        {
            if (player.PlayerId == 1)
            {
                baseSpawn = RedSpawn;
                piecePrefab = RedPiecePrefab;
            }
            else if (player.PlayerId == 2)
            {
                baseSpawn = BlueSpawn;
                piecePrefab = BluePiecePrefab;
            }
            else if (player.PlayerId == 3)
            {
                baseSpawn = GreenSpawn;
                piecePrefab = GreenPiecePrefab;
            }
            else if (player.PlayerId == 4)
            {
                baseSpawn = YellowSpawn;
                piecePrefab = YellowPiecePrefab;
            }
            else
            {
                Debug.LogError($"Không có chuồng cho Player ID {player.PlayerId}");
                return;
            }

            if (baseSpawn == null || piecePrefab == null)
            {
                Debug.LogWarning($"Thiếu thông tin spawn cho PlayerId {player.PlayerId}");
                return;
            }

          
            for (int i = 0; i < 4; i++)
            {
                Transform point = baseSpawn.GetChild(i);
                if (point != null)
                {
                    Vector3 directionToCenter = mapCenter.position - point.position;
                    directionToCenter.y = 0;

                    if (directionToCenter == Vector3.zero)
                        directionToCenter = Vector3.forward;

                    Quaternion lookRotation = Quaternion.LookRotation(directionToCenter);
                    Quaternion originalRotation = point.rotation;
                    Quaternion finalRotation = Quaternion.Euler(
                        originalRotation.eulerAngles.x,
                        lookRotation.eulerAngles.y,
                        originalRotation.eulerAngles.z
                    );

                    Runner.Spawn(piecePrefab, point.position, finalRotation, player);
                }
                else
                {
                    Debug.LogWarning($"Thiếu điểm spawn quân cờ con thứ {i}");
                }
            }
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
       
        Debug.Log("OnHostMigration được gọi - chưa xử lý.");
    }

}
