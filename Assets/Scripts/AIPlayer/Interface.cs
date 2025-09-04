using UnityEngine;

public interface IAIStrategy
{
    Vector3? GetMove(int[,,] board, TicTacToeNxN game, int n, int self, int opponent);
}
