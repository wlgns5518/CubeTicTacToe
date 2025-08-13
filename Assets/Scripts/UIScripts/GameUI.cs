using System.Collections;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private Coroutine turnTimerCoroutine; // 턴 타이머를 관리하는 코루틴
    [SerializeField] private TextMeshProUGUI timerText; // 에디터에서 할당
    public GameOverUI gameButton;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI playerScoreText;

    public void StartTurnTimer()
    {
        if (turnTimerCoroutine != null)
        {
            StopCoroutine(turnTimerCoroutine); // 이전 코루틴 중지
        }
        turnTimerCoroutine = StartCoroutine(TurnTimer());
    }

    private IEnumerator TurnTimer()
    {
        float turnTimeLimit = 10f; // 턴 제한 시간
        float elapsedTime = 0f;

        while (elapsedTime < turnTimeLimit)
        {
            if (GameManager.Instance.tictactoe.gameOver) // 게임 종료 시 코루틴 중지
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            UpdateTimerText(turnTimeLimit - elapsedTime); // 남은 시간 업데이트
            yield return null; // 다음 프레임까지 대기
        }

        // 시간이 초과되면 게임 종료
        GameManager.Instance.tictactoe.gameOver = true;

        // 팝업창 활성화 및 결과 메시지 설정
        if (gameButton != null)
        {
            // isOTurn의 반대 값으로 결과 처리
            GameManager.Instance.tictactoe.isOTurn = !GameManager.Instance.tictactoe.isOTurn;
            GameResult();
        }
    }

    private void UpdateTimerText(float remainingTime)
    {
        if (timerText != null)
        {
            timerText.text = $"Time : {Mathf.Ceil(remainingTime)}s";
        }
    }
    public void GameResult()
    {
        if(GameManager.Instance.IsAIMode())
        {
            UpdatePlayerScore(GameManager.Instance.tictactoe.isOTurn);
        }
        else
        {
            if (gameButton != null)
            {
                // 플레이어가 "O"이고 현재 턴이 "O"라면 승리로 간주
                if (GameManager.Instance.PlayerRole == "O" && GameManager.Instance.tictactoe.isOTurn)
                {
                    UpdatePlayerScore(true);
                }
                // 플레이어가 "X"이고 현재 턴이 "X"라면 승리로 간주
                else if (GameManager.Instance.PlayerRole == "X" && !GameManager.Instance.tictactoe.isOTurn)
                {
                    UpdatePlayerScore(true);
                }
                else
                {
                    UpdatePlayerScore(false);
                }
                
            }
        }
        gameButton.resultPopup.SetActive(true);
        gameButton.resultButton.gameObject.SetActive(true);
    }
    public void UpdatePlayerScore(bool win)
    {
        if (win)
        {
            gameButton.SetResultMessage(win); // 승리
            int[] Scores = GameManager.Instance.UpdatePlayerScore(win);
            playerScoreText.text = Scores[0].ToString() + $"( + {Scores[1].ToString()})";
        }
        else
        {
            gameButton.SetResultMessage(win); // 패배
            int[] Scores = GameManager.Instance.UpdatePlayerScore(win);
            playerScoreText.text = Scores[0].ToString() + $"( - {Scores[1].ToString()})";
        }
    }
    public void UpdateInfoText(string message)
    {
        if (infoText != null)
        {
            infoText.text = message;
            infoText.gameObject.SetActive(true);
            StartCoroutine(HideInfoText(1f)); // 1초 후 텍스트 숨기기
        }
    }

    private IEnumerator HideInfoText(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (infoText != null)
        {
            infoText.gameObject.SetActive(false);
        }
    }
}
