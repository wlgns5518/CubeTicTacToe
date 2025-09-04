using UnityEngine;

public class ComputerPlayerNxN : MonoBehaviour
{
    private TicTacToeNxN tictactoe;
    private int n;

    private IAIStrategy aiStrategy;

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

    public void MakeMove()
    {
        if (tictactoe == null) return;

        int[,,] board = tictactoe.board;
        const int OPPONENT = 1;
        const int SELF = 2;

        Vector3? move = aiStrategy.GetMove(board, tictactoe, n, SELF, OPPONENT);

        if (move.HasValue)
        {
            ExecuteMove(move.Value);
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
