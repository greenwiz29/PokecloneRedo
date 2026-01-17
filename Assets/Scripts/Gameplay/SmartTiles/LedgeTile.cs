using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Ledge Tile")]
public class LedgeTile : Tile
{
    public Vector2Int allowedEntryDir; // e.g. (0, -1) for jumping down
    public int jumpDistance = 2;       // future-proofing
}
