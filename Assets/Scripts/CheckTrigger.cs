using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;

public class CheckTrigger : NetworkBehaviour
{
    public float stayTimeToTrigger = 1.0f; 
    private Coroutine stayCoroutine;
    [Networked] public EffectType effectType { get; set; } 

    private TextMeshProUGUI effectFrontText;

    public void SetEffectType(EffectType newType)
    {
        if (Object.HasStateAuthority)
        {
            effectType = newType;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Piece"))
        {
            stayCoroutine = StartCoroutine(CheckIfStayed(other));
            Debug.Log("[CheckTrigger] Piece vào vùng trigger, bắt đầu đếm thời gian.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Piece") && stayCoroutine != null)
        {
            StopCoroutine(stayCoroutine);
            stayCoroutine = null;
            Debug.Log("[CheckTrigger] Piece rời trigger, hủy đếm thời gian.");
        }
    }

    IEnumerator CheckIfStayed(Collider other)
    {
        yield return new WaitForSeconds(stayTimeToTrigger);

        if (other != null && other.bounds.Intersects(GetComponent<Collider>().bounds))
        {
            Debug.Log("[CheckTrigger] Đủ thời gian - kiểm tra và kích hoạt hiệu ứng.");
            TriggerEffect(other.gameObject);
        }
    }

    private void TriggerEffect(GameObject pieceObject)
    {
        Piece piece = pieceObject.GetComponent<Piece>();
        if (piece == null)
        {
            return;
        }

        if (piece.State != PieceState.OnPath)
        {
            return;
        }

        
        switch (effectType)
        {
            case EffectType.MoveForward1:
                piece.Effect_MoveForward(1);
                StartCoroutine(HintForEffect("Effect: Move Forward 1\n - Move the piece forward by 1 tile."));
                break;
            case EffectType.MoveForward2:
                piece.Effect_MoveForward(2);
                StartCoroutine(HintForEffect("Effect: Move Forward 2\n - Move the piece forward by 2 tiles."));
                break;
            case EffectType.MoveForward3:
                piece.Effect_MoveForward(3);
                StartCoroutine(HintForEffect("Effect: Move Forward 3\n - Move the piece forward by 3 tiles."));
                break;
            case EffectType.MoveForward4:
                piece.Effect_MoveForward(4);
                StartCoroutine(HintForEffect("Effect: Move Forward 4\n - Move the piece forward by 4 tiles."));
                break;
            case EffectType.MoveForward5:
                piece.Effect_MoveForward(5);
                StartCoroutine(HintForEffect("Effect: Move Forward 5\n - Move the piece forward by 5 tiles."));
                break;
            case EffectType.MoveBackward1:
                piece.Effect_MoveBackward(1);
                StartCoroutine(HintForEffect("Effect: Move Backward 1\n - Move the piece backward by 1 tile."));
                break;
            case EffectType.MoveBackward2:
                piece.Effect_MoveBackward(2);
                StartCoroutine(HintForEffect("Effect: Move Backward 2\n - Move the piece backward by 2 tiles."));
                break;
            case EffectType.MoveBackward3:
                piece.Effect_MoveBackward(3);
                StartCoroutine(HintForEffect("Effect: Move Backward 3\n - Move the piece backward by 3 tiles."));
                break;
            case EffectType.MoveBackward4:
                piece.Effect_MoveBackward(4);
                StartCoroutine(HintForEffect("Effect: Move Backward 4\n - Move the piece backward by 4 tiles."));
                break;
            case EffectType.MoveBackward5:
                piece.Effect_MoveBackward(5);
                StartCoroutine(HintForEffect("Effect: Move Backward 5\n - Move the piece backward by 5 tiles."));
                break;
            case EffectType.SendToBase:
                piece.ReturnToBase();
                StartCoroutine(HintForEffect("Effect: Send to Base\n - Send the piece back to its base."));
                break;
            case EffectType.KickRandomOpponent:
                piece.Effect_KickRandomOpponent();
                StartCoroutine(HintForEffect("Effect: Kick Random Opponent\n - Kick a random opponent's piece back to their base."));
                break;
            case EffectType.AutoSpawn1Piece:
                piece.Effect_AutoSpawn1Piece();
                StartCoroutine(HintForEffect("Effect: Auto Spawn 1 Piece\n - Automatically spawn a new piece for the player."));
                break;
            case EffectType.ReturnToSpawnPoint:
                piece.Effect_ReturnToSpawnPoint();
                StartCoroutine(HintForEffect("Effect: Return to Spawn Point\n - Return the piece to its spawn point."));
                break;
            case EffectType.HasShieldBlockKick:
                piece.Effet_HasShieldBlockKick();
                StartCoroutine(HintForEffect("Effect: Has Shield Block Kick\n - The piece has a shield that can blocks a kicks."));
                break;
            case EffectType.StartRotatePiece:
                piece.Effect_StartRotatePiece();
                StartCoroutine(HintForEffect("Effect: Start Rotate Piece\n - The piece starts rotating for 60s."));
                break;
            case EffectType.SwapRandomOpponent:
                piece.Effect_SwapRandomOpponent();
                StartCoroutine(HintForEffect("Effect: Swap Random Opponent\n - Swap positions with a random opponent's piece."));
                break;
            case EffectType.SpawnAndSwapRandomOpponentToBase:
                piece.Effect_SpawnAndSwapRandomOpponentToBase();
                StartCoroutine(HintForEffect("Effect: Spawn and Swap Random Opponent to Base\n - Spawn a new piece at random opponent's piece position and swap its to their base."));
                break;
            case EffectType.Effect_HasBomb:
                piece.Effect_HasBomb();
                StartCoroutine(HintForEffect("Effect: Has Bomb\n - The piece has a bomb that can be kick its and attacker return to base."));
                break;
            case EffectType.RandomOpponentMoveFoward1:
                piece.Effect_RandomOpponentMoveFoward(1);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Forward 1\n - Move a random opponent's piece forward by 1 tile."));
                break;
            case EffectType.RandomOpponentMoveFoward2:
                piece.Effect_RandomOpponentMoveFoward(2);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Forward 2\n - Move a random opponent's piece forward by 2 tiles."));
                break;
            case EffectType.RandomOpponentMoveFoward3:
                piece.Effect_RandomOpponentMoveFoward(3);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Forward 3\n - Move a random opponent's piece forward by 3 tiles."));
                break;
            case EffectType.RandomOpponentMoveFoward4:
                piece.Effect_RandomOpponentMoveFoward(4);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Forward 4\n - Move a random opponent's piece forward by 4 tiles."));
                break;
            case EffectType.RandomOpponentMoveFoward5:
                piece.Effect_RandomOpponentMoveFoward(5);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Forward 5\n - Move a random opponent's piece forward by 5 tiles."));
                break;
            case EffectType.RandomOpponentMoveBackward1:
                piece.Effect_RandomOpponentMoveBackward(1);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Backward 1\n - Move a random opponent's piece backward by 1 tile."));
                break;
            case EffectType.RandomOpponentMoveBackward2:
                piece.Effect_RandomOpponentMoveBackward(2);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Backward 2\n - Move a random opponent's piece backward by 2 tiles."));
                break;
            case EffectType.RandomOpponentMoveBackward3:
                piece.Effect_RandomOpponentMoveBackward(3);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Backward 3\n - Move a random opponent's piece backward by 3 tiles."));
                break;
            case EffectType.RandomOpponentMoveBackward4:
                piece.Effect_RandomOpponentMoveBackward(4);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Backward 4\n - Move a random opponent's piece backward by 4 tiles."));
                break;
            case EffectType.RandomOpponentMoveBackward5:
                piece.Effect_RandomOpponentMoveBackward(5);
                StartCoroutine(HintForEffect("Effect: Random Opponent Move Backward 5\n - Move a random opponent's piece backward by 5 tiles."));
                break;
            case EffectType.FlyToGoalDoor:
                piece.Effect_FlyToGoalDoor();
                StartCoroutine(HintForEffect("Effect: Fly To Goal Door.\n - This piece auto fly to goal door."));
                break;
            default:
                Debug.Log("[CheckTrigger] Không có hiệu ứng nào để kích hoạt.");
                return;
        }

        Debug.Log($"[CheckTrigger] Đã kích hoạt effect: {effectType}");

        
        if (Object.HasStateAuthority)
        {
            Debug.Log($"[CheckTrigger] Despawn effect object: {gameObject.name}");
            Runner.Despawn(Object);
        }
    }

    IEnumerator HintForEffect(string effectText)
    {
        effectFrontText = GameObject.Find("Canvas/Effect Text").GetComponent<TextMeshProUGUI>();

        effectFrontText.text = effectText;
        
        yield return new WaitForSeconds(8f); 

        effectFrontText.text = ""; 
    }
}
