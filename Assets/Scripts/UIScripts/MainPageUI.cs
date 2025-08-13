using System.Linq;
using TMPro;
using UnityEngine;

public class MainPageUI : MonoBehaviour
{
    public GameObject playPanel;
    public GameObject gamePanel;
    public GameObject setUpPanel;
    public GameObject rankingPanel;
    public GameObject machingPanel;
    public TextMeshProUGUI machingText;
    public void PlayButton(int version)
    {
        SoundManager.Instance.PlayUIClickSound();
        playPanel.SetActive(false);
        gamePanel.SetActive(true);
        //version 이 0이면 3x3 1이면 4x4
        GameManager.Instance.SetVersion(version);
    }
    public void GameStartButton(int mode)
    {
        SoundManager.Instance.PlayUIClickSound();
        //mode 가 0이면 싱글 1이면 멀티
        //씬을 넘어가며 게임실행 
        GameManager.Instance.GameStart(mode);
        
    }
    public void BackButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        gamePanel.SetActive(false);
        playPanel.SetActive(true);
    }
    public void ExitButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        LoginManager.Instance.OnDisconnect();
        Application.Quit();
    }
    public void SetUpButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        setUpPanel.SetActive(true);
    }
    public void SetUpCloseButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        setUpPanel.SetActive(false);
    }
    public void LogoutButton()
    {
        SoundManager.Instance.PlayUIClickSound();

        if (LoginManager.user != null)
        {
            // 로그인 제공자 확인
            if (LoginManager.user.IsAnonymous) // 익명 로그인 여부 확인
            {
                Debug.Log("Logging out from Anonymous Login.");
                Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut(); // 익명 로그아웃
            }
            else if (LoginManager.user.ProviderId == "google.com") // Google 로그인 여부 확인
            {
                Debug.Log("Logging out from Google Login.");
                LoginManager.Instance.SignOutFromGoogle();
                LoginManager.Instance.OnDisconnect();
            }
            else
            {
                Debug.LogWarning("Unknown login provider.");
            }
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
        }

        Application.Quit();
    }
    public void RankingButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        rankingPanel.SetActive(true);
    }
    public void RankingCloseButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        rankingPanel.SetActive(false);
    }
    public void MachingButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        machingPanel.SetActive(true);
    }
    public void MachingCloseButton()
    {
        SoundManager.Instance.PlayUIClickSound();
        machingPanel.SetActive(false);
    }
}
