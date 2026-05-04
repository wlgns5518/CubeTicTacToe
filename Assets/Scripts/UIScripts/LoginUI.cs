using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    public GameObject emailLoginPanel;
    public GameObject loginPanel;
    public GameObject loadingPanel;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;

    private void Start()
    {
        if (PlayerPrefs.HasKey("UserId"))
            Loading();
    }

    public void EmailLoginButton()
    {
        emailLoginPanel.SetActive(true);
    }

    public void CloseButton()
    {
        emailLoginPanel.SetActive(false);
    }

    public void Login()
    {
        loadingPanel.SetActive(false); // 로딩 패널을 숨기고
        loginPanel.SetActive(true);    // 로그인 패널을 표시
    }

    public void Loading()
    {
        loadingPanel.SetActive(true);
    }
}