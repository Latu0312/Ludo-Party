using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

[System.Serializable]
public class UIUnit
{
    public string name;
    public RectTransform target;
    public CanvasGroup canvasGroup;

    
    public bool fadeIn = true;
    public float fadeInDuration = 0.5f;

    public bool pulsing = false;
    public float pulseScale = 1.1f;
    public float pulseDuration = 0.6f;

    public bool moveToTarget = false;
    public Vector2 targetPosition;
    public float moveDuration = 0.5f;

    public bool fadeOut = false;
    public float fadeOutAfterSeconds = 2f;

  
    public bool rotateOnClick = true;
    public float rotateDegrees = 360f;
    public float rotateDuration = 0.5f;
}

public class MultiUIManager : MonoBehaviour
{
    public List<UIUnit> uiList = new List<UIUnit>();
    private bool[] isHolding;
    public float holdRotateSpeed = 180f; 

    void Awake()
    {
        isHolding = new bool[uiList.Count];
    }

    void Start()
    {
        foreach (var ui in uiList)
        {
            InitUI(ui);
        }
    }
    void Update()
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            if (isHolding[i] && uiList[i].rotateOnClick && uiList[i].target != null)
            {
                uiList[i].target.Rotate(0, 0, -holdRotateSpeed * Time.deltaTime); 
            }
        }
    }


    void InitUI(UIUnit ui)
    {
        if (ui.target == null) return;

       
        if (ui.canvasGroup == null)
        {
            ui.canvasGroup = ui.target.GetComponent<CanvasGroup>();
            if (ui.canvasGroup == null)
                ui.canvasGroup = ui.target.gameObject.AddComponent<CanvasGroup>();
        }

       
        if (ui.fadeIn)
        {
            ui.canvasGroup.alpha = 0;
            ui.canvasGroup.DOFade(1f, ui.fadeInDuration);
        }

        
        if (ui.pulsing)
        {
            ui.target.DOScale(Vector3.one * ui.pulseScale, ui.pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        
        if (ui.moveToTarget)
        {
            ui.target.DOAnchorPos(ui.targetPosition, ui.moveDuration).SetEase(Ease.OutQuad);
        }

        
        if (ui.fadeOut)
        {
            ui.canvasGroup.DOFade(0f, 0.5f).SetDelay(ui.fadeOutAfterSeconds);
        }
    }

  
    public void RotateUI(int index)
    {
        if (index < 0 || index >= uiList.Count) return;

        var ui = uiList[index];
        if (!ui.rotateOnClick || ui.target == null) return;

        ui.target.DOLocalRotate(new Vector3(0, 0, ui.rotateDegrees), ui.rotateDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                ui.target.localRotation = Quaternion.identity;
            });
    }
  
    public void OnHoldStart(int index)
    {
        if (index < 0 || index >= isHolding.Length) return;
        isHolding[index] = true;
    }

    public void OnHoldEnd(int index)
    {
        if (index < 0 || index >= isHolding.Length) return;
        isHolding[index] = false;
    }
}
