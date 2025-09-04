using UnityEngine;

public class EasyAIStrategy : IAIStrategy
{
    private System.Random random = new System.Random();

    public Vector3? GetMove(int[,,] board, TicTacToeNxN game, int n, int self, int opponent)
    {
        var emptyCells = new System.Collections.Generic.List<Vector3>();

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                for (int z = 0; z < n; z++)
                {
                    if (board[x, y, z] == 0)
                        emptyCells.Add(new Vector3(x, y, z));
                }
            }
        }

        if (emptyCells.Count == 0) return null;
        return emptyCells[random.Next(emptyCells.Count)];
    }
}
