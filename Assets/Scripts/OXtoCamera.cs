using UnityEngine;

public class OXtoCamera : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            // y축 기준 90도 회전 보정
            transform.Rotate(0, 90, 0);
        }
    }
}