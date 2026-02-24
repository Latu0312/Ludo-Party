using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;

public class RankingManager : NetworkBehaviour
{
    
    public TextMeshProUGUI rankingText;

   
    public TextMeshProUGUI[] playerResultTexts = new TextMeshProUGUI[4]; 

    [Networked] public NetworkDictionary<PlayerColor, int> PlayerRankings { get; } = MakeInitializer(new Dictionary<PlayerColor, int>());
    [Networked] public int CurrentRank { get; set; } = 1; 

    private GameManager gameManager;
    private Dictionary<PlayerColor, int> completedPieces = new Dictionary<PlayerColor, int>();
    private Dictionary<PlayerColor, List<int>> piecePositions = new Dictionary<PlayerColor, List<int>>();

    private DatabaseReference dbRef;

   
    private Dictionary<int, string> playerNames = new Dictionary<int, string>();

    public override void Spawned()
    {
       
        rankingText = GameObject.Find("Canvas/Ranking Text")?.GetComponent<TextMeshProUGUI>();
        if (rankingText == null)
        {
            Debug.LogError("Không tìm thấy Ranking Text trong Canvas!");
        }
        else
        {
            rankingText.text = "Rankings: Waiting...";
        }

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        
        completedPieces[PlayerColor.Red] = 0;
        completedPieces[PlayerColor.Blue] = 0;
        completedPieces[PlayerColor.Green] = 0;
        completedPieces[PlayerColor.Yellow] = 0;

        piecePositions[PlayerColor.Red] = new List<int>();
        piecePositions[PlayerColor.Blue] = new List<int>();
        piecePositions[PlayerColor.Green] = new List<int>();
        piecePositions[PlayerColor.Yellow] = new List<int>();

     
        LoadAllPlayerNames();
    }

