using System.Collections.Generic;
using UnityEngine;

public class LevelChunkSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ChunkSlot
    {
        public string zoneName;
        public Transform spawnLocation;
        public List<GameObject> chunkPrefabs;
    }

    public List<ChunkSlot> levelZones;
    public LevelManager levelManager;

    private void Start()
    {
        Debug.Log("🚀 LevelChunkSpawner: Start() fired!");
        GenerateLayout();
    }

    public void GenerateLayout()
    {
        if (levelZones == null || levelZones.Count == 0)
        {
            Debug.LogError("❌ LevelChunkSpawner: 'levelZones' list is EMPTY in the Inspector!");
            return;
        }

        int totalSpawned = 0;

        foreach (var slot in levelZones)
        {
            if (slot.spawnLocation == null)
            {
                Debug.LogError($"❌ Zone '{slot.zoneName}' has NO Spawn Location assigned in Inspector!");
                continue;
            }

            if (slot.chunkPrefabs == null || slot.chunkPrefabs.Count == 0)
            {
                Debug.LogError($"❌ Zone '{slot.zoneName}' has NO prefabs in 'Chunk Prefabs' array!");
                continue;
            }

            // Pick a random prefab
            int randomIndex = Random.Range(0, slot.chunkPrefabs.Count);
            GameObject selectedPrefab = slot.chunkPrefabs[randomIndex];

            if (selectedPrefab == null)
            {
                Debug.LogError($"❌ Zone '{slot.zoneName}' has an unassigned (Element null) slot in Chunk Prefabs!");
                continue;
            }

            // Spawn prefab at the anchor position
            GameObject spawnedChunk = Instantiate(selectedPrefab, slot.spawnLocation.position, slot.spawnLocation.rotation, slot.spawnLocation);
            totalSpawned++;
            Debug.Log($"✅ Successfully spawned '{selectedPrefab.name}' under '{slot.spawnLocation.name}'!");
        }

        Debug.Log($"🎉 Level Generation Complete! Total Chunks Spawned: {totalSpawned}");

        // Automatically trigger tree growth on newly spawned chunks
        LevelManager mgr = levelManager != null ? levelManager : FindFirstObjectByType<LevelManager>();
        if (mgr != null)
        {
            mgr.ApplyWorldGrowth();
        }
    }

    public void RebuildLevel()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("RebuildLevel can only be run during Play Mode!");
            return;
        }

        Debug.Log("🔄 Rebuilding Level Chunks...");

        // Destroy existing chunks
        foreach (var slot in levelZones)
        {
            if (slot.spawnLocation == null) continue;

            for (int i = slot.spawnLocation.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.spawnLocation.GetChild(i).gameObject);
            }
        }

        // Generate fresh ones
        GenerateLayout();
    }
}