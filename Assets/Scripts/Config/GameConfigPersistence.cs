using System.IO;
using UnityEngine;

/// <summary>
/// Saves and loads GameConfig values as JSON in Application.persistentDataPath.
/// Auto-loads any saved file before the first scene loads, so JSON overrides
/// the asset defaults for every run of the build.
/// </summary>
public static class GameConfigPersistence
{
    private const string FileName = "gameconfig.json";

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSavedFile => File.Exists(FilePath);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoLoad()
    {
        Load();
    }

    public static bool Load()
    {
        var path = FilePath;
        if (!File.Exists(path)) return false;

        try
        {
            string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, GameConfig.Instance);
            Debug.Log($"GameConfig loaded from {path}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load GameConfig from {path}: {ex.Message}");
            return false;
        }
    }

    public static bool Save()
    {
        var path = FilePath;
        try
        {
            string json = JsonUtility.ToJson(GameConfig.Instance, prettyPrint: true);
            File.WriteAllText(path, json);
            Debug.Log($"GameConfig saved to {path}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save GameConfig to {path}: {ex.Message}");
            return false;
        }
    }

    public static bool DeleteSavedFile()
    {
        var path = FilePath;
        if (!File.Exists(path)) return false;
        try
        {
            File.Delete(path);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to delete {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reset the runtime GameConfig back to the values baked into the .asset.
    /// Does not delete the saved JSON file - call Save() afterwards to persist.
    /// </summary>
    public static void ResetToAssetDefaults()
    {
        var asset = GameConfig.AssetReference;
        if (asset == null)
        {
            asset = Resources.Load<GameConfig>("GameConfig");
        }
        if (asset == null) return;

        string json = JsonUtility.ToJson(asset);
        JsonUtility.FromJsonOverwrite(json, GameConfig.Instance);
    }
}
