using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

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
        if (Touchscreen.current != null)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];
            if (touch0.isInProgress && !touch1.isInProgress)
            {
                GameManager.Instance.tictactoe.OnCubeClicked(x, y, z, gameObject);
            }
        }
        //if (manager is TicTacToeNxN ticTacToeManager)
        //{
        //    ticTacToeManager.OnCubeClicked(x, y, z, gameObject);
        //}
        //else
        //{
        //    Debug.LogError("Manager type not supported.");
        //}
    }
}