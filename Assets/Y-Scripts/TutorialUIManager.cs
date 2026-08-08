using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TutorialAction
{
    None,       // The UI stays until a new trigger overwrites it or turns it off
    Move,
    Jump,
    Dash,
    Grapple,
    Attack,
    Block,
    Sprint      // <-- NEW
}

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject tutorialPanel; 
    public Image tutorialIcon;
    public TextMeshProUGUI tutorialText;

    [Header("Player Reference")]
    public PlayerInputHandler playerInput;

    private TutorialAction currentRequiredAction;
    private bool isTutorialActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public void ShowTutorial(string text, Sprite icon, TutorialAction requiredAction)
    {
        tutorialText.text = text;
        
        if (icon != null)
        {
            tutorialIcon.sprite = icon;
            tutorialIcon.enabled = true;
        }
        else
        {
            tutorialIcon.enabled = false;
        }

        currentRequiredAction = requiredAction;
        tutorialPanel.SetActive(true);
        isTutorialActive = true;
    }

    // A public method so other triggers can force the UI to close
    public void HideTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        isTutorialActive = false;
    }

    private void Update()
    {
        if (!isTutorialActive || playerInput == null) return;

        bool actionCompleted = false;

        switch (currentRequiredAction)
        {
            case TutorialAction.None:
                // Do nothing. Wait for a new trigger to call HideTutorial() or ShowTutorial()
                break;
            case TutorialAction.Move:
                if (playerInput.MoveInput.magnitude > 0.1f) actionCompleted = true; //[cite: 5]
                break;
            case TutorialAction.Jump:
                if (playerInput.JumpTriggered) actionCompleted = true; //[cite: 5]
                break;
            case TutorialAction.Dash:
                if (playerInput.DashTriggered) actionCompleted = true; //[cite: 5]
                break;
            case TutorialAction.Grapple:
                if (playerInput.GrappleTriggered) actionCompleted = true; //[cite: 5]
                break;
            case TutorialAction.Attack:
                if (playerInput.AttackTriggered) actionCompleted = true; //[cite: 5]
                break;
            case TutorialAction.Block:
                if (playerInput.IsBlocking) actionCompleted = true; //[cite: 5]
                break;
            case TutorialAction.Sprint:
                if (playerInput.IsSprinting) actionCompleted = true; //[cite: 5]
                break;
        }

        if (actionCompleted)
        {
            HideTutorial();
        }
    }
}