using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    public GameObject emailLoginPanel;
    public GameObject LoginPanel;
    public TMP_InputField emailInputField; // 이메일 입력 필드
    public TMP_InputField passwordInputField; // 비밀번호 입력 필드
    
    public void EmailLoginButton()
    {
        emailLoginPanel.SetActive(true);
    }
    //public void SignInButton()
    //{
    //    string email = emailInputField.text; // 입력된 이메일
    //    string password = passwordInputField.text; // 입력된 비밀번호
    //    LoginManager.Instance.RegisterWithEmail(email, password);
    //}
    //public void LoginButton()
    //{
    //    string email = emailInputField.text; // 입력된 이메일
    //    string password = passwordInputField.text; // 입력된 비밀번호
    //    LoginManager.Instance.SignInWithEmail(email, password);
    //}
    public void CloseButton()
    {
        emailLoginPanel.SetActive(false);
    }
    public void Login()
    {
        LoginPanel.SetActive(true);
    }
}
