using UnityEngine;

public class MainPageButton : MonoBehaviour
{
    public GameObject playPanel;
    public GameObject gamePanel;
    public GameObject setUpPanel;
    private int gameVersion;
    public void PlayButton(int version)
    {
        SoundManager.Instance.PlayUIClickSound();
        playPanel.SetActive(false);
        gamePanel.SetActive(true);
        //version 이 0이면 3x3 1이면 4x4
        gameVersion = version;
    }
    public void GameStartButton(int mode)
    {
        SoundManager.Instance.PlayUIClickSound();
        //mode 가 0이면 싱글 1이면 멀티
        //씬을 넘어가며 게임실행 
        GameManager.Instance.GameStart(gameVersion, mode);
        
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
}
