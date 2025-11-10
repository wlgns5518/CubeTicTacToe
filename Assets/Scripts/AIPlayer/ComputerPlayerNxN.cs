using System;
using System.Threading.Tasks;
using UnityEngine;

public class ComputerPlayerNxN : MonoBehaviour
{
    private TicTacToeNxN tictactoe;
    private int n;

    private IAIStrategy aiStrategy;

    // 하드 AI 연산 중복 방지
    private bool _isThinking;

    private void Awake()
    {
        if (!GameManager.Instance.IsAIMode())
        {
            enabled = false;
            return;
        }

        n = GameManager.Instance.CurrentVersion;

        // 점수 기반 난이도 설정
        int playerScore = GameManager.Instance.PlayerScore;

        if (playerScore >= 1200)
        {
            aiStrategy = new HardAIStrategy();
            Debug.Log("AI 난이도: Hard");
        }
        else if (playerScore >= 1000)
        {
            aiStrategy = new NormalAIStrategy();
            Debug.Log("AI 난이도: Normal");
        }
        else
        {
            aiStrategy = new EasyAIStrategy();
            Debug.Log("AI 난이도: Easy");
        }
    }
    private void Start()
    {
        tictactoe = GameManager.Instance.tictactoe;

        if (tictactoe == null) return;

        tictactoe.OnAITurnStarted += MakeMove;

        if (!GameManager.Instance.isOTurnFirst)
        {
            MakeMove();
        }
    }

    private void OnDestroy()
    {
        if (tictactoe != null)
            tictactoe.OnAITurnStarted -= MakeMove;
    }

    public async void MakeMove()
    {
        if (tictactoe == null) return;

        const int OPPONENT = 1;
        const int SELF = 2;

        // Hard일 때만 별도 스레드에서 연산
        if (aiStrategy is HardAIStrategy)
        {
            if (_isThinking) return;

            try
            {
                _isThinking = true;

                // 보드 스냅샷으로 연산(동시성 안전)
                int[,,] boardSnapshot = (int[,,])tictactoe.board.Clone();

                Vector3? move = await Task.Run(() =>
                    aiStrategy.GetMove(boardSnapshot, tictactoe, n, SELF, OPPONENT)
                );

                // 메인 스레드 복귀 후 안전 확인
                if (!move.HasValue) return;
                if (tictactoe == null || tictactoe.gameOver) return;

                int x = (int)move.Value.x;
                int y = (int)move.Value.y;
                int z = (int)move.Value.z;

                // 선택된 자리가 여전히 비어있는지 확인
                if (x < 0 || x >= n || y < 0 || y >= n || z < 0 || z >= n) return;
                if (tictactoe.board[x, y, z] != 0) return;

                ExecuteMove(move.Value);
            }
            catch (Exception ex)
            {
                Debug.LogError($"AI 계산 중 오류: {ex}");
            }
            finally
            {
                _isThinking = false;
            }
        }
        else
        {
            // 다른 난이도는 기존처럼 동기 실행(연산이 가벼움)
            int[,,] board = tictactoe.board;
            Vector3? move = aiStrategy.GetMove(board, tictactoe, n, SELF, OPPONENT);

            if (move.HasValue)
            {
                ExecuteMove(move.Value);
            }
        }
    }

    private void ExecuteMove(Vector3 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        GameObject cube = tictactoe.cubes[x, y, z];
        tictactoe.OnCubeClicked(x, y, z, cube);
    }
}