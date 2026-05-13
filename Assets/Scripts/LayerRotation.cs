using UnityEngine;
using System.Collections;

public class LayerRotation : MonoBehaviour
{
    public int layerIndex;
    private float holdTime = 0.15f; 
    private float soundTime = 1.0f; 
    private float thresholdSpace = 5f; 
    private float timer = 0;
    private bool isDragging = false;
    
    private Camera mainCam;
    public float rotationSpeed = 0.5f; // Với cách tính mới, để 1.0 là mặc định
    public float snapAngle = 15f; 
    public float snapSpeed = 15f;
    private float lastMouseAngle;

    private bool canRotate = false;
    private float accumulatedAngle = 0f; // Tổng góc đã xoay kể từ tiếng kêu trước
    public float angleStepForSound = 50f;

    void Start()
    {
        mainCam = Camera.main;
    }

    public void StartManualDrag(bool isTileBlocked)
    {
        timer = 0;
        isDragging = false;

        canRotate = !isTileBlocked;

        if (!canRotate) return;
        
        lastMouseAngle = GetMouseAngle();

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
        float currentMouseAngle = GetMouseAngle();
        
        float delta = Mathf.DeltaAngle(currentMouseAngle, lastMouseAngle);
        lastMouseAngle = currentMouseAngle;

        if (Mathf.Abs(delta) > thresholdSpace || timer > holdTime)
        {
            if (!isDragging) {
                AudioManager.Instance.PlaySFX("drag");
                isDragging = true;
                timer = 0.0f;
                accumulatedAngle = 0.0f;
            } 
            
            if (isDragging) {
                float rotationAmount = -delta * rotationSpeed;
                transform.Rotate(Vector3.up, -delta * rotationSpeed);

                accumulatedAngle += Mathf.Abs(rotationAmount);
                if (accumulatedAngle >= angleStepForSound && timer >= soundTime)
                {
                    AudioManager.Instance.PlaySFX("drag");
                    accumulatedAngle -= angleStepForSound;
                    timer -= soundTime;
                }
            }
        }
    }

    public void EndManualDrag(TileInteraction tile)
    {
        if (!canRotate || !isDragging)
        {
            tile.HandleSelection();
        }
        else
        {
            SnapToGrid();
        }
        isDragging = false;
        canRotate = false;
    }

    // Hàm quan trọng: Lấy góc của chuột so với tâm vật thể trên mặt phẳng XZ
    private float GetMouseAngle()
    {
        // Tạo một mặt phẳng nằm ngang đi qua tâm của Layer
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = hitPoint - transform.position;
            
            // Trả về góc (độ) của vector hướng trên mặt phẳng XZ
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
        
        // Sử dụng Quaternion.Slerp để mượt mà hơn Lerp thông thường
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.05f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * snapSpeed);
            yield return null;
        }
        transform.rotation = targetRot;

        GameManager.Instance.CheckBlockingForLayer(this.layerIndex);
        GameManager.Instance.UpdateAffectedLayers(this.layerIndex);
        GameManager.Instance.CheckBlockedSelectedTiles();
    }
}