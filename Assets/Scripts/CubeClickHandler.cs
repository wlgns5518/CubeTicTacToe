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

    void Update()
    {
        // 마우스 클릭 또는 터치 시작 감지
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryClick(Mouse.current.position.ReadValue());
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    void TryClick(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 자기 자신이 맞았을 때만 실행
            if (hit.collider.gameObject == gameObject)
            {
                GameManager.Instance.tictactoe.OnCubeClicked(x, y, z, gameObject);
            }
        }
    }
}