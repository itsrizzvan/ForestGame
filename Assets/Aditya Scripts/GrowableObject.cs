using UnityEngine;

public class GrowableObject : MonoBehaviour, IGrowable
{
    [Header("Growth Stages")]
    [Tooltip("Stage 0 = 0 Deaths, Stage 1 = 1-2 Deaths, Stage 2 = 3+ Deaths")]
    public GameObject[] stageVisuals; 

    [Header("Unlocks")]
    public GameObject grapplePoint; // Enable grapple node when grown

    public void UpdateGrowth(int deathCount)
    {
        // Figure out which stage to display based on death count
        int activeStage = Mathf.Clamp(deathCount, 0, stageVisuals.Length - 1);

        // Turn off all stages, then turn on only the active one
        for (int i = 0; i < stageVisuals.Length; i++)
        {
            if (stageVisuals[i] != null)
                stageVisuals[i].SetActive(i == activeStage);
        }

        // Enable grapple point if stage 1 or higher
        if (grapplePoint != null)
        {
            grapplePoint.SetActive(activeStage > 0);
        }
    }
}