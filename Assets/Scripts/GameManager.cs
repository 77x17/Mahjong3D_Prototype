using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<TileInteraction> selectedTiles = new List<TileInteraction>();

    public List<TileInteraction> allTiles = new List<TileInteraction>();

    void Awake() { Instance = this; }

    public void SelectTile(TileInteraction tile)
    {
        if (selectedTiles.Contains(tile)) return;

        selectedTiles.Add(tile);

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

    void CheckMatch()
    {
        if (selectedTiles[0].tileID == selectedTiles[1].tileID && 
            selectedTiles[1].tileID == selectedTiles[2].tileID)
        {
            // Khớp nhau -> Xóa
            foreach (var t in selectedTiles) {
                UnregisterTile(t);
                Destroy(t.gameObject);
            }
            Debug.Log("Đã xóa 3 khối giống nhau!");
        }
        else
        {
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
}