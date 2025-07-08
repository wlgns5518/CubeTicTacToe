using UnityEngine;

public class CubeClickHandler : MonoBehaviour
{
    private MonoBehaviour manager; // 일반적인 MonoBehaviour로 변경
    private int x, y, z;

    public void Init(MonoBehaviour manager, int x, int y, int z)
    {
        this.manager = manager;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    void OnMouseDown()
    {
        if (manager is TicTacToeNxN ticTacToeManager)
        {
            ticTacToeManager.OnCubeClicked(x, y, z, gameObject);
        }
        else
        {
            Debug.LogError("Manager type not supported.");
        }
    }
}