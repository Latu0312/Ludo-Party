using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UserDataLoader : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI softCurrencyText;
    public TextMeshProUGUI hardCurrencyText;

   
    public TextMeshProUGUI tokenFileText;  

   
    public List<ShopItemData> allShopItems;

    private DatabaseReference dbRef;
    private DatabaseReference currencyRef;

    void Update()
    {
        StartCoroutine(InitWithToken());
    }

    IEnumerator InitWithToken()
    {
       
        while (string.IsNullOrEmpty(UserSession.Token))
            yield return new WaitForSeconds(0.2f);

        string token = UserSession.Token.Trim();

     
        if (tokenFileText != null)
            tokenFileText.text = "Token: " + (string.IsNullOrEmpty(token) ? "(trống)" : token);

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Token rỗng, không thể tải dữ liệu Firebase.");
            yield break;
        }

      
        dbRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(token);
        currencyRef = dbRef.Child("currency");

        LoadUserData();
        ListenForCurrencyChanges();
        CheckOwnedItems(token);
    }

    void LoadUserData()
    {
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Lỗi khi lấy dữ liệu người dùng: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string username = snapshot.Child("username").Value?.ToString() ?? "Unknown";
                string hardCurrency = snapshot.Child("currency").Child("hardCurrency").Value?.ToString() ?? "0";
                string softCurrency = snapshot.Child("currency").Child("softCurrency").Value?.ToString() ?? "0";

                if (usernameText != null) usernameText.text = "" + username;
                if (hardCurrencyText != null) hardCurrencyText.text = "" + hardCurrency;
                if (softCurrencyText != null) softCurrencyText.text = "" + softCurrency;
            }
        });
    }

    void ListenForCurrencyChanges()
    {
        currencyRef.ValueChanged += (s, e) =>
        {
            if (e.Snapshot != null && e.Snapshot.Exists)
            {
                string hard = e.Snapshot.Child("hardCurrency").Value?.ToString() ?? "0";
                string soft = e.Snapshot.Child("softCurrency").Value?.ToString() ?? "0";

                if (hardCurrencyText != null) hardCurrencyText.text = "" + hard;
                if (softCurrencyText != null) softCurrencyText.text = "" + soft;
            }
        };
    }

    void CheckOwnedItems(string token)
    {
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                foreach (var item in allShopItems)
                {
                    if (item.category == ItemCategory.Subscription) continue;

                    string ownedKey = item.category.ToString() + "Owned";
                    var ownedNode = snapshot.Child(ownedKey);

                    if (ownedNode.Exists && ownedNode.HasChild(item.itemId))
                    {
                        bool owned = (bool)ownedNode.Child(item.itemId).Value;
                        if (owned && item.buyButton != null)
                        {
                            item.buyButton.interactable = false;
                            Debug.Log($"Đã sở hữu {item.itemId}, tắt nút mua.");
                        }
                    }
                }
            }
        });
    }
}
