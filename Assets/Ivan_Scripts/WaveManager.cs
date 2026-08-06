using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int totalChimpanzeesToSpawn = 4;
    public GameObject chimpanzeePrefab;
    public Transform[] spawnPoints;

    [Header("Combat Coordination")]
    [Tooltip("Maximum simultaneous attackers allowed (Cap enforced at max 3)")]
    [Range(1, 3)]
    public int maxSimultaneousAttackers = 3;

    private int activeAttackers = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        SpawnWave();
    }

    void SpawnWave()
    {
        if (chimpanzeePrefab == null)
        {
            Debug.LogError("WaveManager: Chimpanzee Prefab is not assigned!");
            return;
        }

        for (int i = 0; i < totalChimpanzeesToSpawn; i++)
        {
            Vector3 basePosition = (spawnPoints != null && spawnPoints.Length > 0)
                ? spawnPoints[Random.Range(0, spawnPoints.Length)].position
                : transform.position;

            Vector3 spawnPos = basePosition + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
            GameObject enemy = Instantiate(chimpanzeePrefab, spawnPos, Quaternion.identity);

            ChimpanzeeAI ai = enemy.GetComponent<ChimpanzeeAI>();
            if (ai != null)
            {
                ai.Initialize(this, i);
            }

            activeEnemies.Add(enemy);
        }
    }

    public bool RequestAttackPermission()
    {
        if (activeAttackers < maxSimultaneousAttackers)
        {
            activeAttackers++;
            return true;
        }
        return false;
    }

    public void ReleaseAttackPermission()
    {
        activeAttackers = Mathf.Max(0, activeAttackers - 1);
    }

    public void OnEnemyDied(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
}