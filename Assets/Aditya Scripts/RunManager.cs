using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    // Explicit field and property to prevent compilation mismatch
    [SerializeField] private int deathCount = 0;
    public int DeathCount 
    { 
        get { return deathCount; } 
        private set { deathCount = value; } 
    }

    [SerializeField] private int currentLevel = 1;
    public int CurrentLevel 
    { 
        get { return currentLevel; } 
        private set { currentLevel = value; } 
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayerDeath()
    {
        deathCount++;
        Debug.Log($"Player Died! Total Deaths: {deathCount}");
    }

    public void AdvanceLevel()
    {
        currentLevel++;
        Debug.Log($"Advanced to Level {currentLevel}");
    }
}