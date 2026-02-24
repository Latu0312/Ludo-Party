using Unity.VisualScripting;
using UnityEngine;

public class PanelActive : MonoBehaviour
{
    [SerializeField] private GameObject[] panels; 
    private GameObject currentActivePanel; 
    ButtonTabController buttonTabController;
   public void Update()
    {
        if(buttonTabController == null)
        {
            buttonTabController = FindFirstObjectByType<ButtonTabController>();
        }
    }
    public void TogglePanel(int panelIndex)
    {
        if (panelIndex >= 0 && panelIndex < panels.Length && panels[panelIndex] != null)
        {
          
            if (currentActivePanel == panels[panelIndex] && panels[panelIndex].activeSelf)
            {
                panels[panelIndex].SetActive(false);
                currentActivePanel = null;
            }
            else
            {
               
                if (currentActivePanel != null)
                {
                    currentActivePanel.SetActive(false);
                }
               
                panels[panelIndex].SetActive(true);
                currentActivePanel = panels[panelIndex];
            }
        }
    }

   
    public void ShowPanel(int panelIndex)
    {
        if (panelIndex >= 0 && panelIndex < panels.Length && panels[panelIndex] != null)
        {
           
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null) panels[i].SetActive(false);
            }
            
            panels[panelIndex].SetActive(true);
            currentActivePanel = panels[panelIndex];
            buttonTabController?.ResetAllButtons(); 
        }
    }

    
    public void HideAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        currentActivePanel = null;
    }
    public void ShowFriend(int panelIndex)
    {
        if (panelIndex >= 0 && panelIndex < panels.Length && panels[panelIndex] != null)
        {
            
            if (currentActivePanel == panels[panelIndex] && panels[panelIndex].activeSelf)
            {
                panels[panelIndex].SetActive(false);
                currentActivePanel = null;
            }
            else
            {
               
                if (currentActivePanel != null)
                {
                    currentActivePanel.SetActive(true);
                }
                
                panels[panelIndex].SetActive(true);
                currentActivePanel = panels[panelIndex];
            }
        }

    }
}