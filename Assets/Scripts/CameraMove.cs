using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviourPun
{
    public float moveSpeed = 50f;
    public float rotationSpeed = 25f;
    public float zoomSpeed = 200f;
    public float minDistance = 5f; // 확대의 최대값
    public float maxDistance = 20f; // 축소의 최대값

    private float distance = 10f;
    private Vector3 target;
    private float scrollValue = 0;

    // Input Actions
    public InputAction rotateAction;
    public InputAction zoomAction;

    public event System.Action<float> OnZoomEvent;
    private void OnEnable()
    {
        // Enable Input Actions
        rotateAction.Enable();
        zoomAction.Enable();
        // Subscribe to zoom event
        zoomAction.performed += OnZoomPerformed;
        zoomAction.canceled += OnZoomCanceled;
    }
    private void Start()
    {
        target = GameManager.Instance.tictactoe.centerPosition;
        distance = Vector3.Distance(transform.position, target);
        this.transform.position = new Vector3(target.x, target.y, -10);        
    }

    private void Update()
    {
        // 마우스 버튼이 눌렸을 때만 회전 처리
        if (rotateAction.IsPressed())
        {
            Vector2 delta = Mouse.current.delta.ReadValue(); // 마우스 움직임 값 읽기
            float yaw = delta.x * rotationSpeed * Time.deltaTime;
            float pitch = -delta.y * rotationSpeed * Time.deltaTime;
            transform.RotateAround(target, Vector3.up, yaw);
            transform.RotateAround(target, transform.right, pitch);
            distance = Vector3.Distance(transform.position, target);
        }
        // Zoom camera
        if (scrollValue != 0)
        {
            distance = Mathf.Clamp(distance - scrollValue * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
            transform.position = target - transform.forward * distance;
        }
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        Vector2 scrollDelta = context.ReadValue<Vector2>();
        scrollValue = scrollDelta.y; // Use y-axis scroll
        OnZoomEvent?.Invoke(scrollValue); // Trigger event
    }

    private void OnZoomCanceled(InputAction.CallbackContext context)
    {
        scrollValue = 0;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        zoomAction.performed -= OnZoomPerformed;
        zoomAction.canceled -= OnZoomCanceled;

        // Disable Input Actions
        rotateAction.Disable();
        zoomAction.Disable();
    }
}