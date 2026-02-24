using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    
    public enum AnimationType { None, Fade, Slide, Scale, FadeAndSlide }

    [System.Serializable]
    public class PanelSetting
    {
        public string name;                
        public GameObject panel;           
        public AnimationType animationType; 
        public Vector2 slideOffset = new Vector2(1000f, 0); 
        public float duration = 0.5f;      

        
        [HideInInspector] public RectTransform rt;
        [HideInInspector] public CanvasGroup cg;
        [HideInInspector] public Vector2 initialAnchoredPos;
        [HideInInspector] public Vector3 initialScale;
        [HideInInspector] public float initialAlpha = 1f;
        [HideInInspector] public bool cached;
    }

   
    public List<PanelSetting> panels = new List<PanelSetting>(); 

    private PanelSetting currentPanel;

    void Start()
    {
        
        foreach (var p in panels) CachePanel(p);

        
        for (int i = 0; i < panels.Count; i++)
        {
            SetActiveInstant(panels[i], i == 0);
        }

        if (panels.Count > 0)
            currentPanel = panels[0];
    }

   

    private void CachePanel(PanelSetting s)
    {
        if (s == null || s.panel == null) return;
        s.rt = s.panel.GetComponent<RectTransform>();
        if (s.rt == null)
        {
            Debug.LogError($"Panel '{s.name}' thiếu RectTransform.");
            return;
        }

        s.cg = s.panel.GetComponent<CanvasGroup>();
        s.initialAnchoredPos = s.rt.anchoredPosition;
        s.initialScale = s.rt.localScale;
        s.initialAlpha = s.cg ? s.cg.alpha : 1f;
        s.cached = true;
    }

    private void SetActiveInstant(PanelSetting s, bool active)
    {
        if (s == null || s.panel == null) return;

        ResetToInitialTransform(s);
        if (s.cg) s.cg.alpha = s.initialAlpha;
        s.panel.SetActive(active);
    }

    private void ResetToInitialTransform(PanelSetting s)
    {
        if (!s.cached) return;
        s.rt.anchoredPosition = s.initialAnchoredPos;
        s.rt.localScale = s.initialScale;
    }

    private void KillTweens(PanelSetting s, bool complete = false)
    {
        if (s.rt) s.rt.DOKill(complete);
        if (s.cg) s.cg.DOKill(complete);
    }

    
    public void SwitchPanelByIndex(int index)
    {
        if (index < 0 || index >= panels.Count) return;
        SwitchPanel(panels[index]);
    }

    public void SwitchPanelByName(string panelName)
    {
        var target = panels.Find(p => p.name == panelName);
        if (target != null) SwitchPanel(target);
    }

    private void SwitchPanel(PanelSetting newPanel)
    {
        if (newPanel == null || newPanel.panel == null) return;
        if (currentPanel == newPanel) return;
        
        if (currentPanel != null && currentPanel != panels[0])
        {
            HidePanel(currentPanel);
        }

        
        if (panels.Count > 0 && panels[0].panel != null)
        {
            panels[0].panel.SetActive(true);

            
            var txt = panels[0].panel.GetComponentInChildren<txtanimation>(true);
            if (newPanel == panels[0])
            {
                txt?.StartAnim(); 
            }
            else
            {
                txt?.StopAnim();       
            }
        }

       
        ShowPanel(newPanel);
        currentPanel = newPanel;
    }

    private void EnsureCanvasGroup(PanelSetting s)
    {
        if (!s.cg)
        {
            s.cg = s.panel.AddComponent<CanvasGroup>();
            s.cg.alpha = s.initialAlpha;
        }
    }

   

    private void ShowPanel(PanelSetting s)
    {
        if (s == null || s.panel == null) return;

        KillTweens(s);
        ResetToInitialTransform(s);
        s.panel.SetActive(true);

        switch (s.animationType)
        {
            case AnimationType.Fade:
                EnsureCanvasGroup(s);
                s.cg.alpha = 0f;
                s.cg.DOFade(s.initialAlpha, s.duration);
                break;

            case AnimationType.Slide:
                s.rt.anchoredPosition = s.initialAnchoredPos + s.slideOffset;
                s.rt.DOAnchorPos(s.initialAnchoredPos, s.duration).SetEase(Ease.OutBack);
                break;

            case AnimationType.Scale:
                s.rt.localScale = Vector3.zero;
                s.rt.DOScale(s.initialScale, s.duration).SetEase(Ease.OutBack);
                break;

            case AnimationType.FadeAndSlide:
                EnsureCanvasGroup(s);
                s.cg.alpha = 0f;
                var seqIn = DOTween.Sequence();
                s.rt.anchoredPosition = s.initialAnchoredPos + s.slideOffset;
                seqIn.Join(s.cg.DOFade(s.initialAlpha, s.duration));
                seqIn.Join(s.rt.DOAnchorPos(s.initialAnchoredPos, s.duration).SetEase(Ease.OutCubic));
                break;

            case AnimationType.None:
            default:
                EnsureCanvasGroup(s);
                s.cg.alpha = s.initialAlpha;
                break;
        }
    }

    private void HidePanel(PanelSetting s)
    {
        if (s == null || s.panel == null) return;

        KillTweens(s);

        switch (s.animationType)
        {
            case AnimationType.Fade:
                EnsureCanvasGroup(s);
                s.cg.DOFade(0f, s.duration).OnComplete(() =>
                {
                    s.panel.SetActive(false);
                    ResetToInitialTransform(s);
                    s.cg.alpha = s.initialAlpha;
                });
                break;

            case AnimationType.Slide:
                s.rt.DOAnchorPos(s.initialAnchoredPos + s.slideOffset, s.duration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        s.panel.SetActive(false);
                        s.rt.anchoredPosition = s.initialAnchoredPos;
                    });
                break;

            case AnimationType.Scale:
                s.rt.DOScale(Vector3.zero, s.duration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        s.panel.SetActive(false);
                        s.rt.localScale = s.initialScale;
                    });
                break;

            case AnimationType.FadeAndSlide:
                EnsureCanvasGroup(s);
                var seqOut = DOTween.Sequence();
                seqOut.Join(s.cg.DOFade(0f, s.duration));
                seqOut.Join(s.rt.DOAnchorPos(s.initialAnchoredPos + s.slideOffset, s.duration).SetEase(Ease.InCubic));
                seqOut.OnComplete(() =>
                {
                    s.panel.SetActive(false);
                    s.rt.anchoredPosition = s.initialAnchoredPos;
                    s.cg.alpha = s.initialAlpha;
                });
                break;

            case AnimationType.None:
            default:
                s.panel.SetActive(false);
                ResetToInitialTransform(s);
                if (s.cg) s.cg.alpha = s.initialAlpha;
                break;
        }
    }
}
