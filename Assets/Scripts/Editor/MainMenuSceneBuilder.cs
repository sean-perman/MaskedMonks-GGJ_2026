#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click setup for the MainMenu scene + Build Settings.
/// Run via "MaskedMonks > Setup Main Menu Scene" in the Unity menu.
/// </summary>
public static class MainMenuSceneBuilder
{
    private const string MenuPath = "MaskedMonks/Setup Main Menu Scene";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        // Make sure the user has saved any unsaved changes in the active scene.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        // Create a fresh scene with the default Camera + Light objects.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Add the MainMenu controller GameObject.
        var go = new GameObject("MainMenu");
        go.AddComponent<MainMenuController>();
        // The helper screens are auto-added via [RequireComponent], but we add
        // them explicitly so they're visible in the saved scene asset.
        if (go.GetComponent<ConfigEditorMenu>() == null)
        {
            go.AddComponent<ConfigEditorMenu>();
        }
        if (go.GetComponent<MainMenuControlsScreen>() == null)
        {
            go.AddComponent<MainMenuControlsScreen>();
        }

        // Make sure Assets/Scenes exists, then save the scene.
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);

        // Wire Build Settings: MainMenu first, gameplay second.
        var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        buildScenes.RemoveAll(s => s.path == MainMenuScenePath);

        bool hasGameplay = buildScenes.Exists(s => s.path == GameplayScenePath);

        // Insert MainMenu at index 0 so it boots first.
        buildScenes.Insert(0, new EditorBuildSettingsScene(MainMenuScenePath, true));

        if (!hasGameplay && System.IO.File.Exists(GameplayScenePath))
        {
            buildScenes.Add(new EditorBuildSettingsScene(GameplayScenePath, true));
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Main Menu Scene Created",
            $"Created {MainMenuScenePath} and added it to Build Settings as scene 0.\n\n" +
            (hasGameplay
                ? "SampleScene was already in Build Settings."
                : (System.IO.File.Exists(GameplayScenePath)
                    ? "SampleScene was added to Build Settings."
                    : "WARNING: SampleScene.unity was not found - add your gameplay scene to Build Settings manually.")) +
            "\n\nPress Play with the MainMenu scene open to test.",
            "OK");
    }
}
#endif
