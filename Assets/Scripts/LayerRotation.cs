using UnityEngine;
using System.Collections;

public class LayerRotation : MonoBehaviour
{
    public int layerIndex;
    private float holdTime = 0.15f; 
    private float timer = 0;
    private bool isDragging = false;
    
    private Vector3 lastMousePos;
    public float rotationSpeed = 0.4f; // Chỉnh tốc độ xoay tại đây
    public float snapAngle = 15f;    // Góc để khớp (nên tính dựa trên số tile mỗi vòng)

    public void StartManualDrag()
    {
        timer = 0;
        isDragging = false;
        lastMousePos = Input.mousePosition;
        StopAllCoroutines(); // Dừng snap nếu đang snap dở
    }

    public void UpdateManualDrag()
    {
        if (Input.touchCount > 1)
        {
            isDragging = false; 
            return;
        }

        timer += Time.deltaTime;
        
        // Tính toán độ di chuyển của chuột
        Vector3 currentPos = Input.mousePosition;
        Vector3 delta = currentPos - lastMousePos;

        // Nếu chuột di chuyển đủ xa hoặc giữ đủ lâu thì coi là xoay
        if (delta.magnitude > 2f || timer > holdTime)
        {
            isDragging = true;
            // Xoay layer quanh trục Y
            transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.Self);
        }
        lastMousePos = currentPos;
    }

    public void EndManualDrag(TileInteraction tile)
    {
        if (!isDragging)
        {
            // Nếu không phải đang kéo -> Thực hiện chọn Tile
            tile.HandleSelection();
        }
        else
        {
            // Nếu đang kéo -> Thực hiện khớp vị trí
            SnapToGrid();
        }
        isDragging = false;
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
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            yield return null;
        }
        transform.rotation = targetRot;
    }
}