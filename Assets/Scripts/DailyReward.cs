using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using System;

public class DailyReward : MonoBehaviour
{
    [SerializeField] private Image focus;
    [SerializeField] private Image giftSpot;
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private Sprite CollectedSprite;
    [SerializeField] private GameObject checkMark;
    [SerializeField] private Button claimButton;
    [SerializeField] private TextMeshProUGUI timeLeft;

    void Start()
    {
        string lastTime = PlayerPrefs.GetString("LastClaimTime", "");

        DateTime lastClaimTime;

        if (!string.IsNullOrEmpty(lastTime))
        {
            lastClaimTime = DateTime.Parse(lastTime);
        }
        else
        {
            lastClaimTime = DateTime.MinValue;
        }
        
        if (DateTime.Now.Date > lastClaimTime.Date)
        {
            claimButton.interactable = true;
        }
        else
        {
            claimButton.interactable = false;
            timeLeft.text = GetTimeToNextClaim();
        }
    }

    private string GetTimeToNextClaim()
    {
        int hours = Mathf.FloorToInt((float)(DateTime.Today.AddDays(1) - DateTime.Now).TotalHours);
        int minutes = Mathf.FloorToInt((float)(DateTime.Today.AddDays(1) - DateTime.Now).TotalMinutes) %60;
        return (hours + " hours and " + minutes + " minutes left to claim next prize");
    }


    public void OnClaimButtonPressed()
    {
        PlayerPrefs.SetString("LastClaimTime", DateTime.Now.ToString());

        CliamGift();
    }

    public void CliamGift()
    {
        claimButton.interactable = false;
        checkMark.SetActive(true);
        giftSpot.sprite = CollectedSprite;
        focus.enabled = false;
        TitleText.text = "Daily Login Rewards<color=#f6e19c>3 </color>/ 7";
        timeLeft.text = GetTimeToNextClaim();
    }
}
