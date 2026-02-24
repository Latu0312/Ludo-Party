using UnityEngine;
using UnityEngine.UI;

public class ButtonAlphaController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonAlphaSetting
    {
        public Button button;    
        public Image targetImage; 
        public bool isDefault;   
    }

   
    [SerializeField] private ButtonAlphaSetting[] buttonAlphas;

    private int currentIndex = -1;

    void Start()
    {
        for (int i = 0; i < buttonAlphas.Length; i++)
        {
            int index = i;
            if (buttonAlphas[i].button != null)
                buttonAlphas[i].button.onClick.AddListener(() => OnButtonClicked(index));

            
            if (buttonAlphas[i].targetImage != null)
            {
                Color c = buttonAlphas[i].targetImage.color;
                c.a = buttonAlphas[i].isDefault ? 1f : 0f;
                buttonAlphas[i].targetImage.color = c;

                if (buttonAlphas[i].isDefault) currentIndex = i;
            }
        }
    }

    void OnButtonClicked(int index)
    {
       
        if (currentIndex != -1 && buttonAlphas[currentIndex].targetImage != null)
        {
            Color c = buttonAlphas[currentIndex].targetImage.color;
            c.a = 0f;
            buttonAlphas[currentIndex].targetImage.color = c;
        }

       
        if (buttonAlphas[index].targetImage != null)
        {
            Color c = buttonAlphas[index].targetImage.color;
            c.a = 1f;
            buttonAlphas[index].targetImage.color = c;
        }

        currentIndex = index;
    }
}
