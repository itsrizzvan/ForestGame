using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("Number of Chimpanzees to spawn for this level")]
    public int totalChimpanzeesToSpawn = 2;
    public GameObject chimpanzeePrefab;
    public Transform[] spawnPoints;

    [Header("Combat Coordination")]
    [Tooltip("Max enemies allowed to strike the player simultaneously")]
    public int maxSimultaneousAttackers = 1; 
    private int currentAttackers = 0;

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        SpawnWave();
    }

    void SpawnWave()
    {
        for (int i = 0; i < totalChimpanzeesToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            Vector3 spawnPos = spawnPoint.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
            
            GameObject enemy = Instantiate(chimpanzeePrefab, spawnPos, Quaternion.identity);
            
            ChimpanzeeAI ai = enemy.GetComponent<ChimpanzeeAI>();
            if (ai != null)
            {
                ai.Initialize(this,i);
            }

            activeEnemies.Add(enemy);
        }
    }

    public bool RequestAttackPermission()
    {
        if (currentAttackers < maxSimultaneousAttackers)
        {
            currentAttackers++;
            return true;
        }
        return false;
    }

    public void ReleaseAttackPermission()
    {
        currentAttackers = Mathf.Max(0, currentAttackers - 1);
    }

    public void OnEnemyDied(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        if (activeEnemies.Count == 0)
        {
            Debug.Log("Wave Cleared!");
            // Trigger next wave or level progression logic here
        }
    }
}