using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AvatarSelector : MonoBehaviour
{
    
    
    public Image previewImage;        
    public Button[] optionButtons;    
    public Sprite[] optionSprites;    
    public Image mainScreenAvatar;    

    private int selectedIndex = -1;   

    void Start()
    {
        
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }

        
        int savedIndex = PlayerPrefs.GetInt("SelectedAvatar", -1);
        if (savedIndex >= 0 && savedIndex < optionSprites.Length)
        {
            selectedIndex = savedIndex;
            previewImage.sprite = optionSprites[savedIndex];
            mainScreenAvatar.sprite = optionSprites[savedIndex];
        }
    }

    void SelectAvatar(int index)
    {
        selectedIndex = index;
        previewImage.sprite = optionSprites[index];
    }

   
    public void ConfirmSelection()
    {
        if (selectedIndex >= 0 && selectedIndex < optionSprites.Length)
        {
            
            mainScreenAvatar.sprite = optionSprites[selectedIndex];

            
            PlayerPrefs.SetInt("SelectedAvatar", selectedIndex);
            PlayerPrefs.Save();
        }
    }
}
