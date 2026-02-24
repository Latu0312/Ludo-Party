using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class DailyRewardTimerUI : MonoBehaviour
{

    private string userToken;
    private DatabaseReference dbRef;

    private DateTime nextAvailableTime;
    private bool canClaim = false;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        
        userToken = UserSession.Token;

        if (string.IsNullOrEmpty(userToken))
        {
            Debug.LogError("UserSession.Token is null or empty!");
            return;
        }

        LoadLastClaimTime();
    }

    void LoadLastClaimTime()
    {
        dbRef.Child("users").Child(userToken).Child("dailyReward").Child("lastClaimDate")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    string lastDateStr = task.Result.Value?.ToString();

                    if (DateTime.TryParse(lastDateStr, out DateTime lastClaim))
                    {
                        nextAvailableTime = lastClaim.Date.AddDays(1); 
                        canClaim = DateTime.Now >= nextAvailableTime;
                    }
                    else
                    {
                       
                        canClaim = true;
                    }
                }
            });
    }

    
}
