using System.Collections.Generic;
using UnityEngine;

public class HardAIStrategy : IAIStrategy
{
    private int maxDepth = 2;

    public Vector3? GetMove(int[,,] board, TicTacToeNxN game, int n, int self, int opponent)
    {
        // 1. 승리/방어 후보 우선
        Vector3? criticalMove = FindCriticalMove(board, game, n, self);
        if (criticalMove.HasValue) { Debug.Log("AI Difficulty: HARD (Winning Move)"); return criticalMove; }

        criticalMove = FindCriticalMove(board, game, n, opponent);
        if (criticalMove.HasValue) { Debug.Log("AI Difficulty: HARD (Blocking Move)"); return criticalMove; }

        // 2. 미니맥스 탐색
        return MinimaxRoot(board, game, n, self, opponent);
    }

    private Vector3? MinimaxRoot(int[,,] board, TicTacToeNxN game, int n, int self, int opponent)
    {
        int bestScore = int.MinValue;
        Vector3? bestMove = null;

        foreach (Vector3 move in GetCandidateMoves(board, n))
        {
            board[(int)move.x, (int)move.y, (int)move.z] = self;
            int score = Minimax(board, game, n, maxDepth - 1, false, self, opponent, int.MinValue, int.MaxValue);
            board[(int)move.x, (int)move.y, (int)move.z] = 0;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        Debug.Log("AI Difficulty: HARD (Minimax Move)");
        return bestMove;
    }

    private int Minimax(int[,,] board, TicTacToeNxN game, int n, int depth, bool isMaximizing, int self, int opponent, int alpha, int beta)
    {
        if (game.CheckCompletedLines(self) > 0) return 1000 + depth;
        if (game.CheckCompletedLines(opponent) > 0) return -1000 - depth;
        if (IsBoardFull(board, n) || depth == 0) return Evaluate(board, game, n, self, opponent);

        int bestValue = isMaximizing ? int.MinValue : int.MaxValue;

        foreach (Vector3 move in GetCandidateMoves(board, n))
        {
            board[(int)move.x, (int)move.y, (int)move.z] = isMaximizing ? self : opponent;
            int eval = Minimax(board, game, n, depth - 1, !isMaximizing, self, opponent, alpha, beta);
            board[(int)move.x, (int)move.y, (int)move.z] = 0;

            if (isMaximizing)
            {
                bestValue = Mathf.Max(bestValue, eval);
                alpha = Mathf.Max(alpha, eval);
            }
            else
            {
                bestValue = Mathf.Min(bestValue, eval);
                beta = Mathf.Min(beta, eval);
            }

            if (beta <= alpha) break; // 알파베타 가지치기
        }

        return bestValue;
    }

    private IEnumerable<Vector3> GetCandidateMoves(int[,,] board, int n)
    {
        // 후보칸: 빈 칸만
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                    if (board[x, y, z] == 0)
                        yield return new Vector3(x, y, z);
    }

    private bool IsBoardFull(int[,,] board, int n)
    {
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                    if (board[x, y, z] == 0) return false;
        return true;
    }

    private int Evaluate(int[,,] board, TicTacToeNxN game, int n, int self, int opponent)
    {
        int score = 0;
        // 단순 휴리스틱: 내 직전 완성 +10, 상대 직전 완성 -10
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                {
                    if (board[x, y, z] == 0)
                    {
                        board[x, y, z] = self;
                        if (game.CheckCompletedLines(self) > 0) score += 10;
                        board[x, y, z] = opponent;
                        if (game.CheckCompletedLines(opponent) > 0) score -= 10;
                        board[x, y, z] = 0;
                    }
                }
        return score;
    }

    private Vector3? FindCriticalMove(int[,,] board, TicTacToeNxN game, int n, int player)
    {
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                {
                    if (board[x, y, z] != 0) continue;
                    board[x, y, z] = player;
                    if (game.CheckCompletedLines(player) > 0)
                    {
                        board[x, y, z] = 0;
                        return new Vector3(x, y, z);
                    }
                    board[x, y, z] = 0;
                }
        return null;
    }
}