using Fusion;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class DiceManager : NetworkBehaviour
{
    public CinemachineCamera mainCam;
    public CinemachineCamera diceCam;

   
    [Networked] public int DiceResult { get; private set; }
    [Networked] public int LastDiceValue { get; set; } 

    TurnManager turnManager;
    DiceRoll diceRoll; 
    PlayerTurnState playerTurnState;
    TimeManager timeManager;

    
    public Sprite[] diceFaces; 

   
    public Image P1DiceFaceImage; 
    public Image P2DiceFaceImage; 
    public Image P3DiceFaceImage; 
    public Image P4DiceFaceImage; 

    Coroutine rollCoroutine;

    

    [Networked] public bool activeDiceCam { get; set; }
    public override void Spawned()
    {
        activeDiceCam = true;

        if (Object.HasStateAuthority)
            Debug.Log("DiceManager ready.");

        P1DiceFaceImage = GameObject.Find("Canvas/P1/Dice Image").GetComponent<Image>();
        P2DiceFaceImage = GameObject.Find("Canvas/P2/Dice Image").GetComponent<Image>();
        P3DiceFaceImage = GameObject.Find("Canvas/P3/Dice Image").GetComponent<Image>();
        P4DiceFaceImage = GameObject.Find("Canvas/P4/Dice Image").GetComponent<Image>();

        P1DiceFaceImage.sprite = P2DiceFaceImage.sprite = P3DiceFaceImage.sprite = P4DiceFaceImage.sprite = diceFaces[0];
        P1DiceFaceImage.color = P2DiceFaceImage.color = P3DiceFaceImage.color = P4DiceFaceImage.color = new Color(255, 255, 255, 255);

        diceRoll = FindFirstObjectByType<DiceRoll>();

        mainCam = GameObject.Find("Main Cinemachine Cam").GetComponent<CinemachineCamera>();
        diceCam = GameObject.Find("Dice Cinemachine Cam").GetComponent<CinemachineCamera>();     
    }

    private void Update()
    {
        if (turnManager == null)
        {
            turnManager = GameObject.FindFirstObjectByType<TurnManager>();
        }
        if (playerTurnState == null)
        {
            playerTurnState = GameObject.FindFirstObjectByType<PlayerTurnState>();
        }
        if (timeManager == null)
        {
            timeManager = GameObject.FindFirstObjectByType<TimeManager>();
        }
    }

   
    public void RequestRoll()
    {
       
        if (playerTurnState != null && playerTurnState.HasRolled)
            return;

        
        if (playerTurnState != null)
            playerTurnState.RPC_SetRolled(true);

        if (playerTurnState != null)
            playerTurnState.RPC_SetMoved(false); 

        
        RPC_RequestRoll();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_RequestRoll(RpcInfo info = default)
    {
        int playerId = turnManager.GetCurrentPlayerId();
        if (playerId != info.Source.PlayerId) return;

        
        RPC_SwitchToDiceCam();

       
        if (ShouldForceSix(playerId))
        {
            ForceRollSix(playerId); 
        }
        else
        {
            rollCoroutine = StartCoroutine(StartRollSequence(playerId)); 
        }

    }

    public void StopRollCoroutine()
    {
        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);
            RPC_SwitchToMainCam();
        }  
    }

    private IEnumerator StartRollSequence(int playerId)
    {
        
        diceRoll.ResetDicePosition();

        
        yield return new WaitForSeconds(1f);

        
        diceRoll.RollDice();

       
        yield return StartCoroutine(WaitDiceStop());

        
        yield return new WaitForSeconds(1f);

        
        LastDiceValue = DiceResult;

       
        DiceResult = diceRoll.diceFaceNum;

        
        RPC_ApplyRollToAll(DiceResult, playerId);

        
        yield return new WaitForSeconds(0.5f);

        
        RPC_SwitchToMainCam();
    }

    private IEnumerator WaitDiceStop()
    {
        Rigidbody rb = diceRoll.GetComponent<Rigidbody>();
        yield return new WaitForSeconds(0.5f);

        while (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f)
        {
            yield return null;
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_ApplyRollToAll(int value, int playerId)
    {
        DiceResult = value;

        switch (playerId)
        {
            case 1: P2DiceFaceImage.sprite = P3DiceFaceImage.sprite = P4DiceFaceImage.sprite = diceFaces[0]; break;
            case 2: P1DiceFaceImage.sprite = P3DiceFaceImage.sprite = P4DiceFaceImage.sprite = diceFaces[0]; break;
            case 3: P1DiceFaceImage.sprite = P2DiceFaceImage.sprite = P4DiceFaceImage.sprite = diceFaces[0]; break;
            case 4: P1DiceFaceImage.sprite = P2DiceFaceImage.sprite = P3DiceFaceImage.sprite = diceFaces[0]; break;
        }

        
        Sprite rolledFace = diceFaces[value]; 
        switch (playerId)
        {
            case 1: P1DiceFaceImage.sprite = rolledFace; break;
            case 2: P2DiceFaceImage.sprite = rolledFace; break;
            case 3: P3DiceFaceImage.sprite = rolledFace; break;
            case 4: P4DiceFaceImage.sprite = rolledFace; break;
        }

        if (value > 0)
        {
            turnManager.HighlightMovablePieces();
        }

        if (value != 6)
        {
            if (playerTurnState != null && !PlayerHasPieceOnBoard(playerId))
            {
                playerTurnState.RPC_IncrementNoSixRollCount(playerId);
            }       

            if (!CheckAllPlayerPieceCanMove(playerId))
            {
                turnManager.NextTurnWhenDone();
            }

            
        }
        else
        {
            turnManager.ResetWaitBeforeNextTurnCoroutine(); 

            if (playerTurnState != null)
            {
                playerTurnState.RPC_ResetNoSixRollCount(playerId);

                playerTurnState.RPC_SetRolled(false); 
            }
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_SetWaitingFace(int nextPlayerID)
    {
        
        switch (nextPlayerID)
        {
            case 1: P1DiceFaceImage.sprite = diceFaces[7]; break;
            case 2: P2DiceFaceImage.sprite = diceFaces[7]; break;
            case 3: P3DiceFaceImage.sprite = diceFaces[7]; break;
            case 4: P4DiceFaceImage.sprite = diceFaces[7]; break;
        }
    }

   
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_SwitchToDiceCam()
    {
        if (!activeDiceCam) return;

        diceCam.Priority = 20;
        mainCam.Priority = 10;
        turnManager.playerBaseCam.Priority = 10;
    }

    
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_SwitchToMainCam()
    {
        mainCam.Priority = 20;
        diceCam.Priority = 10;
        turnManager.playerBaseCam.Priority = 10;
    }

    public void ResetDice()
    {
        DiceResult = 0;
        LastDiceValue = 0;
        if (diceRoll != null)
            diceRoll.diceFaceNum = 0;
        Debug.Log("Dice reset to 0");
    }

    private bool ShouldForceSix(int playerId)
    {
       
        var allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        bool hasPieceOnBoard = false;

        foreach (var piece in allPieces)
        {
            if (piece.Object.InputAuthority.PlayerId == playerId &&
                piece.State == PieceState.OnPath) 
            {
                hasPieceOnBoard = true;
                break;
            }
        }

        return !hasPieceOnBoard && playerTurnState.GetNoSixRollCount(playerId) >= 2;
    }

    private void ForceRollSix(int playerId)
    {
        Debug.Log($"Player {playerId} được bảo kê roll ra 6!");

       
        DiceResult = 6;
        LastDiceValue = DiceResult;

        
        RPC_ApplyRollToAll(6, playerId);

       
        playerTurnState.RPC_ResetNoSixRollCount(playerId);

        
        RPC_SwitchToMainCam();
    }


    private bool CheckAllPlayerPieceCanMove(int playerId)
    {
        var allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);

        foreach (var piece in allPieces)
        {
            if (piece.Object.InputAuthority.PlayerId != playerId)
                continue;

            
            if (piece.CanMove(DiceResult))
                return true;
        }

        
        return false;
    }
    private bool PlayerHasPieceOnBoard(int playerId)
    {
        var allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            if (piece.Object.InputAuthority.PlayerId == playerId && piece.State == PieceState.OnPath)
                return true;
        }
        return false;
    }

}
