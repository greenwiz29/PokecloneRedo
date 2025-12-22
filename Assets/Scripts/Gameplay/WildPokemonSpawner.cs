using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildPokemonSpawner : MonoBehaviour
{
    [SerializeField] GameObject wildPokemonPrefab;
    [SerializeField] int maxActive = 20;
    [SerializeField] float spawnInterval = 4f;
    [SerializeField] float spawnRadius = 6f;
    [SerializeField] BattleTrigger battleTrigger;

    readonly List<GameObject> active = new();
    MapArea mapArea;

    void Awake()
    {
        mapArea = SceneMapAreaRegistry.GetMapAreaForScene(gameObject.scene);

        if (mapArea == null)
        {
            Debug.LogError(
                $"Spawner {name} could not resolve MapArea for scene {gameObject.scene.name}"
            );
            enabled = false;
            return;
        }
    }

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            active.RemoveAll(p => p == null);

            if (active.Count >= maxActive)
                continue;

            SpawnPokemon();
        }
    }

    private void SpawnPokemon()
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + (Vector3)offset;

        var obj = Instantiate(wildPokemonPrefab, spawnPos, Quaternion.identity);
        var controller = obj.GetComponent<WildPokemonController>();

        controller.Init(mapArea, spawnPos, battleTrigger);

        active.Add(obj);
    }

}
