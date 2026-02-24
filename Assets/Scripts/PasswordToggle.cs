using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PasswordToggle : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField; 
    [SerializeField] private Toggle passwordToggle; 
    [SerializeField] private Text toggleLabel; 

    void Start()
    {
       
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
        if (toggleLabel != null) toggleLabel.text = "Show Password";
        passwordToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
            if (toggleLabel != null) toggleLabel.text = "Hide PassWord";
        }
        else
        {
           
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            if (toggleLabel != null) toggleLabel.text = "Show Password ";
        }
        
        passwordInputField.ForceLabelUpdate();
    }

   
    public string GetPassword()
    {
        return passwordInputField.text;
    }
}