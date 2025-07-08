using UnityEngine;

public class ComputerPlayerNxN : MonoBehaviour
{
    [SerializeField] private TicTacToeNxN tictactoe;
    private int n;
    private void Awake()
    {
        if(!GameManager.Instance.IsAIMode())
        {
            this.enabled = false;
        }
        n = GameManager.Instance.CurrentVersion;
    }
    private void Start()
    {
        if (tictactoe != null)
        {
            tictactoe.OnAITurnStarted += HandleAITurnStarted; // 이벤트 구독
            // AI가 선공일 경우 첫 번째 턴을 수행
            if (GameManager.Instance.IsAIMode() && GameManager.Instance.isOTurnFirst == false)
            {
                HandleAITurnStarted();
            }
        }
    }
    private void OnDestroy()
    {
        if (tictactoe != null)
        {
            tictactoe.OnAITurnStarted -= HandleAITurnStarted; // 이벤트 구독 해제
        }
    }
    private void HandleAITurnStarted()
    {
        MakeMove();
    }

    public void MakeMove()
    {
        int[,,] board = tictactoe.board;
        int opponent = 1; // 상대방 (O)
        int self = 2; // AI (X)

        // 1. 상대방의 승리를 막기 위한 위치 찾기
        Vector3? blockMove = FindCriticalMove(board, opponent);
        if (blockMove.HasValue)
        {
            ExecuteMove(blockMove.Value, self);
            return;
        }

        // 2. 자신의 승리를 위한 위치 찾기
        Vector3? winMove = FindCriticalMove(board, self);
        if (winMove.HasValue)
        {
            ExecuteMove(winMove.Value, self);
            return;
        }

        // 3. 임의의 빈 칸 선택
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                for (int z = 0; z < n; z++)
                {
                    if (board[x, y, z] == 0)
                    {
                        ExecuteMove(new Vector3(x, y, z), self);
                        return;
                    }
                }
            }
        }
    }

    private Vector3? FindCriticalMove(int[,,] board, int player)
    {
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                for (int z = 0; z < n; z++)
                {
                    if (board[x, y, z] == 0)
                    {
                        board[x, y, z] = player;
                        if (tictactoe.CheckCompletedLines(player) > 0)
                        {
                            board[x, y, z] = 0; // 원상복구
                            return new Vector3(x, y, z);
                        }
                        board[x, y, z] = 0; // 원상복구
                    }
                }
            }
        }
        return null;
    }

    private void ExecuteMove(Vector3 position, int player)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int z = (int)position.z;

        GameObject cube = tictactoe.cubes[x, y, z];
        tictactoe.OnCubeClicked(x, y, z, cube);
    }
}