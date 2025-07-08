using UnityEngine;

public class ComputerPlayer3x3 : MonoBehaviour
{
    private TicTacToe3x3 tictactoe3x3;

    private void Start()
    {
        tictactoe3x3 = GameManager.Instance.tictactoe3x3;

        if (tictactoe3x3 != null)
        {
            tictactoe3x3.OnAITurnStarted += HandleAITurnStarted; // 이벤트 구독
        }
    }

    private void OnDestroy()
    {
        if (tictactoe3x3 != null)
        {
            tictactoe3x3.OnAITurnStarted -= HandleAITurnStarted; // 이벤트 구독 해제
        }
    }

    private void HandleAITurnStarted()
    {
        MakeMove();
    }

    public void MakeMove()
    {
        int[,,] board = tictactoe3x3.board;
        int opponent = 1; // 상대방 (O)
        int self = 2; // AI (X)

        Vector3? blockMove = FindCriticalMove(board, opponent);
        if (blockMove.HasValue)
        {
            ExecuteMove(blockMove.Value, self);
            return;
        }

        Vector3? winMove = FindCriticalMove(board, self);
        if (winMove.HasValue)
        {
            ExecuteMove(winMove.Value, self);
            return;
        }

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
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
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (board[x, y, z] == 0)
                    {
                        board[x, y, z] = player;
                        if (tictactoe3x3.CheckCompletedLines(player) > 0)
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

        GameObject cube = tictactoe3x3.cubes[x, y, z];
        tictactoe3x3.OnCubeClicked(x, y, z, cube);
    }
}