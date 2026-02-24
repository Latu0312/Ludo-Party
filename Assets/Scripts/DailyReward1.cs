using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    
    public Transform rewardsPanel; 

    private DailyRewardItem[] rewardItems;

    private DatabaseReference dbRef;
    private string userToken;
    private string todayDate;
    private int currentStreak;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

       
        userToken = UserSession.Token;

        if (string.IsNullOrEmpty(userToken))
        {
            Debug.LogError("Token rỗng, không thể tải Daily Reward.");
            return;
        }

        todayDate = DateTime.Now.ToString("yyyy-MM-dd");

        rewardItems = rewardsPanel.GetComponentsInChildren<DailyRewardItem>();
        LoadRewardState();
    }

    void LoadRewardState()
    {
        dbRef.Child("users").Child(userToken).Child("dailyReward").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                string lastDate = snapshot.Child("lastClaimDate").Value?.ToString() ?? "";
                int streak = int.TryParse(snapshot.Child("currentStreak").Value?.ToString(), out int s) ? s : 0;

                DateTime lastClaim;
                if (DateTime.TryParse(lastDate, out lastClaim))
                {
                    TimeSpan diff = DateTime.Now.Date - lastClaim.Date;
                    currentStreak = (diff.Days == 1) ? streak + 1 : (diff.Days == 0 ? streak : 1);
                    if (diff.Days > 1) ResetClaimedDays();
                }
                else
                {
                    currentStreak = 1;
                }

                SetupAllRewardItems(snapshot);
            }
        });
    }

    void SetupAllRewardItems(DataSnapshot snapshot)
    {
        bool hasClaimedToday = snapshot.Child("lastClaimDate").Value?.ToString() == todayDate;

        foreach (var item in rewardItems)
        {
            item.SetupDisplay();

            int dayIndex = item.dayIndex;
            string dayKey = "day" + dayIndex;

            bool claimed = snapshot.Child("claimedDays").Child(dayKey).Value?.ToString() == "true";
            bool canClaim = (dayIndex == currentStreak && !claimed && !hasClaimedToday);

            item.SetState(canClaim, claimed);
            item.claimButton.onClick.RemoveAllListeners();

            if (canClaim)
            {
                item.claimButton.onClick.AddListener(() =>
                    ClaimReward(item, dayKey));
            }
        }
    }

    void ClaimReward(DailyRewardItem item, string dayKey)
    {
        string targetPath = item.rewardType == "experience"
            ? $"users/{userToken}/experience"
            : $"users/{userToken}/currency/{item.rewardType}";

        dbRef.Child(targetPath).RunTransaction(mutableData =>
        {
            int current = mutableData.Value == null ? 0 : Convert.ToInt32(mutableData.Value);
            mutableData.Value = current + item.rewardAmount;
            return TransactionResult.Success(mutableData);
        });

        
        DatabaseReference dailyRef = dbRef.Child("users").Child(userToken).Child("dailyReward");
        dailyRef.Child("lastClaimDate").SetValueAsync(todayDate);
        dailyRef.Child("currentStreak").SetValueAsync(currentStreak);
        dailyRef.Child("claimedDays").Child(dayKey).SetValueAsync(true).ContinueWithOnMainThread(t =>
        {
            LoadRewardState(); 
        });
    }

    void ResetClaimedDays()
    {
        var claimedRef = dbRef.Child("users").Child(userToken).Child("dailyReward").Child("claimedDays");
        for (int i = 1; i <= 7; i++)
        {
            claimedRef.Child("day" + i).SetValueAsync(null);
        }
    }
}
