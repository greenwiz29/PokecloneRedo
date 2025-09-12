using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    void Awake()
    {
        var existingObjects = FindObjectsByType<EssentialObjects>(FindObjectsSortMode.None);
        if (existingObjects.Length == 0)
        {
            // if there's a grid, spawn at its center
            var spawnPos = new Vector3(0, 0, 0);
            var grid = FindFirstObjectByType<Grid>();
            if (grid != null)
            {
                spawnPos = grid.transform.position;
            }
            Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}
