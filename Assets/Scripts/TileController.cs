using UnityEngine;

public class TileController : MonoBehaviour
{
    [Header("Cấu hình ảnh")]
    public SpriteRenderer imageRenderer; // Kéo cái Quad 'TileImage' vào đây
    public Sprite[] tileTextures;   // Danh sách 6 ảnh từ 1 đến 6

    // Hàm này gọi để cập nhật hình ảnh dựa trên ID
    public void UpdateTileVisual(int id)
    {
        // Kiểm tra ID hợp lệ (tránh lỗi ngoài phạm vi mảng)
        // Mảng bắt đầu từ 0 nên lấy ID - 1
        int index = id - 1;

        if (index >= 0 && index < tileTextures.Length)
        {
            // Gán Sprite trực tiếp vào SpriteRenderer của Quad
            imageRenderer.sprite = tileTextures[index];

            FitSpriteToSize();  
        }
        else
        {
            Debug.LogWarning("TileID không hợp lệ hoặc chưa gán đủ ảnh!");
        }
    }

    public void SetImageColor(Color color)
    {
        if (imageRenderer != null)
        {
            imageRenderer.color = color;
        }
    }

    void FitSpriteToSize()
    {
        if (imageRenderer.sprite == null) return;

        // 1. Lấy kích thước hiện tại của Sprite trong không gian World (đơn vị Unity)
        float spriteWidth = imageRenderer.sprite.bounds.size.x;
        float spriteHeight = imageRenderer.sprite.bounds.size.y;

        // 2. Mục tiêu của chúng ta là mặt Cube (thường là 1.0 đơn vị)
        // Bạn có thể chỉnh targetSize là 0.9f nếu muốn ảnh nhỏ hơn mặt cube một chút
        float targetSize = 0.9f; 

        // 3. Tính toán tỉ lệ cần thiết
        float worldScreenWidth = targetSize / spriteWidth;
        float worldScreenHeight = targetSize / spriteHeight;

        // 4. Áp dụng vào Scale của Transform
        imageRenderer.transform.localScale = new Vector3(worldScreenWidth, worldScreenHeight, 1f);
    }
}