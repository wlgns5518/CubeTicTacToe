using UnityEngine;
public class NormalAIStrategy : IAIStrategy
{
    public Vector3? GetMove(int[,,] board, TicTacToeNxN game, int n, int self, int opponent)
    {
        // 1. 내 승리
        Vector3? move = FindCriticalMove(board, game, n, self);
        if (move.HasValue) return move;

        // 2. 상대 방어
        move = FindCriticalMove(board, game, n, opponent);
        if (move.HasValue) return move;

        // 3. 빈칸
        return FindFirstEmptyCell(board, n);
    }

    private Vector3? FindCriticalMove(int[,,] board, TicTacToeNxN game, int n, int player)
    {
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                for (int z = 0; z < n; z++)
                {
                    if (board[x, y, z] != 0) continue;

                    board[x, y, z] = player;
                    // 변경: 실제 보드가 아닌 시뮬레이션 보드 상태로 승리 라인 검사
                    bool isWinningMove = game.HasCompletedLine(board, player);
                    board[x, y, z] = 0;

                    if (isWinningMove)
                        return new Vector3(x, y, z);
                }
            }
        }
        return null;
    }

    private Vector3? FindFirstEmptyCell(int[,,] board, int n)
    {
        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                for (int z = 0; z < n; z++)
                {
                    if (board[x, y, z] == 0)
                        return new Vector3(x, y, z);
                }
            }
        }
        return null;
    }
}