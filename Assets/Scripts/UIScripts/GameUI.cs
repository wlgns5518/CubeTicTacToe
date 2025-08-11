using System.Collections;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private Coroutine turnTimerCoroutine; // 턴 타이머를 관리하는 코루틴
    [SerializeField] private TextMeshProUGUI timerText; // 에디터에서 할당
    public GameOverUI gameButton;
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
            gameButton.resultPopup.SetActive(true);
            gameButton.resultButton.gameObject.SetActive(true);
            gameButton.SetResultMessage(!GameManager.Instance.tictactoe.isOTurn); // 승리 여부 전달
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
        // 팝업창 활성화 및 결과 메시지 설정
        if (gameButton != null)
        {
            gameButton.resultPopup.SetActive(true);
            gameButton.resultButton.gameObject.SetActive(true);
            gameButton.SetResultMessage(GameManager.Instance.tictactoe.isOTurn); // 승리 여부 전달
        }
    }
}
