using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class DotweenUI : MonoBehaviour
{
  
    public float moveDistance = 1000f;    
    public float duration = 0.5f;          
    public Ease easeType = Ease.InBack;    
    private Vector3 originalPosition;
    private bool isAnimating = false;
    public void Start()
    {
        
        originalPosition = transform.localPosition;

    }

    public void ClosePanel()
    {
        if (isAnimating) return;
        isAnimating = true;

        transform.DOLocalMoveY(originalPosition.y + moveDistance, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                transform.localPosition = originalPosition; 
                isAnimating = false;
            });
    }

    public void OpenPanel()  
    {
        if (isAnimating) return;
        isAnimating = true;

     
        transform.localPosition = originalPosition + Vector3.up * moveDistance;
        gameObject.SetActive(true);

        transform.DOLocalMoveY(originalPosition.y, duration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                isAnimating = false;
            });
    }


}
