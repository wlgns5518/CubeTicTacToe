using UnityEngine;
using UnityEngine.InputSystem;

public class CubeClickHandler : MonoBehaviour
{
    private int x, y, z;

    public void Init(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    void OnMouseDown()
    {
        //컴퓨터용
        //GameManager.Instance.tictactoe.OnCubeClicked(x, y, z, gameObject);
        //모바일용
        if (Touchscreen.current != null)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];
            if (touch0.isInProgress && !touch1.isInProgress)
            {
                GameManager.Instance.tictactoe.OnCubeClicked(x, y, z, gameObject);
            }
        }
    }
}