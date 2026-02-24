using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardItem : MonoBehaviour
{
   
    public int dayIndex = 1;
    public string rewardType = "softCurrency";
    public int rewardAmount = 50;

   
    public TextMeshProUGUI rewardText;
    public Button claimButton;
    public CanvasGroup canvasGroup;

    public void SetupDisplay()
    {
        string icon = rewardType == "softCurrency" ? "" :
                      rewardType == "hardCurrency" ? "" : "";
        rewardText.text = $"Day {dayIndex}";
    }

    public void SetState(bool canClaim, bool claimed)
    {
        if (claimed)
        {
            rewardText.text += "\n✅";
            claimButton.interactable = false;
            canvasGroup.alpha = 0.6f;
        }
        else if (canClaim)
        {
            claimButton.interactable = true;
            canvasGroup.alpha = 1f;
        }
        else
        {
            claimButton.interactable = false;
            canvasGroup.alpha = 0.3f;
        }
    }
}
