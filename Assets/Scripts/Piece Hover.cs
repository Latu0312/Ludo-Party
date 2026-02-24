using Fusion;
using UnityEngine;

public class PieceHover : NetworkBehaviour
{
    private Camera mainCamera;
    private Piece currentHoveredPiece;
    private TurnManager turnManager;
    private PlayerTurnState playerTurnState;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            mainCamera = Camera.main;
        }
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

        if (!Object.HasInputAuthority || mainCamera == null)
            return;

        

        if (playerTurnState.HasMoved) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            Piece piece = hit.collider.GetComponent<Piece>();
            if (piece != null)
            {
                
                if (currentHoveredPiece != piece)
                {
                    
                    if (currentHoveredPiece != null)
                        currentHoveredPiece.HidePossiblePath();

                   
                    if (piece.State != PieceState.Finished && piece.Object.InputAuthority.PlayerId == turnManager.currentPlayerTurn && piece.Object.InputAuthority.PlayerId == Runner.LocalPlayer.PlayerId)
                    {
                        currentHoveredPiece = piece;
                        currentHoveredPiece.ShowPossiblePath();
                    }
                }
            }
            else
            {
                
                if (currentHoveredPiece != null)
                {
                    currentHoveredPiece.HidePossiblePath();
                    currentHoveredPiece = null;
                }
            }
        }
        else
        {
          
            if (currentHoveredPiece != null)
            {
                currentHoveredPiece.HidePossiblePath();
                currentHoveredPiece = null;
            }
        }
    }
}
