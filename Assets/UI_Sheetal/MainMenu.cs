using System.Collections; // <-- Required for Coroutines (timers)
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using UnityEngine.UI; // <-- Required to interact with UI Images

public class MainMenu : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string tutorialSceneName = "tutorial level";
    [SerializeField] private float delayBeforeLoad = 0.5f;

    [Header("UI Feedback")]
    [Tooltip("Drag the PlayButton GameObject here to access its Image component")]
    [SerializeField] private Image playButtonImage;
    [Tooltip("The sprite you want to appear when A is pressed")]
    [SerializeField] private Sprite pressedButtonSprite;
    
    private bool hasTriggered = false;

    private void Update()
    {
        // Check if the bottom face button ('A' on Xbox) was pressed
        if (!hasTriggered && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            StartCoroutine(LoadTutorialRoutine());
        }
    }

    // Keep this public so your Button's OnClick() event can still trigger it if clicked with a mouse
    public void StartTutorial()
    {
        if (!hasTriggered)
        {
            StartCoroutine(LoadTutorialRoutine());
        }
    }

    private IEnumerator LoadTutorialRoutine()
    {
        hasTriggered = true;

        // 1. Swap the sprite to show the button was pressed
        if (playButtonImage != null && pressedButtonSprite != null)
        {
            playButtonImage.sprite = pressedButtonSprite;
        }

        // 2. Wait for the delay timer to finish so the player can see the new sprite
        yield return new WaitForSeconds(delayBeforeLoad);

        // 3. Load the next scene (using the fader if it exists, otherwise standard load)
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeToScene(tutorialSceneName);
        }
        else
        {
            SceneManager.LoadScene(tutorialSceneName);
        }
    }
}