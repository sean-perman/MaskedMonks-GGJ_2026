using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CreatePlayerInputPrefab
{
    [MenuItem("Tools/Generate PlayerInput Prefab")]
    public static void GeneratePrefab()
    {
        string assetPath = "Assets/Input/PlayerActions.inputactions";
        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
        if (inputAsset == null)
        {
            Debug.LogError($"InputAction asset not found at {assetPath}. Create it first.");
            return;
        }

        // Ensure Prefabs folder exists
        string prefabsFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabsFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        var go = new GameObject("PlayerInput_Prefab");
        var playerInput = go.AddComponent<PlayerInput>();
        playerInput.actions = inputAsset;
        playerInput.defaultControlScheme = "Gamepad";
        playerInput.notificationBehavior = PlayerNotifications.SendMessages;

        // Add bridge script to forward SendMessage calls to PlayerController
        var bridge = go.AddComponent<PlayerInputBridge>();

        string prefabPath = "Assets/Prefabs/PlayerInput.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Generated PlayerInput prefab at {prefabPath}");
    }
}
