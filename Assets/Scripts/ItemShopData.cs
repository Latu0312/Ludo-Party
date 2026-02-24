using UnityEngine;
using UnityEngine.UI;

public enum ItemCategory { Subscription, Dice, Skin, Board }

[System.Serializable]
public class ShopItemData
{
    public string itemId;
    public ItemCategory category;
    public int price;
    public bool isHardCurrency;
    public GameObject itemPrefab;

    
    public Button buyButton;
   
    public int rewardAmount;
}
