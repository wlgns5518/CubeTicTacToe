using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public TicTacToe3x3 tictactoe3x3 { get; private set; }
    public TicTacToe4x4 tictactoe4x4 { get; private set; }

    void Awake()
    {
        // ΩÃ±€≈Ê √ ±‚»≠
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        tictactoe3x3 = this.GetComponent<TicTacToe3x3>();
        tictactoe4x4 = this.GetComponent<TicTacToe4x4>();
        tictactoe3x3.enabled = false;
        tictactoe4x4.enabled = false;
    }
}
