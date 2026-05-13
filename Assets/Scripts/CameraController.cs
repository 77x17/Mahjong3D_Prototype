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

    private float x = 0.0f;
    private float y = 20.0f;

    void Start()
    {
        if (target == null) {
            GameObject sun = GameObject.Find("Sun");
            if (sun != null) target = sun.transform;
            else target = new GameObject("DefaultTarget").transform;
        }

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target)
        {
            // 1. Xoay Camera bằng chuột phải
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * xSpeed;
                y -= Input.GetAxis("Mouse Y") * ySpeed;
                y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
            }

            // 2. ZOOM bằng cuộn chuột
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                // Trừ vì cuộn lên thường là tiến gần (giảm distance)
                distance -= scroll * scrollSpeed;
                // Giới hạn khoảng cách trong tầm cho phép
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            // 3. Tính toán Rotation dựa trên x và y
            Quaternion rotation = Quaternion.Euler(y, x, 0);

            // 4. Tính toán Vị trí mới (Position) dựa trên distance đã zoom
            Vector3 position = target.position + (rotation * new Vector3(0, 0, -distance));

            // 5. Áp dụng Vị trí và Hướng nhìn
            transform.position = position;
            transform.LookAt(target);
        }
    }
}