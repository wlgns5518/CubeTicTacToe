using Photon.Pun;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class CameraMoveAndroid : MonoBehaviourPun
{
    public float rotationSpeed = 25f;
    public float zoomSpeed = 25f;
    public float minDistance = 5f; // 확대의 최대값
    public float maxDistance = 20f; // 축소의 최대값
    public bool cameraMoving = true;

    private float distance = 10f;
    private Vector3 target;
    private float scrollValue = 0;

    private Vector2 touch0Pos;
    private Vector2 touch1Pos;
    private void Start()
    {
        target = GameManager.Instance.tictactoe.centerPosition;
        distance = Vector3.Distance(transform.position, target);
        this.transform.position = new Vector3(target.x, target.y, -10);
    }
    private void Update()
    {
        if (Touchscreen.current != null)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];
            if (touch0.isInProgress && !touch1.isInProgress)
            {
                cameraMoving = true;
                Vector2 delta = touch0.delta.ReadValue(); // 단일 터치의 움직임 값 읽기
                float yaw = delta.x * rotationSpeed * Time.deltaTime;
                float pitch = -delta.y * rotationSpeed * Time.deltaTime;
                transform.RotateAround(target, Vector3.up, yaw);
                transform.RotateAround(target, transform.right, pitch);
                distance = Vector3.Distance(transform.position, target);
            }
            else if (touch0.isInProgress && touch1.isInProgress)
            {
                cameraMoving = true;
                touch0Pos = touch0.position.ReadValue();
                touch1Pos = touch1.position.ReadValue();
                float currentDistance = Vector2.Distance(touch0Pos, touch1Pos);
                if (scrollValue == 0) // 이전 프레임의 거리를 저장
                {
                    scrollValue = currentDistance;
                }
                float deltaDistance = currentDistance - scrollValue;
                scrollValue = currentDistance;

                distance = Mathf.Clamp(distance - deltaDistance * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
                transform.position = target - transform.forward * distance;
            }
            else
            {
                scrollValue = 0; // 터치가 없을 경우 초기화
                cameraMoving = false;
            }
        }
    }

}