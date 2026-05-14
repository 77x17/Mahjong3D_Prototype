using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<TileInteraction> selectedTiles = new List<TileInteraction>();

    public List<TileInteraction> allTiles = new List<TileInteraction>();

    public TextMeshProUGUI countTile1Text;
    public TextMeshProUGUI countTile2Text;
    public TextMeshProUGUI countTile3Text;
    public TextMeshProUGUI countTile4Text;
    public TextMeshProUGUI countTile5Text;
    public TextMeshProUGUI countTile6Text;

    const int MAX_COUNT_PER_TILE = 6;

    int countTile1, countTile2, countTile3, countTile4, countTile5, countTile6;

    Vector3 actualTileSize;

    [SerializeField] private GameObject breakParticlePrefab;

    void Awake() { 
        Instance = this; 

        countTile1 = MAX_COUNT_PER_TILE;
        countTile2 = MAX_COUNT_PER_TILE;
        countTile3 = MAX_COUNT_PER_TILE;
        countTile4 = MAX_COUNT_PER_TILE;
        countTile5 = MAX_COUNT_PER_TILE;
        countTile6 = MAX_COUNT_PER_TILE;

        countTile1Text.text = $"{MAX_COUNT_PER_TILE}";
        countTile2Text.text = $"{MAX_COUNT_PER_TILE}";
        countTile3Text.text = $"{MAX_COUNT_PER_TILE}";
        countTile4Text.text = $"{MAX_COUNT_PER_TILE}";
        countTile5Text.text = $"{MAX_COUNT_PER_TILE}";
        countTile6Text.text = $"{MAX_COUNT_PER_TILE}";
    }

    public void SelectTile(TileInteraction tile)
    {
        if (selectedTiles.Contains(tile)) return;

        selectedTiles.Add(tile);

        if (selectedTiles.Count == 2)
        {
            CheckPreviousMatch();
        }

        if (selectedTiles.Count == 3)
        {
            CheckMatch();
        }
    }

    public void RemoveTileFromList(TileInteraction tile)
    {
        if (selectedTiles.Contains(tile))
        {
            selectedTiles.Remove(tile);
        }
    }

    void CheckPreviousMatch()
    {
        if (selectedTiles[0].tileID != selectedTiles[1].tileID)
        {
            AudioManager.Instance.PlaySFX("wrong");

            // Không khớp -> Bỏ chọn tất cả
            foreach (var t in selectedTiles) t.Deselect();

            selectedTiles.Clear();
        }
    }

    void CheckMatch()
    {
        if (selectedTiles[0].tileID == selectedTiles[1].tileID && 
            selectedTiles[1].tileID == selectedTiles[2].tileID)
        {
            // AudioManager.Instance.PlaySFX("break");
            List<TileInteraction> matchGroup = new List<TileInteraction>(selectedTiles);
            // Khớp nhau -> Xóa
            HashSet<int> layersToUpdate = new HashSet<int>();
            foreach (var t in selectedTiles) {
                int savedLayerIndex = t.layerIndex;
                layersToUpdate.Add(savedLayerIndex);
                
                UnregisterTile(t);
                t.GetComponent<Collider>().enabled = false;

                // Destroy(t.gameObject);

                UpdateTileCountUI(t.tileID);

                t.Deselect();
            }

            StartCoroutine(AnimateMatchSuccess(matchGroup));

            foreach (int index in layersToUpdate)
            {
                UpdateAffectedLayers(index);
            }
            
            Debug.Log("Đã xóa 3 khối giống nhau!");
        }
        else
        {
            AudioManager.Instance.PlaySFX("wrong");

            // Không khớp -> Bỏ chọn tất cả
            foreach (var t in selectedTiles) t.Deselect();
            Debug.Log("Không khớp, thử lại!");
        }
        selectedTiles.Clear();
    }

    void UpdateTileCountUI(int id)
    {
        switch (id) {
            case 1: countTile1 = Mathf.Max(0, countTile1 - 1); countTile1Text.text = $"{countTile1}"; break;
            case 2: countTile2 = Mathf.Max(0, countTile2 - 1); countTile2Text.text = $"{countTile2}"; break;
            case 3: countTile3 = Mathf.Max(0, countTile3 - 1); countTile3Text.text = $"{countTile3}"; break;
            case 4: countTile4 = Mathf.Max(0, countTile4 - 1); countTile4Text.text = $"{countTile4}"; break;
            case 5: countTile5 = Mathf.Max(0, countTile5 - 1); countTile5Text.text = $"{countTile5}"; break;
            case 6: countTile6 = Mathf.Max(0, countTile6 - 1); countTile6Text.text = $"{countTile6}"; break;
        }
    }

    // Khi tile bị xóa, phải xóa khỏi danh sách allTiles
    public void UnregisterTile(TileInteraction tile) {
        allTiles.Remove(tile);
    }

    IEnumerator AnimateMatchSuccess(List<TileInteraction> tiles)
    {
        float flyDuration = 0.3f;   
        float waitDuration = 0.2f;  
        float crushDuration = 0.1f;
        float elapsed = 0;

        if (actualTileSize == Vector3.zero) {
            MeshRenderer sampleRenderer = tiles[0].GetComponentInChildren<MeshRenderer>();
            actualTileSize = sampleRenderer != null ? sampleRenderer.bounds.size : new Vector3(1f, 0.5f, 1.3f);
        }

        float originalWidth = Mathf.Max(actualTileSize.x, actualTileSize.z);
        float scaledWidth = originalWidth * 0.3f; 
        float padding = 0.3f; // Một chút khoảng cách nhỏ giữa các khối cho đẹp
        float spacing = scaledWidth / 2.0f + padding;

        // 1. Tính toán vị trí và kích thước
        // Z = 4f là khoảng cách vừa đủ để khối chiếm diện tích đẹp trên màn hình
        float distanceCam = 4f; 
        Vector3 centerPoint = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.75f, distanceCam));

        Vector3[] targetPositions = new Vector3[3];
        Vector3 rightDir = Camera.main.transform.right;
        targetPositions[0] = centerPoint - rightDir * spacing;
        targetPositions[1] = centerPoint;
        targetPositions[2] = centerPoint + rightDir * spacing;

        // 2. Lưu trạng thái bắt đầu
        Vector3[] startPos = new Vector3[3];
        Quaternion[] startRot = new Quaternion[3];
        Vector3[] startScales = new Vector3[3];
        Vector3[] targetScales = new Vector3[3];

        for (int i = 0; i < 3; i++) {
            if (tiles[i] == null) continue;
            startPos[i] = tiles[i].transform.position;
            startRot[i] = tiles[i].transform.rotation;
            startScales[i] = tiles[i].transform.localScale;
            // Mục tiêu là nhỏ lại một nửa so với lúc đang cầm
            targetScales[i] = startScales[i] * 0.5f; 
        }

        float arcWidth = Mathf.Max(actualTileSize.x, actualTileSize.z) * 1.0f;

        // 3. GIAI ĐOẠN 1: Bay lên và xoay mặt Upper về Camera
        Quaternion faceCamRot = Quaternion.LookRotation(Camera.main.transform.up, -Camera.main.transform.forward);
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Min(1f, elapsed / flyDuration);
            // float t = elapsed / flyDuration;
            float curve = t * t * (3f - 2f * t); 

            for (int i = 0; i < 3; i++)
            {
                if (tiles[i] == null) continue;

                Vector3 basePos = Vector3.Lerp(startPos[i], targetPositions[i], curve);
                float arc = Mathf.Sin(t * Mathf.PI) * arcWidth;
                if (i == 0) // Khối bên trái: đẩy quỹ đạo lệch sang trái
                {
                    tiles[i].transform.position = basePos - rightDir * arc;
                }
                else if (i == 2) // Khối bên phải: đẩy quỹ đạo lệch sang phải
                {
                    tiles[i].transform.position = basePos + rightDir * arc;
                }
                else
                {
                    tiles[i].transform.position = basePos;
                }
                
                // tiles[i].transform.position = Vector3.Lerp(startPos[i], targetPositions[i], curve);

                tiles[i].transform.localScale = Vector3.Lerp(startScales[i], targetScales[i], curve);

                tiles[i].transform.rotation = Quaternion.Slerp(startRot[i], faceCamRot, curve);
            }
            yield return null;
        }

        for (int i = 0; i < 3; i++)
        {
            if (tiles[i] == null) continue;
            tiles[i].transform.position = targetPositions[i];
            tiles[i].transform.localScale = targetScales[i];
            tiles[i].transform.rotation = faceCamRot;
        }

        AudioManager.Instance.PlaySFX("break");
        // 4. GIAI ĐOẠN 2: Khựng lại để khoe hình ảnh
        yield return new WaitForSeconds(waitDuration);

        // 5. GIAI ĐOẠN 3: Lao vào nhau và thu nhỏ
        elapsed = 0;
        Vector3 finalCenter = targetPositions[1];

        while (elapsed < crushDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crushDuration;
            float easeIn = t * t; // Lao vào nhanh dần

            for (int i = 0; i < 3; i++)
            {
                if (tiles[i] == null) continue;
                tiles[i].transform.position = Vector3.Lerp(targetPositions[i], finalCenter, easeIn);
                tiles[i].transform.localScale = Vector3.Lerp(targetScales[i], Vector3.zero, easeIn);
            }
            yield return null;
        }

        if (breakParticlePrefab != null)
        {
            // Sinh ra hiệu ứng tại vị trí finalCenter (tâm va chạm)
            GameObject fx = Instantiate(breakParticlePrefab, finalCenter, Quaternion.identity);
            
            // Xóa object hiệu ứng sau một khoảng thời gian (ví dụ 1.5 giây) 
            // để tránh rác bộ nhớ. Hãy điều chỉnh số 1.5f bằng với thời gian chạy của Particle.
            Destroy(fx, 1.5f);
        }

        foreach (var t in tiles) if (t != null) Destroy(t.gameObject);
    }

    public void CheckBlockingTiles()
    {
        Debug.Log("Checking blocking tiles for " + allTiles.Count + " tiles.");
        foreach (TileInteraction tile in allTiles)
        {
            tile.SetBlocked(tile.IsBlockedByLogic());
        }
    }

    public void CheckBlockingForLayer(int layerIndex)
    {
        // Tìm tất cả các gạch thuộc layer cụ thể này
        foreach (TileInteraction tile in allTiles)
        {
            if (tile.layerIndex == layerIndex)
            {
                // Cập nhật trạng thái bị chặn cho từng viên trong layer này
                tile.SetBlocked(tile.IsBlockedByLogic());
            }
        }
    }
    public void UpdateAffectedLayers(int changedLayerIndex)
    {
        if (changedLayerIndex > 0) 
        {
            CheckBlockingForLayer(changedLayerIndex - 1);
        }
    }

    public void CheckBlockedSelectedTiles()
    {
        for (int i = selectedTiles.Count - 1; i >= 0; i--)
        {
            TileInteraction tile = selectedTiles[i];

            if (tile == null) continue;

            if (tile.IsBlockedByLogic())
            {
                Debug.Log($"Khối {tile.tileID} trong danh sách chọn đã bị chặn sau khi xoay!");
                    
                // CÁCH 1: Trả khối đó về trạng thái bị chặn (màu xám) và xóa khỏi danh sách chọn
                tile.Deselect();
                tile.SetBlocked(true);
                selectedTiles.RemoveAt(i);
            }
        }
    }
}