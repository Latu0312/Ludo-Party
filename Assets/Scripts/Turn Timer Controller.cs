using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class TurnTimerController : NetworkBehaviour
{
    public static TurnTimerController Instance;

    public TurnTimerUI P1Timer;
    public TurnTimerUI P2Timer;
    public TurnTimerUI P3Timer;
    public TurnTimerUI P4Timer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowTurnTimer(int playerId, float duration)
    {
        var timerUI = GetTimerUI(playerId);

        if (P1Timer != null) P1Timer.ResetTimer();
        if (P2Timer != null) P2Timer.ResetTimer();
        if (P3Timer != null) P3Timer.ResetTimer();
        if (P4Timer != null) P4Timer.ResetTimer();

        if (timerUI != null)
            timerUI.StartTimer(duration);
    }

    private TurnTimerUI GetTimerUI(int playerId)
    {
        return playerId switch
        {
            1 => P1Timer,
            2 => P2Timer,
            3 => P3Timer,
            4 => P4Timer,
            _ => null
        };
    }
}
