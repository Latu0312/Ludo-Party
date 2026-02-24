using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShopManager : MonoBehaviour
{
   
    public List<ShopItemData> subscriptions;
    public List<ShopItemData> dices;
    public List<ShopItemData> skins;
    public List<ShopItemData> boards;

   
    public GameObject errorPanel;
    public Text errorText;

    private string userToken;
    private DatabaseReference dbRef;

    private void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        
        userToken = UserSession.Token;

        if (string.IsNullOrEmpty(userToken))
        {
            ShowError("Token rỗng, không thể kết nối Firebase.");
            return;
        }

        SetupButtons(subscriptions);
        SetupButtons(dices);
        SetupButtons(skins);
        SetupButtons(boards);
    }

    private void SetupButtons(List<ShopItemData> items)
    {
        foreach (var item in items)
        {
            if (item.buyButton != null)
            {
                item.buyButton.onClick.RemoveAllListeners();
                item.buyButton.onClick.AddListener(() => BuyItem(item));
            }
        }
    }

    public void BuyItem(ShopItemData item)
    {
        string ownedKey = item.category.ToString() + "Owned";
        DatabaseReference currencyRef = dbRef.Child("users").Child(userToken).Child("currency");
        DatabaseReference ownedRef = dbRef.Child("users").Child(userToken).Child(ownedKey);

        currencyRef.RunTransaction(mutableData =>
        {
            var currency = mutableData.Value as Dictionary<string, object> ?? new Dictionary<string, object>();
            int softCurrency = currency.ContainsKey("softCurrency") ? Convert.ToInt32(currency["softCurrency"]) : 0;
            int hardCurrency = currency.ContainsKey("hardCurrency") ? Convert.ToInt32(currency["hardCurrency"]) : 0;

            int current = item.isHardCurrency ? hardCurrency : softCurrency;

            if (current < item.price)
            {
                ShowErrorLocal("Không đủ tiền để mua vật phẩm.");
                return TransactionResult.Abort();
            }

            if (item.isHardCurrency)
            {
                hardCurrency -= item.price;
                currency["hardCurrency"] = hardCurrency;
            }
            else
            {
                softCurrency -= item.price;
                currency["softCurrency"] = softCurrency;
            }

            
            if (item.category == ItemCategory.Subscription && item.rewardAmount > 0)
            {
                softCurrency += item.rewardAmount;
                currency["softCurrency"] = softCurrency;
            }

            mutableData.Value = currency;
            return TransactionResult.Success(mutableData);
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
               
                if (item.category != ItemCategory.Subscription)
                {
                    ownedRef.Child(item.itemId).GetValueAsync().ContinueWithOnMainThread(checkTask =>
                    {
                        if (checkTask.IsCompleted && checkTask.Result.Exists && (bool)checkTask.Result.Value == true)
                        {
                            ShowErrorLocal("Bạn đã sở hữu vật phẩm này.");
                        }
                        else
                        {
                          
                            ownedRef.Child(item.itemId).SetValueAsync(true).ContinueWithOnMainThread(ownedTask =>
                            {
                                if (ownedTask.IsCompleted)
                                {
                                    Debug.Log("Mua thành công: " + item.itemId);
                                    errorPanel.SetActive(false);
                                    if (item.buyButton != null) item.buyButton.interactable = false;
                                }
                                else
                                {
                                    ShowErrorLocal("Lỗi khi lưu vật phẩm đã mua.");
                                }
                            });
                        }
                    });
                }
                else
                {
                    
                    ownedRef.Child(System.DateTime.UtcNow.Ticks.ToString()).SetValueAsync(item.itemId);
                    Debug.Log("Mua gói subscription thành công: " + item.itemId);
                    errorPanel.SetActive(false);
                }
            }
            else
            {
                ShowErrorLocal("Không đủ tiền hoặc lỗi trừ tiền.");
            }
        });
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorPanel.SetActive(true);
    }

    private void ShowErrorLocal(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => ShowError(msg));
    }
}
