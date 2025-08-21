using UnityEngine;

public class OXtoCamera : MonoBehaviour
{
    private Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;
    }

    private void FixedUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);
            // y축 기준 90도 회전 보정
            transform.Rotate(0, 90, 0);
        }
    }
}