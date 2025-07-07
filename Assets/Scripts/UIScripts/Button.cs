using UnityEngine;

public class Button : MonoBehaviour
{
    public GameObject playPanel;
    public GameObject gamePanel;
    public GameObject setUpPanel;
    public void PlayButton(int version)
    {
        playPanel.SetActive(false);
        gamePanel.SetActive(true);
    }
    public void GameStartButton(int mode)
    {
        //mode에 따라 싱글, 멀티로 구분
        //씬을 넘어가며 게임실행
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
