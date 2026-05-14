using UnityEngine;
using System.Collections;

public class LayerRotation : MonoBehaviour
{
    public int layerIndex;
    
    private float holdTime = 0.15f;       // Tăng nhẹ thời gian giữ (giây) để tránh nhận nhầm khi click nhanh
    private float thresholdSpace = 30f;    // Tăng khoảng cách di chuyển (pixel) mới bắt đầu xoay
    private float timer = 0;
    private Vector2 startMousePosition;   // Vị trí lúc bắt đầu chạm
    private bool isDragging = false;
    
    private Camera mainCam;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 0.25f; 
    public float smoothSpeed = 12f;      // Tăng nhẹ để bám tay hơn nhưng vẫn mượt
    public float snapAngle = 15f; 
    public float snapSpeed = 15f;
    
    private float lastMouseAngle;
    private Vector2 lastMousePosition;
    private float targetRotationY; 

    [Header("Settings cho vùng ngoài")]
    public float rotationSensitivity = 0.15f; 
    public float circleRadiusThreshold = 2.0f;

    private bool canRotate = false;
    private float accumulatedAngle = 0f; 
    public float angleStepForSound = 50f;
    private float soundTime = 1.0f; 

    void Start()
    {
        mainCam = Camera.main;
        targetRotationY = transform.eulerAngles.y;
    }

    public void StartManualDrag(bool isTileBlocked)
    {
        timer = 0;
        isDragging = false;
        canRotate = !isTileBlocked;

        if (!canRotate) return;
        
        lastMouseAngle = GetMouseAngle();
        startMousePosition = Input.mousePosition; // Lưu vị trí bắt đầu để tính quãng đường di chuyển
        lastMousePosition = Input.mousePosition; 
        targetRotationY = transform.eulerAngles.y;

        StopAllCoroutines(); 
    }

    public void UpdateManualDrag()
    {
        if (!canRotate) return;

        if (Input.touchCount > 1)
        {
            isDragging = false; 
            return;
        }

        timer += Time.deltaTime;
        
        // Tính khoảng cách chuột đã di chuyển kể từ lúc nhấn xuống
        float moveDistance = Vector2.Distance(Input.mousePosition, startMousePosition);

        // CHỈ KÍCH HOẠT XOAY KHI: Di chuyển đủ xa HOẶC giữ đủ lâu
        if (!isDragging)
        {
            if (moveDistance > thresholdSpace && timer > holdTime)
            {
                AudioManager.Instance.PlaySFX("drag");
                isDragging = true;
                timer = 0.0f;
                accumulatedAngle = 0.0f;
            }
            else
            {
                // Nếu chưa đủ điều kiện drag thì chưa tính toán xoay
                lastMousePosition = Input.mousePosition;
                lastMouseAngle = GetMouseAngle();
                return;
            }
        }

        float deltaAmount = 0f;
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            float distanceFromCenter = Vector3.Distance(hitPoint, transform.position);

            if (distanceFromCenter <= circleRadiusThreshold)
            {
                float currentMouseAngle = GetMouseAngle();
                float deltaAngle = Mathf.DeltaAngle(currentMouseAngle, lastMouseAngle);
                deltaAmount = -deltaAngle * rotationSpeed;
                lastMouseAngle = currentMouseAngle;
            }
            else
            {
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 mouseDelta = currentMousePos - lastMousePosition;
                Vector3 screenPosOfCenter = mainCam.WorldToScreenPoint(transform.position);
                Vector2 relativePosScreen = currentMousePos - new Vector2(screenPosOfCenter.x, screenPosOfCenter.y);

                float horizontalSide = (relativePosScreen.y > 0) ? 1f : -1f;
                float verticalSide = (relativePosScreen.x > 0) ? -1f : 1f;

                deltaAmount = (mouseDelta.x * horizontalSide + mouseDelta.y * verticalSide) * rotationSensitivity;
                lastMouseAngle = GetMouseAngle(); 
            }
        }
        
        lastMousePosition = Input.mousePosition;

        // Xử lý Xoay thực tế
        if (isDragging) {
            targetRotationY += deltaAmount;
            float nextY = Mathf.LerpAngle(transform.eulerAngles.y, targetRotationY, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Euler(0, nextY, 0);

            // Âm thanh
            accumulatedAngle += Mathf.Abs(deltaAmount);
            if (accumulatedAngle >= angleStepForSound && timer >= soundTime)
            {
                AudioManager.Instance.PlaySFX("drag");
                accumulatedAngle -= angleStepForSound;
                timer -= soundTime;
            }
        }
    }

    public void EndManualDrag(TileInteraction tile)
    {
        // Nếu ngón tay chưa di chuyển đủ xa và chưa giữ đủ lâu -> Tính là Click vào Tile
        float moveDistance = Vector2.Distance(Input.mousePosition, startMousePosition);
        
        if (!isDragging && moveDistance < thresholdSpace && timer < holdTime)
        {
            tile.HandleSelection();
        }
        else if (isDragging)
        {
            SnapToGrid();
        }
        
        isDragging = false;
        canRotate = false;
    }

    // Các hàm GetMouseAngle, SnapToGrid, SmoothSnap giữ nguyên như cũ...
    private float GetMouseAngle()
    {
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = hitPoint - transform.position;
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
        return 0;
    }

    public void SnapToGrid()
    {
        float currentY = transform.eulerAngles.y;
        float snappedY = Mathf.Round(currentY / snapAngle) * snapAngle;
        StartCoroutine(SmoothSnap(snappedY));
    }

    IEnumerator SmoothSnap(float targetY)
    {
        Quaternion targetRot = Quaternion.Euler(0, targetY, 0);
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.05f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * snapSpeed);
            yield return null;
        }
        transform.rotation = targetRot;
        targetRotationY = targetY; 
        GameManager.Instance.CheckBlockingForLayer(this.layerIndex);
        GameManager.Instance.UpdateAffectedLayers(this.layerIndex);
        GameManager.Instance.CheckBlockedSelectedTiles();
    }
}