using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraMoveAndroid : MonoBehaviourPun
{
    public bool cameraMoving { get; private set; } = false;

    [Header("Camera Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 20f;

    private float distance;
    private Vector3 target;

    private float previousPinchDistance = 0f;

    private void Start()
    {
        target = GameManager.Instance.tictactoe.centerPosition;
        distance = Vector3.Distance(transform.position, target);
        transform.position = target - transform.forward * distance;
    }

    private void Update()
    {
        if (Touchscreen.current == null) return;

        var touches = Touchscreen.current.touches;
        var touch0 = touches[0];
        var touch1 = touches[1];

        if (touch0.isInProgress && !touch1.isInProgress)
        {
            HandleSingleTouch(touch0);
        }
        else if (touch0.isInProgress && touch1.isInProgress)
        {
            HandlePinchZoom(touch0, touch1);
        }
        else
        {
            previousPinchDistance = 0f;
            cameraMoving = false;
        }
    }

    private void HandleSingleTouch(TouchControl touch)
    {
        cameraMoving = true;
        Vector2 delta = touch.delta.ReadValue();
        float yaw = delta.x * rotationSpeed * Time.deltaTime;
        float pitch = -delta.y * rotationSpeed * Time.deltaTime;

        transform.RotateAround(target, Vector3.up, yaw);
        transform.RotateAround(target, transform.right, pitch);

        // 거리 갱신
        distance = Vector3.Distance(transform.position, target);
    }

    private void HandlePinchZoom(TouchControl touch0, TouchControl touch1)
    {
        cameraMoving = true;

        Vector2 pos0 = touch0.position.ReadValue();
        Vector2 pos1 = touch1.position.ReadValue();
        float currentDistance = Vector2.Distance(pos0, pos1);

        if (previousPinchDistance == 0f)
        {
            previousPinchDistance = currentDistance;
            return;
        }

        float deltaDistance = currentDistance - previousPinchDistance;
        previousPinchDistance = currentDistance;

        distance = Mathf.Clamp(distance - deltaDistance * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
        transform.position = target - transform.forward * distance;
    }
}
