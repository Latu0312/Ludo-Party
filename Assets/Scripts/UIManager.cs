using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class UIManager : MonoBehaviour
{
    public TMP_InputField dk_email;
    public TMP_InputField dk_password;
    public TMP_InputField dn_email;
    public TMP_InputField dn_password;
    public TMP_InputField rs_email;



    public void Button_Dangky()
    {
        FirebaseManager.Instance.CreateUser(dk_email.text, dk_password.text);
    }
    public void Button_Dangnhap()
    {
        FirebaseManager.Instance.SignIn(dn_email.text, dn_password.text);
    }
    public void Button_KiemtraDangnhap()
    {
        FirebaseManager.Instance.DetectAcc();
    }
    public void Button_Dangxuat()
    {
        FirebaseManager.Instance.SignOut();
    }
    public void Button_PassReset()
    {
        FirebaseManager.Instance.PassReset(rs_email.text);
    }

}