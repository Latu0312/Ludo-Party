using System;
using UnityEngine;
using TMPro;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using Firebase.Database;

public class RewardedAdManager : MonoBehaviour
{
    private RewardedAd rewardedAd;
    public TMP_Text currencyText;

    private string userId;
    private DatabaseReference dbRef;
    private DatabaseReference userCurrencyRef;

#if UNITY_ANDROID
    private string adUnitId = "ca-app-pub-7265362355325372~6929495499";
#elif UNITY_IOS
    private string adUnitId = "ca-app-pub-7265362355325372~6929495499";
#else
    private string adUnitId = "ca-app-pub-7265362355325372~6929495499";
#endif

    void Start()
    {
        
        userId = UserSession.Token?.Trim();

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("UserSession.Token is null or empty!");
            return;
        }

        Debug.Log("Loaded userId from UserSession: " + userId);

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        userCurrencyRef = dbRef.Child("users").Child(userId).Child("currency").Child("softCurrency");

        MobileAds.Initialize(initStatus =>
        {
            LoadRewardedAd();
        });

        
        userCurrencyRef.ValueChanged += HandleSoftCurrencyChanged;
    }

    void LoadRewardedAd()
    {
        var adRequest = new AdRequest();

        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Failed to load rewarded ad: " + error?.GetMessage());
                return;
            }

            rewardedAd = ad;
            RegisterCallbacks(ad);
        });
    }

    void RegisterCallbacks(RewardedAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Ad shown");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Ad dismissed");
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Ad failed to show: " + error.GetMessage());
        };

        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Ad impression recorded");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("Ad clicked");
        };
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("User earned reward: " + reward.Amount);
                AddRewardToFirebase();
            });
        }
        else
        {
            Debug.LogWarning("Ad not ready.");
            LoadRewardedAd();
        }
    }

    private void AddRewardToFirebase()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("userId is null or empty. Cannot proceed.");
            return;
        }

        userCurrencyRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to get current softCurrency: " + task.Exception);
                return;
            }

            int currentSoftCurrency = 0;
            if (task.Result.Exists)
            {
                int.TryParse(task.Result.Value.ToString(), out currentSoftCurrency);
            }

            int updatedCurrency = currentSoftCurrency + 100;

            Debug.Log($"[Firebase] Updating softCurrency for user {userId}: {updatedCurrency}");

            userCurrencyRef.SetValueAsync(updatedCurrency).ContinueWith(updateTask =>
            {
                if (updateTask.IsFaulted || updateTask.IsCanceled)
                {
                    Debug.LogError("Failed to update softCurrency: " + updateTask.Exception);
                    return;
                }

                Debug.Log("Successfully updated softCurrency!");
                
            });
        });
    }

   
    private void HandleSoftCurrencyChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Failed to listen for currency updates: " + args.DatabaseError.Message);
            return;
        }

        string value = args.Snapshot?.Value?.ToString() ?? "0";

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            currencyText.text = value;
            Debug.Log("[Firebase] Real-time softCurrency updated to: " + value);
        });
    }

 
    private void OnDestroy()
    {
        if (userCurrencyRef != null)
        {
            userCurrencyRef.ValueChanged -= HandleSoftCurrencyChanged;
        }
    }
}
