using UnityEngine;

public class ShowPanel : MonoBehaviour
{
   
    public GameObject panel;
    public void Show()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }
    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    public void Toggle()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }
}
