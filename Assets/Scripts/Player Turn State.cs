using Fusion;
using UnityEngine;

public class PlayerTurnState : NetworkBehaviour
{
    [Networked] public bool HasRolled { get; set; }
    [Networked] public bool HasMoved { get; set; }

    [Networked] public int NoSixRollCount_P1 { get; set; }
    [Networked] public int NoSixRollCount_P2 { get; set; }
    [Networked] public int NoSixRollCount_P3 { get; set; }
    [Networked] public int NoSixRollCount_P4 { get; set; }


    public override void Spawned()
    {
        HasRolled = false;
        HasMoved = false;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ResetForNewTurn()
    {
        HasRolled = false;
        HasMoved = false;
        Debug.Log("[PlayerTurnState] Đã reset HasRolled và HasMoved cho lượt mới");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetMoved(bool value)
    {
        HasMoved = value;
        Debug.Log($"[PlayerTurnState] ResetMoved: {HasMoved}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetRolled(bool value)
    {
        HasRolled = value;
        Debug.Log($"[PlayerTurnState] ResetRolled: {HasRolled}");
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_IncrementNoSixRollCount(int playerID)
    {
        switch (playerID)
        {
            case 1: NoSixRollCount_P1++; break;
            case 2: NoSixRollCount_P2++; break;
            case 3: NoSixRollCount_P3++; break;
            case 4: NoSixRollCount_P4++; break;
        }

        Debug.Log($"[PlayerTurnState] Player {playerID} - NoSixRollCount tăng");
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_ResetNoSixRollCount(int playerID)
    {
        switch (playerID)
        {
            case 1: NoSixRollCount_P1 = 0; break;
            case 2: NoSixRollCount_P2 = 0; break;
            case 3: NoSixRollCount_P3 = 0; break;
            case 4: NoSixRollCount_P4 = 0; break;
        }

        Debug.Log($"[PlayerTurnState] Player {playerID} - NoSixRollCount reset về 0");
    }

    public int GetNoSixRollCount(int playerID)
    {
        return playerID switch
        {
            1 => NoSixRollCount_P1,
            2 => NoSixRollCount_P2,
            3 => NoSixRollCount_P3,
            4 => NoSixRollCount_P4,
            _ => 0
        };
    }

}
