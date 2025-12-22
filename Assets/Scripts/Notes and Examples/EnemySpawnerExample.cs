using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawnerExample : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float checkInterval = 1f;

    // List to keep track of active enemies
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        StartCoroutine(MaintainEnemyCount());
    }

    IEnumerator MaintainEnemyCount()
    {
        while (true)
        {
            // 1. Clean up the list by removing null (destroyed) references
            // Destroy(gameObject, lifeTime) for a timed death, 
            // or Destroy(gameObject) from Interact/OnBattleOver function
            activeEnemies.RemoveAll(enemy => enemy == null);

            // 2. If below limit, spawn a replacement
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }

            // 3. Wait before checking again to save performance
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void SpawnEnemy()
    {
        Vector3 randomPos = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        GameObject newEnemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);
        
        // Add to our tracking list
        activeEnemies.Add(newEnemy);
    }
}
