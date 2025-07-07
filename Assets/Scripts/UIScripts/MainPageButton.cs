using UnityEngine;

public class MainPageButton : MonoBehaviour
{
    public GameObject playPanel;
    public GameObject gamePanel;
    public GameObject setUpPanel;
    private int gameVersion;
    public void PlayButton(int version)
    {
        playPanel.SetActive(false);
        gamePanel.SetActive(true);
        //version 이 0이면 3x3 1이면 4x4
        gameVersion = version;
    }
    public void GameStartButton(int mode)
    {
        //mode 가 0이면 싱글 1이면 멀티
        //씬을 넘어가며 게임실행 
        GameManager.Instance.GameStart(gameVersion, mode);
        
    }
    public void BackButton()
    {
        gamePanel.SetActive(false);
        playPanel.SetActive(true);
    }
    public void ExitButton()
    {
        Application.Quit();
    }
    public void SetUpButton()
    {
        setUpPanel.SetActive(true);
    }
    public void SetUpCloseButton()
    {
        setUpPanel.SetActive(false);
    }
}
