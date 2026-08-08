using UnityEngine;

public class GrowableObject : MonoBehaviour, IGrowable
{
    [Header("Growth Stages")]
    public GameObject[] stageVisuals; 

    [Header("Unlocks")]
    public GameObject grapplePoint; 

    public void UpdateGrowth(int deathCount)
    {
        // Explicitly map death count ranges to stages
        int activeStage = 0;
        if (deathCount >= 3) activeStage = 2;      // 3+ Deaths -> Stage 2
        else if (deathCount >= 1) activeStage = 1; // 1-2 Deaths -> Stage 1
        else activeStage = 0;                      // 0 Deaths -> Stage 0

        // Clamp just in case stageVisuals has fewer elements than activeStage
        activeStage = Mathf.Min(activeStage, stageVisuals.Length - 1);

        // Turn off all stages, enable ONLY the target stage
        for (int i = 0; i < stageVisuals.Length; i++)
        {
            if (stageVisuals[i] != null)
                stageVisuals[i].SetActive(i <= activeStage);
        }

        if (grapplePoint != null)
        {
            grapplePoint.SetActive(activeStage > 0);
        }
    }
}