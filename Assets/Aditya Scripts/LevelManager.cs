
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private void Start()
    {
        ApplyWorldGrowth();
    }

    public void ApplyWorldGrowth()
    {
        int deaths = RunManager.Instance != null ? RunManager.Instance.DeathCount : 0;

        // Modern Unity API (Unity 2023.1+)
        MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (var script in allScripts)
        {
            if (script is IGrowable growable)
            {
                growable.UpdateGrowth(deaths);
            }
        }
    }
}