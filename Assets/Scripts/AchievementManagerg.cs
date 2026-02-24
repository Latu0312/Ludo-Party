using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;

public class AchievementManager : MonoBehaviour
{
   
    public List<AchievementUI> achievementPrefabs; 

    private string token;
    private DatabaseReference dbRef;
    private int currentExp;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        StartCoroutine(InitThenLoadData());
    }

  
    IEnumerator InitThenLoadData()
    {
       

        token = UserSession.Token;
        

        ListenToPlayerExperience(); 
        yield return null;
    }

    void ListenToPlayerExperience()
    {
        dbRef.Child("users").Child(token).Child("experience")
            .ValueChanged += (object sender, ValueChangedEventArgs e) =>
            {
                if (e.Snapshot == null || e.Snapshot.Value == null)
                {
                    Debug.LogError("Không đọc được experience người chơi");
                    return;
                }

                int.TryParse(e.Snapshot.Value.ToString(), out currentExp);

                
                UpdateAchievementsUI();
            };
    }

    void UpdateAchievementsUI()
    {
        foreach (var achUI in achievementPrefabs)
        {
            
            achUI.requirementText.text = $" /{achUI.requiredExperience} ";
            achUI.rewardText.text = $"{achUI.rewardAmount}";

            
            int maxExp = 100;
            achUI.expSlider.maxValue = maxExp;
            achUI.expSlider.value = currentExp % maxExp;
            achUI.expText.text = $"EXP: {currentExp}";

           
            achUI.expSlider.interactable = false;

            
            achUI.claimButton.interactable = !achUI.isClaimed && currentExp >= achUI.requiredExperience;

            
            achUI.claimButton.onClick.RemoveAllListeners();
            achUI.claimButton.onClick.AddListener(() => ClaimAchievement(achUI));
        }
    }

    void ClaimAchievement(AchievementUI achUI)
    {
        if (achUI.isClaimed || currentExp < achUI.requiredExperience)
        {
            Debug.Log("Chưa đủ exp hoặc đã nhận rồi");
            return;
        }

        DatabaseReference userRef = dbRef.Child("users").Child(token);

        userRef.RunTransaction(mutableData =>
        {
            var dict = mutableData.Value as Dictionary<string, object>;
            if (dict == null) dict = new Dictionary<string, object>();

            
            Dictionary<string, object> currencyDict;
            if (dict.ContainsKey("currency"))
            {
                currencyDict = dict["currency"] as Dictionary<string, object>;
            }
            else
            {
                currencyDict = new Dictionary<string, object>();
                dict["currency"] = currencyDict;
            }

            switch (achUI.rewardType)
            {
                case "softCurrency":
                    int soft = currencyDict.ContainsKey("softCurrency") ? int.Parse(currencyDict["softCurrency"].ToString()) : 0;
                    currencyDict["softCurrency"] = soft + achUI.rewardAmount;
                    break;
                case "hardCurrency":
                    int hard = currencyDict.ContainsKey("hardCurrency") ? int.Parse(currencyDict["hardCurrency"].ToString()) : 0;
                    currencyDict["hardCurrency"] = hard + achUI.rewardAmount;
                    break;
                case "experience":
                    int exp = dict.ContainsKey("experience") ? int.Parse(dict["experience"].ToString()) : 0;
                    dict["experience"] = exp + achUI.rewardAmount;
                    break;
            }

            mutableData.Value = dict;
            return TransactionResult.Success(mutableData);
        }).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Lỗi khi cập nhật thưởng: " + task.Exception);
                return;
            }

            achUI.isClaimed = true;
            achUI.claimButton.interactable = false;

            Debug.Log("Đã nhận thành tựu: " + achUI.id);
        });
    }

    [System.Serializable]
    public class AchievementUI
    {
        public string id;
        public int requiredExperience;
        public string rewardType;   
        public int rewardAmount;
        public bool isClaimed;

        [Header("UI Binding")]
        public TextMeshProUGUI requirementText;
        public TextMeshProUGUI rewardText;
        public Button claimButton;

       
        public Slider expSlider;
        public TextMeshProUGUI expText;
    }
}
