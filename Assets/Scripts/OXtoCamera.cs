using UnityEngine;

public class OXtoCamera : MonoBehaviour
{
    private Camera playerCamera;
    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (playerCamera != null)
        {
            transform.LookAt(playerCamera.transform);
            // y축 기준 90도 회전 보정
            transform.Rotate(0, 90, 0);
        }
    }
}