using UnityEngine;

public class UiShop : MonoBehaviour
{
    
    [SerializeField] private GameObject panelsub;
    [SerializeField] private GameObject paneldice;
    [SerializeField] private GameObject panelcha;
    [SerializeField] private GameObject panelboard;
    [SerializeField] private GameObject panelmain;
    public void ShowPanelSub()
    {
        panelsub.SetActive(true);
        paneldice.SetActive(false);
        panelcha.SetActive(false);
        panelboard.SetActive(false);
    }
    public void ShowPanelDice()
    {
        panelsub.SetActive(false);
        paneldice.SetActive(true);
        panelcha.SetActive(false);
        panelboard.SetActive(false);
    }
    public void ShowPanelCha()
    {
        panelsub.SetActive(false);
        paneldice.SetActive(false);
        panelcha.SetActive(true);
        panelboard.SetActive(false);
    }
    public void ShowPanelBoard()
    {
        panelsub.SetActive(false);
        paneldice.SetActive(false);
        panelcha.SetActive(false);
        panelboard.SetActive(true);
    }
    public void ShowPanelMain()
    {
        panelsub.SetActive(false);
        paneldice.SetActive(false);
        panelcha.SetActive(false);
        panelboard.SetActive(false);
        panelmain.SetActive(true);
    }

}