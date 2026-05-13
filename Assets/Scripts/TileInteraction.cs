using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    public int tileID;
    public int layerIndex;
    public Color highlightColor = Color.yellow;
    private Color originalColor = Color.white;
    private Renderer tileRenderer;
    private bool isSelected = false;
    private LayerRotation parentLayer;

    // Kích thước để kiểm tra va chạm
    private Vector3 halfExtents;

    private TileController controller;

    public bool isBlocked { get; private set; }

    void Awake()
    {
        controller = GetComponent<TileController>();
        tileRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        // Tìm script LayerRotation ở đối tượng cha (Layer_x)
        parentLayer = GetComponentInParent<LayerRotation>();

        // Lấy kích thước thực tế của khối để dùng cho BoxCast
        // Chúng ta lấy hơi nhỏ hơn thực tế một chút (0.95f) để tránh va chạm nhầm với cạnh bên
        if (tileRenderer != null) {
            halfExtents = tileRenderer.bounds.extents * 0.95f;
        } else {
            halfExtents = new Vector3(0.5f, 0.5f, 0.5f);
        }

        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (controller != null)
        {
            controller.UpdateTileVisual(tileID);
        }
    }

    public bool IsBlockedByLogic()
    {
        // Nếu là tầng trên cùng thì không bị ai đè
        // (Giả sử layers là tổng số tầng bạn có trong LevelGenerator)
        
        foreach (TileInteraction other in GameManager.Instance.allTiles)
        {
            // Chỉ kiểm tra những khối nằm ở tầng ngay trên nó
            if (other.layerIndex == this.layerIndex + 1)
            {
                // Tính khoảng cách ngang (X, Z) giữa khối này và khối tầng trên
                // Chúng ta bỏ qua Y vì chúng ta biết chắc 'other' đang ở tầng trên
                Vector3 thisPos = transform.position;
                Vector3 otherPos = other.transform.position;

                float distanceXZ = Vector2.Distance(
                    new Vector2(thisPos.x, thisPos.z), 
                    new Vector2(otherPos.x, otherPos.z)
                );

                // Nếu khoảng cách nhỏ hơn kích thước khối, nghĩa là có sự đè lên nhau
                // Giả sử safeWidth của bạn là 1.1, ngưỡng 0.8 là đủ để phát hiện đè 1 nửa
                if (distanceXZ < 0.8f) 
                {
                    return true; // Bị đè
                }
            }
        }
        return false;
    }

    // Hàm xử lý logic chọn Mahjong (được gọi từ LayerRotation)
    public void HandleSelection()
    {
        if (!isSelected)
        {
            if (IsBlockedByLogic())
            {
                AudioManager.Instance.PlaySFX("wrong");

                Debug.Log("Không thể chọn vì bị tầng trên đè!");
                return;
            }

            AudioManager.Instance.PlaySFX("select");

            isSelected = true;
            tileRenderer.material.color = highlightColor;

            if (controller != null)
            {
                controller.SetImageColor(highlightColor);
            }
            
            if (GameManager.Instance != null)
                GameManager.Instance.SelectTile(this);
        }
        else
        {
            Deselect();

            if (GameManager.Instance != null)
                GameManager.Instance.RemoveTileFromList(this);
        }
    }

    public void Deselect()
    {
        isSelected = false;
        tileRenderer.material.color = originalColor;

        if (controller != null)
        {
            controller.SetImageColor(Color.white);
        }
    }

    // Các hàm bắt sự kiện chuột từ Unity
    void OnMouseDown() 
    {
        if (parentLayer != null) parentLayer.StartManualDrag(this.isBlocked);
    }

    void OnMouseDrag() 
    {
        if (parentLayer != null) parentLayer.UpdateManualDrag();
    }

    void OnMouseUp() 
    {
        if (parentLayer != null) parentLayer.EndManualDrag(this);
    }

    // Hàm đổi trạng thái và màu sắc
    public void SetBlocked(bool state)
    {
        isBlocked = state;

        if (isBlocked && isSelected)
        {
            isSelected = false;

            GameManager.Instance.RemoveTileFromList(this);
        }

        if (tileRenderer != null)
        {
            // Nếu bị chặn thì màu xám, không thì trả về màu gốc
            tileRenderer.material.color = state ? Color.gray : (isSelected ? highlightColor : originalColor);
        }
        if (controller != null)
        {
            controller.SetImageColor(state ? Color.gray : (isSelected ? highlightColor : originalColor));
        }
        // Debug.Log($"Tile {tileID} set blocked: {state}.");
    }
}