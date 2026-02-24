using Fusion;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : NetworkBehaviour
{
   
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI yourTurnText;
    public Image yourTurnImage;
    public TextMeshProUGUI waitingForAllPlayerText;

    DiceManager diceManager;
    PlayerTurnState playerTurnState;
    TimeManager timeManager;

    
    [Networked] public int currentPlayerTurn { get; private set; }

    public bool isSpawned = false;

   
    public float waitingTimeDefault = 10f; 

    Coroutine waitBeforeNextTurnCoroutine;

    private RankingManager rankingManager;

    public GameObject EndGameCanvas;

    [Networked] public bool IsGameStarted { get; private set; } = false; 

    public CinemachineCamera playerBaseCam;
    public List<Transform> playerBasePoint = new List<Transform>();

  
    [Networked] public bool activeBaseCam { get; set; }

    private SoundManager soundManager;

    public override void Spawned()
    {
        turnText = GameObject.Find("Canvas/Turn").GetComponent<TextMeshProUGUI>();
        yourTurnText = GameObject.Find("Canvas/Your Turn Text").GetComponent<TextMeshProUGUI>();
        yourTurnImage = GameObject.Find("Canvas/Your Turn Image").GetComponent<Image>();
        waitingForAllPlayerText = GameObject.Find("Canvas/Waiting For All Player Text").GetComponent<TextMeshProUGUI>();

        playerBaseCam = GameObject.Find("Player Base Cinemachine Cam").GetComponent<CinemachineCamera>();
        playerBasePoint = GameObject.Find("Player Base Points").GetComponentsInChildren<Transform>().ToList();
        playerBasePoint.RemoveAt(0); 

        EndGameCanvas = GameObject.Find("End Game Canvas");
        EndGameCanvas.SetActive(false);
        soundManager = GameObject.FindAnyObjectByType<SoundManager>();

        currentPlayerTurn = 1;
        isSpawned = true;

        activeBaseCam = true;

        StartCoroutine(WaitUntilAllPlayersThenSpawn());
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_SwitchToPlayerBaseCam(int playerIndex)
    {
        if (!activeBaseCam) return;

        if (playerIndex < 0 || playerIndex > playerBasePoint.Count)
        {
            Debug.LogWarning("playerIndex không hợp lệ");
            return;
        }

        playerBaseCam.LookAt = playerBasePoint[playerIndex];
        playerBaseCam.Follow = playerBasePoint[playerIndex];

        playerBaseCam.Priority = 20;
        diceManager.mainCam.Priority = 10;
        diceManager.diceCam.Priority = 10;
    }

    private IEnumerator WaitUntilAllPlayersThenSpawn()
    {
        Debug.Log("[TurnManager] Waiting for all players to join...");
        yourTurnImage.color = new Color(0f, 0f, 0f, 0.5f); 
        waitingForAllPlayerText.text = "Waiting for all players...";

        
        while (!IsAllPlayersReady())
        {
            RPC_ShowWaitingText(true, "Waiting for all players...");
            yield return new WaitForSeconds(0.5f);
        }

       
        RPC_ShowWaitingText(true, "GAME START");
        yield return new WaitForSeconds(1f);
        RPC_ShowWaitingText(false, "");

        IsGameStarted = true; 

        DisplayYourTurn(currentPlayerTurn); 
        StartCoroutine(SwitchToBaseCamThenSwitchToMainCam());
        WaitNextTurn(); 
    }
    private bool IsAllPlayersReady()
    {
        return Runner.ActivePlayers.Count() >= TileEffectManager.Instance.totalExpectedPlayers;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowWaitingText(bool show, string message)
    {
        waitingForAllPlayerText.text = show ? message : "";
        yourTurnImage.color = show ? new Color(0f, 0f, 0f, 0.5f) : new Color(0f, 0f, 0f, 0f);
    }


    public void Update()
    {
        if (diceManager == null)
        {
            diceManager = GameObject.FindFirstObjectByType<DiceManager>();
        }
        if (playerTurnState == null)
        {
            playerTurnState = GameObject.FindFirstObjectByType<PlayerTurnState>();
        }
        if (timeManager == null)
        {
            timeManager = GameObject.FindFirstObjectByType<TimeManager>();
        }
        if (rankingManager == null)
        {
            rankingManager = GameObject.FindFirstObjectByType<RankingManager>();
        }
    }

    public void RequestNextTurn()
    {
        RPC_HandleNextTurn();
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_HandleNextTurn()
    {
        int maxPlayers = GetMaxPlayerId();
        int next = currentPlayerTurn;
        int attempts = 0;
        bool found = false;

       
        int unfinishedPlayers = 0;
        int lastUnfinishedPlayerId = 0;
        for (int i = 1; i <= maxPlayers; i++)
        {
            if (IsPlayerIdActive(i) && !HasPlayerFinishedAllPieces(i))
            {
                unfinishedPlayers++;
                lastUnfinishedPlayerId = i;
            }
        }

        
        if (unfinishedPlayers == 1)
        {
            PlayerColor color = lastUnfinishedPlayerId switch
            {
                1 => PlayerColor.Red,
                2 => PlayerColor.Blue,
                3 => PlayerColor.Green,
                4 => PlayerColor.Yellow,
                _ => PlayerColor.Red
            };

           
            if (!rankingManager.PlayerRankings.ContainsKey(color))
            {
                rankingManager.OnAllPiecesFinishedForceAssign(color);
            }
            Debug.Log($"Chỉ còn một người chơi ({color}) chưa hoàn thành. Game kết thúc!");
            RPC_StateCallDisplayFinishCanvas();

            rankingManager.RPC_CallStateDisplayFinalResults();

            return;
        }

        do
        {
            next++;
            if (next > maxPlayers) next = 1;

            attempts++;

            
            if (IsPlayerIdActive(next) && !HasPlayerFinishedAllPieces(next))
            {
                found = true;
                break;
            }
        } while (attempts < maxPlayers);

        
        if (!found)
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                if (HasPlayerFinishedAllPieces(i))
                {
                    PlayerColor color = i switch
                    {
                        1 => PlayerColor.Red,
                        2 => PlayerColor.Blue,
                        3 => PlayerColor.Green,
                        4 => PlayerColor.Yellow,
                        _ => PlayerColor.Red
                    };

                    if (!rankingManager.PlayerRankings.ContainsKey(color))
                    {
                        rankingManager.OnAllPiecesFinishedForceAssign(color);
                    }
                }
            }

            Debug.Log("Tất cả người chơi đã hoàn thành. Game kết thúc!");
            RPC_StateCallDisplayFinishCanvas();
            rankingManager.RPC_CallStateDisplayFinalResults();
            return;
        }
        currentPlayerTurn = next;

        if (playerTurnState != null)
        {
            playerTurnState.RPC_ResetForNewTurn();
        }

        RPC_UpdateTurnUI(currentPlayerTurn);
        RPC_UpdateCurrentPlayerTurn(currentPlayerTurn);
        diceManager.RPC_SetWaitingFace(next);
        
        diceManager.StopRollCoroutine();

        RPC_HideAllHighLightTile(); 

        StartCoroutine(SwitchToBaseCamThenSwitchToMainCam());

       
        if (diceManager != null)
        {
            diceManager.ResetDice();
        }

        WaitNextTurn(); 

        RPC_ClearAllHighlights(); 
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_HideAllHighLightTile()
    {
        GameBoard.Instance.HideAllHighlights();
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_UpdateCurrentPlayerTurn(int value)
    {
        currentPlayerTurn = value;
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_UpdateTurnUI(int turnId)
    {
        if (turnText == null) return;

        turnText.text = turnId switch
        {
            1 => "Red turn",
            2 => "Blue turn",
            3 => "Green turn",
            4 => "Yellow turn",
            _ => "Unknown Turn"
        };

        turnText.color = turnId switch
        {
            1 => Color.red,
            2 => Color.blue,
            3 => Color.green,
            4 => Color.yellow,
            _ => Color.white
        };

        
        DisplayYourTurn(turnId);
    }


    private bool IsPlayerIdActive(int id)
    {
        foreach (var player in Runner.ActivePlayers)
        {
            if (player.PlayerId == id) return true;
        }
        return false;
    }

    private int GetMaxPlayerId()
    {
        int maxId = 0;
        foreach (var player in Runner.ActivePlayers)
        {
            if (player.PlayerId > maxId)
                maxId = player.PlayerId;
        }
        return maxId;
    }

    public int GetCurrentPlayerId()
    {
        return currentPlayerTurn;
    }


    public void WaitNextTurn()
    {
        waitBeforeNextTurnCoroutine = StartCoroutine(WaitBeforeNextTurn());
    }

    public void ResetWaitBeforeNextTurnCoroutine()
    {
        StopCoroutine(waitBeforeNextTurnCoroutine);
        WaitNextTurn();
    }

    public void StopWaitNextTurnCoroutine()
    {
        if (waitBeforeNextTurnCoroutine == null) return;
        StopCoroutine(waitBeforeNextTurnCoroutine);
    }

    IEnumerator WaitBeforeNextTurn()
    {
        RPC_CallRingTurnTimer(); 
        yield return new WaitForSeconds(waitingTimeDefault);
        RequestNextTurn();
        Debug.Log($"Hết {waitingTimeDefault}. Đã chuyển lượt cho player: {currentPlayerTurn}");
    }

    public void NextTurnWhenDone()
    {
        StopWaitNextTurnCoroutine();
        RequestNextTurn();
        RPC_CallRingTurnTimer(); 
        Debug.Log($"Di chuyển xong. Đã chuyển lượt cho player: {currentPlayerTurn}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_CallRingTurnTimer()
    {
        TurnTimerController.Instance.RPC_ShowTurnTimer(GetCurrentPlayerId(), waitingTimeDefault);
    }

    public void HighlightMovablePieces()
    {
        if (playerTurnState == null || diceManager == null)
            return;

        
        if (Runner.LocalPlayer.PlayerId != currentPlayerTurn)
            return;
        var allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            if (piece.State == PieceState.Finished)
                continue; 

            if (piece.Object.InputAuthority.PlayerId != currentPlayerTurn)
                continue;

            bool canMove = piece.CanMove(diceManager.DiceResult);
            piece.SetHighlight(canMove);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ClearAllHighlights()
    {
        var allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            piece.SetHighlight(false);
        }
    }

    private void DisplayYourTurn(int turnId)
    {
        if (yourTurnText == null) return;
        if (Runner.LocalPlayer.PlayerId != turnId) return;
        StartCoroutine(YourTurn(turnId));
    }
    IEnumerator YourTurn(int turnId)
    {
        yourTurnText.text = "YOUR TURN";

        yourTurnText.color = turnId switch
        {
            1 => Color.red,
            2 => Color.blue,
            3 => Color.green,
            4 => Color.yellow,
            _ => Color.white
        };

        yourTurnImage.color = new Color(0f, 0f, 0f, 0.5f); 

        yield return new WaitForSeconds(1f);

        yourTurnImage.color = new Color(0f, 0f, 0f, 0f);
        yourTurnText.text = "";
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestEndTurnFromClient()
    {
        Debug.Log("[State] RPC_RequestEndTurnFromClient nhận được yêu cầu chuyển lượt");
        if (!HasStateAuthority) return;

        RPC_ReplyTurnFromState();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ReplyTurnFromState()
    {
        Debug.Log("[Client] Nhận được cho phép chuyển lượt");

        NextTurnWhenDone(); 
    }
    private bool HasPlayerFinishedAllPieces(int playerId)
    {
        if (rankingManager == null) return false;

      
        PlayerColor color = playerId switch
        {
            1 => PlayerColor.Red,
            2 => PlayerColor.Blue,
            3 => PlayerColor.Green,
            4 => PlayerColor.Yellow,
            _ => PlayerColor.Red 
        };

        return rankingManager.GetCompletedPieceCount(color) >= 4;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_CallStateDisplayFinishCanvas()
    {
        RPC_StateCallDisplayFinishCanvas();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StateCallDisplayFinishCanvas()
    {
        soundManager.PlayEndGameSound();
        EndGameCanvas.SetActive(true);
    }

    IEnumerator SwitchToBaseCamThenSwitchToMainCam()
    {
        int index = currentPlayerTurn - 1; 

       
        RPC_SwitchToPlayerBaseCam(index);

        yield return new WaitForSeconds(1f);

        diceManager.RPC_SwitchToMainCam();
    }
}
