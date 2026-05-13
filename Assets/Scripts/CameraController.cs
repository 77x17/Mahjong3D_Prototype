using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;          
    public float distance = 10.0f;    
    
    [Header("Cấu hình Xoay")]
    public float xSpeed = 5.0f;
    public float ySpeed = 5.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("Cấu hình Zoom (Cuộn chuột)")]
    public float scrollSpeed = 10.0f;  // Tốc độ zoom
    public float minDistance = 2.0f;   // Khoảng cách gần nhất
    public float maxDistance = 30.0f;  // Khoảng cách xa nhất
    public bool canZoom = true; // Thêm biến để khóa zoom

    private float x = 0.0f;
    private float y = 20.0f;

    private float smoothTime = 5.0f; // Số càng cao càng mượt nhưng sẽ có độ trễ
    private float currentX = 0.0f;
    private float currentY = 0.0f;

    private Camera cam;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();

        if (target == null) {
            target = new GameObject("DefaultTarget").transform;
        }

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target)
        {
            // if (Input.touchCount == 2)
            // {
            //     Touch touch0 = Input.GetTouch(0);
                
            //     // CẢI TIẾN 1: Dựa trên DPI để tốc độ ổn định trên mọi điện thoại
            //     // Chia cho Screen.dpi giúp giá trị không bị quá lớn trên màn hình độ phân giải cao
            //     float dpiMultiplier = 60f / (Screen.dpi > 0 ? Screen.dpi : 100);

            //     if (touch0.phase == TouchPhase.Moved)
            //     {
            //         // Giảm hệ số xuống thêm nếu vẫn thấy nhanh (ở đây dùng 0.1f)
            //         x += touch0.deltaPosition.x * xSpeed * dpiMultiplier * 0.5f;
            //         y -= touch0.deltaPosition.y * ySpeed * dpiMultiplier * 0.5f;
            //         y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
            //     }

            //     // Xử lý Zoom (Pinch)
            //     Touch touch1 = Input.GetTouch(1);
            //     Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            //     Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            //     float prevMag = (prevPos0 - prevPos1).magnitude;
            //     float currentMag = (touch0.position - touch1.position).magnitude;
            //     float diff = currentMag - prevMag;

            //     // Zoom cũng cần dựa trên DPI để không bị quá nhạy
            //     distance -= diff * scrollSpeed * dpiMultiplier * 0.1f;
            //     distance = Mathf.Clamp(distance, minDistance, maxDistance);
            // }
            // else if (Input.GetMouseButton(1) && Input.touchCount < 2)
            // {
            //     x += Input.GetAxis("Mouse X") * xSpeed;
            //     y -= Input.GetAxis("Mouse Y") * ySpeed;
            //     y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
            // }

            // Zoom bằng cuộn chuột (PC)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                distance -= scroll * scrollSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            // CẢI TIẾN 2: Nội suy (Lerp) để tạo cảm giác trôi mượt mà
            currentX = Mathf.Lerp(currentX, x, Time.deltaTime * smoothTime);
            currentY = Mathf.Lerp(currentY, y, Time.deltaTime * smoothTime);

            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 position = target.position + (rotation * new Vector3(0, 0, -distance));

            transform.position = position;
            transform.LookAt(target);
        }
    }

    public void FitLevel(float levelRadius)
    {
        if (cam == null) cam = GetComponentInChildren<Camera>();

        // 1. Lấy tỉ lệ màn hình hiện tại (Ví dụ: 16:9 là 1.77, 9:16 là 0.56)
        float aspect = cam.aspect;

        // 2. Lấy Vertical FOV hiện tại của camera (tính bằng radian)
        float vFOVRad = cam.fieldOfView * Mathf.Deg2Rad;

        // 3. Tính Horizontal FOV (Góc nhìn ngang) dựa trên Aspect Ratio
        // Công thức: HFOV = 2 * atan(tan(VFOV / 2) * aspect)
        float hFOVRad = 2f * Mathf.Atan(Mathf.Tan(vFOVRad / 2f) * aspect);

        // 4. Tính toán khoảng cách cần thiết để "vừa khít" chiều ngang
        // Chúng ta coi toàn bộ màn chơi là một khối cầu bán kính levelRadius
        // Khoảng cách d = radius / sin(HFOV / 2)
        float distanceForWidth = levelRadius / Mathf.Sin(hFOVRad / 2f);

        // 5. Tính thêm khoảng cách dự phòng cho chiều dọc (đề phòng màn hình quá dài)
        float distanceForHeight = levelRadius / Mathf.Sin(vFOVRad / 2f);

        // 6. Chọn khoảng cách lớn nhất để đảm bảo không chiều nào bị che
        distance = Mathf.Max(distanceForWidth, distanceForHeight);

        // 7. Thêm một chút Padding (khoảng trống lề) để nhìn đẹp hơn (ví dụ 5%)
        distance *= 1.05f;

        // Khóa Zoom
        minDistance = distance;
        maxDistance = distance;
        canZoom = false;

        Debug.Log($"Screen Aspect: {aspect} | Dist: {distance}");
    }
}