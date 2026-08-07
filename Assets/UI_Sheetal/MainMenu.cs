using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartTutorial()
    {
        // This must match your scene name exactly
        SceneManager.LoadScene("tutorial level");
    }
}