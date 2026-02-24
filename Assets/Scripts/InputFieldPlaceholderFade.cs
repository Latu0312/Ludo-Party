using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class InputFieldPlaceholderFade : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private TMP_InputField tmpInputField;
    private TMP_Text placeholderText;

   
    public float fadeDuration = 0.25f;

    void Start()
    {
        tmpInputField = GetComponent<TMP_InputField>();

        if (tmpInputField == null)
        {
            Debug.LogError("Không tìm thấy TMP_InputField trên GameObject.");
            return;
        }

        if (tmpInputField.placeholder == null)
        {
            Debug.LogError("TMP_InputField chưa có Placeholder.");
            return;
        }

        placeholderText = tmpInputField.placeholder as TMP_Text;

        if (placeholderText == null)
        {
            Debug.LogError("Placeholder không phải TMP_Text. Kiểm tra lại thành phần Placeholder.");
            return;
        }

       
        Color c = placeholderText.color;
        c.a = 0.5f;
        placeholderText.color = c;

    }

    public void OnSelect(BaseEventData eventData)
    {
        if (placeholderText != null)
        {
            placeholderText.DOFade(0f, fadeDuration);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (placeholderText != null && string.IsNullOrEmpty(tmpInputField.text))
        {
            placeholderText.DOFade(0.5f, fadeDuration);
        }
    }
}
