using Fusion;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    private Camera mainCamera;
    private bool queueRoll = false;
    private bool raycastMove = false;

    TurnManager turnManager;
    DiceManager diceManager;
    PlayerTurnState playerTurnState;
    DiceRoll diceRoll;
    SoundManager soundManager;

    private bool isRolling = false; 

    private Button rollButton;

    public override void Spawned()
    {
        diceRoll = FindFirstObjectByType<DiceRoll>();

        if (Object.HasInputAuthority)
        {
            mainCamera = Camera.main;
        }

        rollButton = GameObject.Find("Canvas/Roll Button").GetComponent<Button>();
       
        if (rollButton != null)
        {
            rollButton.onClick.AddListener(OnRollButtonClicked);
        }
    }

    private void Update()
    {
        
        if (turnManager == null)
        {
            turnManager = GameObject.FindFirstObjectByType<TurnManager>();
        }
        if (diceManager == null)
        {
            diceManager = GameObject.FindFirstObjectByType<DiceManager>();
        }
        if (playerTurnState == null)
        {
            playerTurnState = GameObject.FindFirstObjectByType<PlayerTurnState>();
        }
        if(soundManager == null)
        {
            soundManager = GameObject.FindFirstObjectByType<SoundManager>();
        }

        if (!turnManager.IsGameStarted) return;

        if (!Object.HasInputAuthority) return;

        if (turnManager != null && turnManager.isSpawned)
        {
            if (turnManager.GetCurrentPlayerId() != Runner.LocalPlayer.PlayerId) return;
        }

        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R) && !isRolling)
        {
            queueRoll = true;       
            StartCoroutine(WaitForRoll());
        }

        if (Object.HasInputAuthority && Input.GetMouseButtonDown(0))
        {
            raycastMove = true;  
        }
 
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        if (turnManager != null && turnManager.isSpawned)
        {
            if (turnManager.GetCurrentPlayerId() != Runner.LocalPlayer.PlayerId) return;
        }

        if (queueRoll)
        {
            queueRoll = false;
            diceManager.RequestRoll();
        }

        if (raycastMove)
        {

            raycastMove = false;

            

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                Piece piece = hit.collider.GetComponent<Piece>();
                if (piece != null)
                {
                    if (piece.State == PieceState.Finished)
                    {
                        return; 
                    }
                    TrySelectPawn(piece);
                }
            }
        }

        void TrySelectPawn(Piece piece)
        {
            piece.TryMove(diceManager.DiceResult);
        }
    }
    private IEnumerator WaitForRoll()
    {
        isRolling = true;
        yield return new WaitForSeconds(2f); 
        isRolling = false;
    }

    private void OnRollButtonClicked()
    {
        if (!turnManager.IsGameStarted) return;
        if (turnManager != null && turnManager.isSpawned)
        {
            if (turnManager.GetCurrentPlayerId() != Runner.LocalPlayer.PlayerId) return;
        }

        if (!isRolling && Object.HasInputAuthority)
        {
            queueRoll = true;
            StartCoroutine(WaitForRoll());
        }

    }
}
