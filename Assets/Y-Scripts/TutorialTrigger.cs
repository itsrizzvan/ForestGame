using UnityEngine;

public enum TriggerBehavior
{
    ShowTutorial,
    HideTutorial
}

[RequireComponent(typeof(BoxCollider))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Should this trigger show a new tutorial, or hide the current one?")]
    public TriggerBehavior behavior = TriggerBehavior.ShowTutorial;

    [Header("Tutorial Content (If Showing)")]
    [TextArea] public string instructionText = "Press [Button] to do Action!";
    public Sprite instructionIcon; 
    
    [Header("Clear Condition")]
    public TutorialAction requiredAction = TutorialAction.None;

    private bool hasTriggered = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            
            if (TutorialUIManager.Instance != null)
            {
                if (behavior == TriggerBehavior.ShowTutorial)
                {
                    // Shows the new tutorial (automatically replacing any old one on screen)
                    TutorialUIManager.Instance.ShowTutorial(instructionText, instructionIcon, requiredAction);
                }
                else if (behavior == TriggerBehavior.HideTutorial)
                {
                    // Instantly clears the screen
                    TutorialUIManager.Instance.HideTutorial();
                }
            }
        }
    }
}