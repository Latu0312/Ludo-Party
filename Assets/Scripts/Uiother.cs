using UnityEngine;

public class Uiother : MonoBehaviour
{
    
    public RectTransform createButton;
    public RectTransform joinButton;

    public void BringCreateToFront()
    {
        createButton.SetAsLastSibling(); 
        joinButton.SetAsFirstSibling();  
    }

    public void BringJoinToFront()
    {
        joinButton.SetAsLastSibling(); 
        createButton.SetAsFirstSibling(); 
    }

}
