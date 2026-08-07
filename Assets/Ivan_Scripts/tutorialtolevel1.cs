using UnityEngine;
using UnityEngine.SceneManagement;

public class tutorialtolevel1 : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string nextSceneName = "LevlDesignOne";
    [SerializeField] private float fadeDuration = 0.8f;

    // Prevents the trigger from firing multiple times while the fade is happening
    private bool hasTriggered = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            // Trigger the ultra-smooth fade we created earlier
            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.FadeToScene(nextSceneName, fadeDuration);
            }
            else
            {
                // Fallback just in case the ScreenFader canvas is missing from the scene
                Debug.LogWarning("ScreenFader not found in scene! Loading instantly.");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}