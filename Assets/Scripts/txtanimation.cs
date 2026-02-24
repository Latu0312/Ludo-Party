
using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections;

public class txtanimation : MonoBehaviour
{
    public TextMeshProUGUI text;
    private Sequence animSeq;
    
    public TextMeshProUGUI loadingText;
    private string baseText = "Loading please wait.";
    private int dotCount = 0;

    private void OnEnable()
    {
        StartAnim();
    }

    private void OnDisable()
    {

    }
    void Start()
    {
        if (loadingText != null)
        {
            loadingText.text = baseText; 
            StartCoroutine(AnimateText());
        }
    }

    public void StartAnim()
    {
        if (text == null) return;
        text.gameObject.SetActive(true); 

        
        if (animSeq != null) animSeq.Kill();

        animSeq = DOTween.Sequence();

        
        text.transform.localScale = Vector3.one; 
        animSeq.Join(text.transform.DOScale(0.8f, 1f).SetLoops(-1, LoopType.Yoyo));

       
        animSeq.Join(text.DOColor(Color.white, 1f).SetLoops(-1, LoopType.Yoyo));
    }
    public void StopAnim()
    {
        if (animSeq != null)
        {
            animSeq.Kill(); 
            animSeq = null;
            text.gameObject.SetActive(false); 
        }
    }
    IEnumerator AnimateText()
    {
        while (true)
        {
            dotCount = (dotCount + 1) % 4; 
            loadingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(0.8f); 
        }
    }
}
