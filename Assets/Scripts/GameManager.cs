using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public TicTacToe3x3 tictactoe3x3 { get; private set; }
    public TicTacToe4x4 tictactoe4x4 { get; private set; }

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        tictactoe3x3 = this.GetComponent<TicTacToe3x3>();
        tictactoe4x4 = this.GetComponent<TicTacToe4x4>();
        tictactoe3x3.enabled = false;
        tictactoe4x4.enabled = false;
    }
    public void GameStart(int version, int mode)
    {
        if (version == 0)
        {
            // 3x3 게임을 위한 씬 번호 1 실행
            SceneManager.LoadScene(1);
            tictactoe3x3.enabled = true;
            if (mode == 0)
            {
                // ComputerPlayer3x3 바로 실행
                ComputerPlayer3x3 computerPlayer = FindAnyObjectByType<ComputerPlayer3x3>();
                if (computerPlayer != null)
                {
                    computerPlayer.enabled = true; // ComputerPlayer4x4 활성화
                }
            }
        }
        else if (version == 1)
        {
            // 4x4 게임을 위한 씬 번호 2 실행
            SceneManager.LoadScene(2);
            tictactoe4x4.enabled = true;
            if (mode == 0)
            {
                // ComputerPlayer4x4 바로 실행
                ComputerPlayer4x4 computerPlayer = FindAnyObjectByType<ComputerPlayer4x4>();
                if (computerPlayer != null)
                {
                    computerPlayer.enabled = true; // ComputerPlayer4x4 활성화
                }
            }
        }
    }
}
