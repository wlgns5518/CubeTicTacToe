using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float rotationSpeed = 25f;
    public float zoomSpeed = 1000f;
    public float minDistance = 1f;
    public float maxDistance = 100f;

    private Vector3 lastMousePosition;
    private float distance = 10f;
    private Vector3 target;

    private void Start()
    {
        target = transform.position + transform.forward * distance;
        distance = Vector3.Distance(transform.position, target);
    }

    private void Update()
    {
        // 마우스 우클릭: 카메라 회전
        if (Input.GetMouseButtonDown(1))
            lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float yaw = delta.x * rotationSpeed * Time.deltaTime;
            float pitch = -delta.y * rotationSpeed * Time.deltaTime;
            transform.RotateAround(target, Vector3.up, yaw);
            transform.RotateAround(target, transform.right, pitch);
            lastMousePosition = Input.mousePosition;
            // 회전 후 카메라와 타겟 사이 거리 유지
            distance = Vector3.Distance(transform.position, target);
        }

        // Alt+좌클릭: 궤도 회전(Orbit)
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(0))
            lastMousePosition = Input.mousePosition;

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float yaw = delta.x * rotationSpeed * Time.deltaTime;
            float pitch = -delta.y * rotationSpeed * Time.deltaTime;
            transform.RotateAround(target, Vector3.up, yaw);
            transform.RotateAround(target, transform.right, pitch);
            lastMousePosition = Input.mousePosition;
            distance = Vector3.Distance(transform.position, target);
        }

        // 휠 클릭: 패닝(Pan)
        if (Input.GetMouseButtonDown(2))
            lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 right = transform.right * -delta.x * moveSpeed * Time.deltaTime * 0.1f;
            Vector3 up = transform.up * -delta.y * moveSpeed * Time.deltaTime * 0.1f;
            transform.position += right + up;
            target += right + up;
            lastMousePosition = Input.mousePosition;
        }

        // 마우스 휠: 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance = Mathf.Clamp(distance - scroll * zoomSpeed * Time.deltaTime, minDistance, maxDistance);
        }

        // 항상 target을 바라보고, distance만큼 떨어진 위치로 이동
        transform.position = target - transform.forward * distance;
    }
}