using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple main menu with a play button.
/// Attach to a Canvas in your MainMenu scene.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name of the game scene to load")]
    [SerializeField] private string gameSceneName = "Game";

    /// <summary>
    /// Called when the Play button is clicked.
    /// Loads the game scene.
    /// </summary>
    public void PlayGame()
    {
        Debug.Log($"Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Called when the Quit button is clicked.
    /// Exits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        // Also stop play mode in editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
