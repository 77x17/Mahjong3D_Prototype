using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LevelGenerator : MonoBehaviour
{
    public GameObject tilePrefab;

    private int layers = 4;
    private int ringsPerLayer = 4;
    private float spacingMultiplier = 1.1f; // Hệ số giãn cách (1.1 = 110%)

    private List<Transform> layerParents = new List<Transform>();

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // 1. LẤY KÍCH THƯỚC KHỐI TỪ PREFAB
        // Tạo một bản tạm để đo kích thước nếu prefab chưa có thông tin
        MeshRenderer renderer = tilePrefab.GetComponentInChildren<MeshRenderer>();
        Vector3 tileSize = renderer != null ? renderer.bounds.size : new Vector3(1, 1, 1);
        
        float tileHeight = tileSize.y + 0.1f;
        float safeWidth = Mathf.Max(tileSize.x, tileSize.z) * spacingMultiplier;

        // --- TÍNH TOÁN SNAP ANGLE DỰA TRÊN VÒNG NGOÀI CÙNG ---
        // Bán kính vòng ngoài cùng
        float outerRadius = (ringsPerLayer - 1) * safeWidth;
        // Tính số lượng tile tối đa có thể đặt ở vòng này (dựa trên công thức Sin bạn đã dùng)
        float outerAngleStepRad = 2 * Mathf.Asin(safeWidth / (2 * outerRadius));
        int tilesInOuterRing = Mathf.FloorToInt((Mathf.PI * 2) / outerAngleStepRad);
        
        // Snap Angle = 360 độ / số lượng tile vòng ngoài
        float calculatedSnapAngle = 360f / tilesInOuterRing;
        Debug.Log($"Generated Level with Snap Angle: {calculatedSnapAngle} (based on {tilesInOuterRing} tiles in outer ring)");
        // ---------------------------------------------------

        // Tạo danh sách vị trí tạm thời để trộn ID
        List<Vector3> allPositions = new List<Vector3>();
        List<int> layerIndices = new List<int>(); // Lưu khối này thuộc layer nào

        // 2. TÍNH TOÁN VỊ TRÍ
        for (int l = 0; l < layers; l++)
        {
            float yPos = l * tileHeight;

            // TẠO CONTAINER CHO MỖI TẦNG
            GameObject layerObj = new GameObject("Layer_" + l);
            layerObj.transform.parent = this.transform;

            // ĐẶT TÂM CỦA LAYER TẠI VỊ TRÍ CHÍNH GIỮA CỦA TẦNG ĐÓ
            layerObj.transform.localPosition = new Vector3(0, yPos, 0);
            
            // Thêm component xử lý xoay cho mỗi tầng
            LayerRotation lr = layerObj.AddComponent<LayerRotation>();
            lr.layerIndex = l;
            // Tùy chọn: Tự động tính snapAngle dựa trên số tile (ở đây tạm để 15 độ như cũ)
            lr.snapAngle = calculatedSnapAngle;
            layerParents.Add(layerObj.transform);

            for (int r = 0; r < ringsPerLayer; r++)
            {
                // Các vòng ngoài: Bán kính phải đảm bảo không chạm vòng trong
                float radius = r * safeWidth;
                if (radius == 0) { continue; } 

                // Tính số lượng ô dựa trên hàm Sin để đảm bảo cạnh các ô không chạm nhau
                // Công thức: sin(alpha/2) = (safeWidth/2) / radius
                float angleStepRad = 2 * Mathf.Asin(safeWidth / (2 * radius));
                int tilesInRing = Mathf.FloorToInt((Mathf.PI * 2) / angleStepRad);
                
                // Tính toán lại góc chia đều để vòng tròn khép kín đẹp
                float finalAngleStep = (Mathf.PI * 2) / tilesInRing;

                for (int i = 0; i < tilesInRing; i++)
                {
                    float currentAngle = i * finalAngleStep;
                    Vector3 localPos = new Vector3(
                        Mathf.Cos(currentAngle) * radius,
                        0, 
                        Mathf.Sin(currentAngle) * radius
                    );
                    allPositions.Add(localPos);
                    layerIndices.Add(l);
                }
            }
        }

        // 4. TẠO ID THEO BỘ 3
        List<int> idList = new List<int>();
        for (int i = 0; i < allPositions.Count / 6; i++)
        {
            idList.Add(1); idList.Add(2); idList.Add(3); 
            idList.Add(4); idList.Add(5); idList.Add(6);
        }

        // Trộn ID
        for (int i = 0; i < idList.Count; i++)
        {
            int temp = idList[i];
            int randomIndex = Random.Range(i, idList.Count);
            idList[i] = idList[randomIndex];
            idList[randomIndex] = temp;
        }

        // 5. SINH KHỐI
        for (int i = 0; i < allPositions.Count; i++)
        {
            int layerIdx = layerIndices[i];
            Transform parentTransform = layerParents[layerIdx];
            
            GameObject tile = Instantiate(tilePrefab);
            tile.transform.SetParent(parentTransform);
            tile.transform.localPosition = allPositions[i];
            
            tile.transform.LookAt(parentTransform.TransformPoint(Vector3.zero));

            TileInteraction ti = tile.GetComponent<TileInteraction>();
            ti.tileID = idList[i];
            ti.layerIndex = layerIdx;
            GameManager.Instance.allTiles.Add(ti);

            // Gán ID
            if (tile.TryGetComponent<TileInteraction>(out TileInteraction interaction))
            {
                interaction.tileID = idList[i];
            }

            // Hiển thị số
            TextMeshProUGUI textComp = tile.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = idList[i].ToString();
            }
        }
    }
}