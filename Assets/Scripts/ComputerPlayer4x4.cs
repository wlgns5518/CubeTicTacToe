using UnityEngine;

public class ComputerPlayer4x4 : MonoBehaviour
{
    private TicTacToe4x4 tictactoe4x4;

    private void Start()
    {
        tictactoe4x4 = GameManager.Instance.tictactoe4x4;
    }

    void Update()
    {
        // AI가 활성화되어 있고, 게임이 진행 중이며 AI의 턴일 때 움직임 수행
        if (tictactoe4x4 != null && !tictactoe4x4.gameOver && !tictactoe4x4.isOTurn)
        {
            MakeMove();
        }
    }

    public void MakeMove()
    {
        int[,,] board = tictactoe4x4.board;
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
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
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
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    if (board[x, y, z] == 0)
                    {
                        board[x, y, z] = player;
                        if (tictactoe4x4.CheckCompletedLines(player) > 0)
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

        GameObject cube = tictactoe4x4.cubes[x, y, z];
        tictactoe4x4.OnCubeClicked(x, y, z, cube);
    }
}