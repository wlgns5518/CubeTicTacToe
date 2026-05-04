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
        // version이 0이면 3x3, 1이면 4x4
        GameManager.Instance.SetVersion(version);
    }

    public void GameStartButton(int mode)
    {
        SoundManager.Instance.PlayUIClickSound();
        // mode가 0이면 싱글, 1이면 멀티
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

        if (LoginManager.user == null)
        {
            Debug.LogError("[Logout] 로그인된 유저가 없습니다.");
            Application.Quit();
            return;
        }

        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.Save();

        if (LoginManager.user.IsAnonymous)
        {
            Debug.Log("[Logout] 익명 유저 로그아웃");
            LoginManager.Instance.OnDisconnect();
        }
        else if (LoginManager.user.ProviderId == "google.com")
        {
            Debug.Log("[Logout] Google 유저 로그아웃");
            LoginManager.Instance.SignOutFromGoogle();
        }
        else
        {
            Debug.LogWarning($"[Logout] 알 수 없는 로그인 제공자: {LoginManager.user.ProviderId}");
            LoginManager.Instance.OnDisconnect();
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