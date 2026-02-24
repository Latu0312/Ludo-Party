using UnityEngine;
using UnityEngine.UI;

public class TurnTimerUI : MonoBehaviour
{
    public Image timerRing;
    private float timer;
    private float duration;
    private bool counting;

    public void StartTimer(float seconds)
    {
        duration = seconds;
        timer = seconds;
        counting = true;

        timerRing.fillAmount = 1f;
        timerRing.enabled = true;
    }

    public void ResetTimer()
    {
        counting = false;
        timer = 0f;
        duration = 0f;
        timerRing.fillAmount = 0f;
    }


    void Update()
    {
        if (!counting) return;

        timer -= Time.deltaTime;
        timerRing.fillAmount = Mathf.Clamp01(timer / duration);

        if (timer <= 0f)
        {
            counting = false;
            
        }
    }
}