    private void Update()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.FindFirstObjectByType<GameManager>();
        }
    }

    private void LoadAllPlayerNames()
    {
        foreach (var kvp in PlayerSpawn.PlayerTokens)
        {
            int playerId = kvp.Key;
            string token = kvp.Value;

         
            dbRef.Child("users").Child(token).Child("username")
                 .GetValueAsync().ContinueWithOnMainThread(task =>
                 {
                     if (task.IsCompleted && task.Result.Exists)
                     {
                         string username = task.Result.Value.ToString();
                         playerNames[playerId] = username;
                         Debug.Log($"[FIREBASE] PlayerId {playerId} có username: {username}");
                     }
                     else
                     {
                         playerNames[playerId] = "Unknown";
                         Debug.LogWarning($"[FIREBASE] Không tìm thấy username cho PlayerId {playerId}, token {token}");
                     }
                 });
        }
    }

    public void RequestPieceFinish(Piece piece)
    {
        if (HasStateAuthority)
        {
            OnPieceFinished(piece);
        }
        else
        {
            RPC_RequestPieceFinish(piece.Object.Id);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPieceFinish(NetworkId pieceId)
    {
        if (Runner.TryFindObject(pieceId, out NetworkObject obj))
        {
            Piece piece = obj.GetComponent<Piece>();
            if (piece != null)
            {
                OnPieceFinished(piece);
            }
        }
    }

   
    public void OnPieceFinished(Piece piece)
    {
        if (!Object.HasStateAuthority) return;

        PlayerColor color = piece.Color;
        int goalIndex = piece.CurrentTileIndex - GameBoard.Instance.commonPath.Count;

       
        piecePositions[color].Add(goalIndex);
        completedPieces[color]++;

        Debug.Log($"Quân cờ {color} về đích tại ô {goalIndex}. Tổng số quân hoàn thành: {completedPieces[color]}");

        
        if (completedPieces[color] == 4)
        {
            CheckRankingCondition(color);
        }
    }

    private void CheckRankingCondition(PlayerColor color)
    {
        var allPieceOfColor = piecePositions[color];
        if (allPieceOfColor.Count == 4)
        {
            if (!PlayerRankings.ContainsKey(color))
            {
                PlayerRankings.Add(color, CurrentRank);
                CurrentRank++;
                RPC_UpdateRankingUI();
                Debug.Log($"Người chơi {color} đạt thứ hạng {PlayerRankings[color]}!");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateRankingUI()
    {
        string rankingDisplay = "Rankings:\n";
        foreach (var rank in PlayerRankings)
        {
            rankingDisplay += $"{rank.Key}: {rank.Value}\n";
        }
        if (rankingText != null)
        {
            rankingText.text = rankingDisplay;
        }
    }

    public int GetCompletedPieceCount(PlayerColor color)
    {
        return completedPieces.ContainsKey(color) ? completedPieces[color] : 0;
    }

    public void OnAllPiecesFinishedForceAssign(PlayerColor color)
    {
        if (!Object.HasStateAuthority) return;

        if (!PlayerRankings.ContainsKey(color))
        {
            PlayerRankings.Add(color, CurrentRank);
            CurrentRank++;
            RPC_UpdateRankingUI();
            Debug.Log($"[FORCE] Người chơi {color} được gán thứ hạng {PlayerRankings[color]}!");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_CallStateDisplayFinalResults()
    {
        RPC_DisplayFinalResults();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DisplayFinalResults()
    {
        if (playerResultTexts == null || playerResultTexts.Length < 4) return;

        foreach (var text in playerResultTexts)
        {
            text.text = ""; 
        }

        foreach (var kvp in PlayerRankings)
        {
            PlayerColor color = kvp.Key;
            int rank = kvp.Value;

            if (rank >= 1 && rank <= 4)
            {
               
                int playerId = -1;
                if (color == PlayerColor.Red) playerId = 1;
                else if (color == PlayerColor.Blue) playerId = 2;
                else if (color == PlayerColor.Green) playerId = 3;
                else if (color == PlayerColor.Yellow) playerId = 4;

              
                string username = (playerId != -1 && playerNames.ContainsKey(playerId))
                                     ? playerNames[playerId]
                                     : color.ToString();

                var text = playerResultTexts[rank - 1];
                text.text = $"{username} - Rank {rank}";
                text.color = GetColorByPlayer(color);

                
                int rewardCurrency = 0;
                int rewardExp = 0;
                switch (rank)
                {
                    case 1: rewardCurrency = 200; rewardExp = 20; break;
                    case 2: rewardCurrency = 150; rewardExp = 15; break;
                    case 3: rewardCurrency = 100; rewardExp = 10; break;
                    case 4: rewardCurrency = 40; rewardExp = 5; break;
                }

                if (playerId != -1 && PlayerSpawn.PlayerTokens.TryGetValue(playerId, out string token))
                {
                    Debug.Log($"[REWARD] {username} (Rank {rank}) nhận {rewardCurrency} tiền, {rewardExp} exp. Token={token}");

                    var userRef = dbRef.Child("users").Child(token);

                   
                    userRef.Child("currency").Child("softCurrency").RunTransaction(mutableData =>
                    {
                        int currentValue = mutableData.Value == null ? 0 : int.Parse(mutableData.Value.ToString());
                        mutableData.Value = currentValue + rewardCurrency;
                        return TransactionResult.Success(mutableData);
                    });

                   
                    userRef.Child("experience").RunTransaction(mutableData =>
                    {
                        int currentValue = mutableData.Value == null ? 0 : int.Parse(mutableData.Value.ToString());
                        mutableData.Value = currentValue + rewardExp;
                        return TransactionResult.Success(mutableData);
                    });
                }
                else
                {
                    Debug.LogWarning($"[REWARD] Không tìm thấy token cho {color} (PlayerId={playerId})");
                }
            }
        }
    }

    private Color GetColorByPlayer(PlayerColor color)
    {
        switch (color)
        {
            case PlayerColor.Red: return Color.red;
            case PlayerColor.Blue: return Color.blue;
            case PlayerColor.Green: return Color.green;
            case PlayerColor.Yellow: return Color.yellow;
            default: return Color.white;
        }
    }
}
