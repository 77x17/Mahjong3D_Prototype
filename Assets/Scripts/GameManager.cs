using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
            AudioManager.Instance.PlaySFX("break");
            // Khớp nhau -> Xóa
            HashSet<int> layersToUpdate = new HashSet<int>();
            foreach (var t in selectedTiles) {
                int savedLayerIndex = t.layerIndex;
                layersToUpdate.Add(savedLayerIndex);
                
                UnregisterTile(t);
                Destroy(t.gameObject);

                switch (t.tileID) {
                    case 1: countTile1 = Mathf.Max(0, countTile1 - 1); countTile1Text.text = $"{countTile1}"; break;
                    case 2: countTile2 = Mathf.Max(0, countTile2 - 1); countTile2Text.text = $"{countTile2}"; break;
                    case 3: countTile3 = Mathf.Max(0, countTile3 - 1); countTile3Text.text = $"{countTile3}"; break;
                    case 4: countTile4 = Mathf.Max(0, countTile4 - 1); countTile4Text.text = $"{countTile4}"; break;
                    case 5: countTile5 = Mathf.Max(0, countTile5 - 1); countTile5Text.text = $"{countTile5}"; break;
                    case 6: countTile6 = Mathf.Max(0, countTile6 - 1); countTile6Text.text = $"{countTile6}"; break;
                }
            }

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

    // Khi tile bị xóa, phải xóa khỏi danh sách allTiles
    public void UnregisterTile(TileInteraction tile) {
        allTiles.Remove(tile);
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