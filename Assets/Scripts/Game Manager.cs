using Fusion;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private List<Piece> allPieces = new List<Piece>();

    private void Awake()
    {

    }

    public override void Spawned()
    {

    }
    public void RegisterPiece(Piece piece)
    {
        allPieces.Add(piece);
        Debug.Log($"Đã thêm Piece: {piece.Object.Id}, Player ID: {piece.Object.InputAuthority.PlayerId}");
    }
    
    public List<Piece> GetPiecesAtTile(int tileIndex, Piece exclude = null, bool isGoalPath = false)
    {
        
        var allPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None).ToList();

        return allPieces
            .Where(p =>
                p != exclude &&
                (p.State == PieceState.OnPath || p.State == PieceState.Finished) &&  
                (
                    isGoalPath
                        ? (p.CurrentTileIndex >= p.commonPath.Count &&  
                           (p.CurrentTileIndex - p.commonPath.Count) == tileIndex &&  
                           p.Color == exclude.Color)  
                        : (p.CurrentTileIndex < p.commonPath.Count &&  
                           p.GetRealTileIndex() == tileIndex)
                )
            ).ToList();
    }
}
