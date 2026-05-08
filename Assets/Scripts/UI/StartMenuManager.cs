using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

/// <summary>
/// Manages the Start Menu scene, including audio initialization for WebGL builds.
/// Handles the start button click and resumes FMOD audio system.
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    private bool audioResumed = false;

    private void Awake()
    {
        // Resume audio immediately on scene load for WebGL compatibility
        ResumeAudio();
    }

    /// <summary>
    /// Resumes FMOD audio system. Required for WebGL builds as FMOD initializes in a suspended state.
    /// </summary>
    public void ResumeAudio()
    {
        if (!audioResumed)
        {
            FMODUnity.RuntimeManager.CoreSystem.mixerSuspend();
            FMODUnity.RuntimeManager.CoreSystem.mixerResume();
            audioResumed = true;
            Debug.Log("[StartMenuManager] FMOD audio resumed successfully");
        }
    }

    /// <summary>
    /// Called when the start button is clicked. Loads the main game scene.
    /// </summary>
    public void OnStartButtonClicked()
    {
        Debug.Log("[StartMenuManager] Start button clicked, loading game scene");
        SceneManager.LoadScene("SampleScene");
    }
}
