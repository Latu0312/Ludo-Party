using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string username;
    public int experience;
    public Currency currency;
    public List<string> skinsOwned;

    public PlayerData()
    {
        currency = new Currency();
        skinsOwned = new List<string>();
    }
}
