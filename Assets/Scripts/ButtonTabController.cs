using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonTabController : MonoBehaviour
{
    public enum AnimationType { None, Slide1, Slide2, Scale, SlideAndScale }

    [System.Serializable]
    public class ButtonSetting
    {
        public Button button;                
        public AnimationType animationType;  
    }

   
    [SerializeField] private ButtonSetting[] buttonSettings;

   
    [SerializeField] private float slideDistance = 20f;
    [SerializeField] private float slideTime = 0.15f;

   
    [SerializeField] private float scaleUpSize = 1.2f;
    [SerializeField] private float scaleTime = 0.15f;

    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private int currentIndex = -1;

    void Start()
    {
        int length = buttonSettings.Length;
        originalPositions = new Vector3[length];
        originalScales = new Vector3[length];

        for (int i = 0; i < length; i++)
        {
            int index = i;
            RectTransform rt = buttonSettings[i].button.GetComponent<RectTransform>();
            originalPositions[i] = rt.anchoredPosition;
            originalScales[i] = rt.localScale;

            buttonSettings[i].button.onClick.AddListener(() => OnButtonClicked(index));
            AddHoverEvents(buttonSettings[i].button, index);
        }

        
        StartCoroutine(SelectDefaultButton());
    }
    void AddHoverEvents(Button button, int index)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnButtonHoverEnter(index); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnButtonHoverExit(index); });
        trigger.triggers.Add(entryExit);
    }

    void OnButtonHoverEnter(int index)
    {
        if (buttonSettings[index].animationType == AnimationType.Scale ||
            buttonSettings[index].animationType == AnimationType.SlideAndScale)
        {
            RectTransform rt = buttonSettings[index].button.GetComponent<RectTransform>();
            rt.DOScale(originalScales[index] * scaleUpSize, scaleTime);
        }
    }

    void OnButtonHoverExit(int index)
    {
        if (buttonSettings[index].animationType == AnimationType.Scale ||
            buttonSettings[index].animationType == AnimationType.SlideAndScale)
        {
            RectTransform rt = buttonSettings[index].button.GetComponent<RectTransform>();
            rt.DOScale(originalScales[index], scaleTime);
        }
    }

    void OnButtonClicked(int index)
    {
        
        if (currentIndex != -1 && buttonSettings[currentIndex].animationType != AnimationType.None)
        {
            RectTransform rtOld = buttonSettings[currentIndex].button.GetComponent<RectTransform>();
            rtOld.DOAnchorPosY(originalPositions[currentIndex].y, slideTime).SetEase(Ease.OutQuad);
        }

        
        switch (buttonSettings[index].animationType)
        {
            case AnimationType.Slide1:
                PlaySlideAnimation(index);
                break;
            case AnimationType.Slide2:
                PlaySlide2Animation(index);
                break;
            case AnimationType.Scale:
                PlayScaleAnimation(index);
                break;
            case AnimationType.SlideAndScale:
                PlaySlideAnimation(index);
                PlayScaleAnimation(index);
                break;
        }

        currentIndex = index;
    }

    void PlaySlideAnimation(int index)
    {
        RectTransform rt = buttonSettings[index].button.GetComponent<RectTransform>();
        rt.DOAnchorPosY(originalPositions[index].y - slideDistance, slideTime).SetEase(Ease.OutQuad);
    }

    void PlayScaleAnimation(int index)
    {
        RectTransform rt = buttonSettings[index].button.GetComponent<RectTransform>();
        rt.DOScale(originalScales[index] * scaleUpSize, scaleTime)
          .OnComplete(() =>
          {
              rt.DOScale(originalScales[index], scaleTime);
          });
    }

    void PlaySlide2Animation(int index)
    {
        RectTransform rt = buttonSettings[index].button.GetComponent<RectTransform>();
        rt.DOAnchorPosY(originalPositions[index].y + slideDistance, slideTime).SetEase(Ease.OutQuad);
    }

    public void ResetAllButtons()
    {
        for (int i = 0; i < buttonSettings.Length; i++)
        {
            RectTransform rt = buttonSettings[i].button.GetComponent<RectTransform>();
            rt.DOKill();

            rt.anchoredPosition = originalPositions[i];
            rt.localScale = originalScales[i];
        }

        currentIndex = -1;
    }
    private IEnumerator SelectDefaultButton()
    {
        yield return null; 
        if (buttonSettings.Length > 0)
        {
            OnButtonClicked(0); 
        }
    }
}
