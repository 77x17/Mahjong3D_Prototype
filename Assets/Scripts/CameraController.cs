using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance = 10.0f;

    [Header("Cấu hình Xoay")]
    public float xSpeed = 5.0f;
    public float ySpeed = 5.0f;
    private const float Y_MIN = 30f; // Giới hạn góc nhìn từ dưới lên
    private const float Y_MAX = 60f; // Giới hạn góc nhìn từ trên xuống

    [Header("Cấu hình Zoom")]
    public float scrollSpeed = 10.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 30.0f;
    public bool canZoom = true;

    private float x = 0.0f;
    private float y = 45.0f; // Bắt đầu ở giữa khoảng 30-60
    private float currentX, currentY;
    private float smoothTime = 5.0f;

    private Camera cam;
    private bool canRotateCamera;

    void Start()
    {
        cam = GetComponentInChildren<Camera>() ?? Camera.main;
        if (target == null) target = new GameObject("CamTarget").transform;

        Vector3 angles = transform.eulerAngles;
        currentX = x = angles.y;
        currentY = y = Mathf.Clamp(angles.x, Y_MIN, Y_MAX);
    }

    void LateUpdate()
    {
        if (!target) return;

        HandleInput();

        // Nội suy để mượt mà
        currentX = Mathf.Lerp(currentX, x, Time.deltaTime * smoothTime);
        currentY = Mathf.Lerp(currentY, y, Time.deltaTime * smoothTime);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = target.position + (rotation * new Vector3(0, 0, -distance));
        transform.LookAt(target);
    }

    private void HandleInput()
    {
        // 1. Kiểm tra bắt đầu chạm
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current?.IsPointerOverGameObject() ?? false)
            {
                canRotateCamera = false;
                return;
            }

            // Kiểm tra Raycast xem có trúng Tile không
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            canRotateCamera = !Physics.Raycast(ray, out RaycastHit hit) || 
                               hit.collider.GetComponentInParent<TileInteraction>() == null;
        }

        // 2. Xử lý xoay
        if (Input.GetMouseButton(0) && canRotateCamera && Input.touchCount < 2)
        {
            float dx, dy;
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                float dpiMult = 60f / (Screen.dpi > 0 ? Screen.dpi : 100);
                dx = t.deltaPosition.x * xSpeed * dpiMult * 0.5f;
                dy = t.deltaPosition.y * ySpeed * dpiMult * 0.5f;
            }
            else
            {
                dx = Input.GetAxis("Mouse X") * xSpeed;
                dy = Input.GetAxis("Mouse Y") * ySpeed;
            }

            x += dx;
            y = Mathf.Clamp(y - dy, Y_MIN, Y_MAX);
        }

        // 3. Xử lý Zoom (Pinch & Scroll)
        if (canZoom)
        {
            float zoomDelta = 0;
            if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0), t1 = Input.GetTouch(1);
                float prevMag = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
                float currentMag = (t0.position - t1.position).magnitude;
                zoomDelta = (currentMag - prevMag) * scrollSpeed * 0.01f;
            }
            else
            {
                zoomDelta = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            }

            if (Mathf.Abs(zoomDelta) > 0.001f)
                distance = Mathf.Clamp(distance - zoomDelta, minDistance, maxDistance);
        }
    }

    public void FitLevel(float levelRadius)
    {
        float vFOVRad = cam.fieldOfView * Mathf.Deg2Rad;
        float hFOVRad = 2f * Mathf.Atan(Mathf.Tan(vFOVRad / 2f) * cam.aspect);
        
        distance = Mathf.Max(levelRadius / Mathf.Sin(hFOVRad / 2f), levelRadius / Mathf.Sin(vFOVRad / 2f)) * 1.05f;
        minDistance = maxDistance = distance;
        canZoom = false;
    }
}