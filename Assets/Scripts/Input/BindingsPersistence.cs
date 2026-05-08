using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Saves and loads per-player input bindings as JSON in
/// Application.persistentDataPath. Used by the main-menu controls screen so
/// edits persist across game sessions, and by PlayerController.Initialize
/// to seed bindings on a fresh PlayerController.
/// </summary>
public static class BindingsPersistence
{
    private const string FileName = "bindings.json";

    [Serializable]
    public class BindingsData
    {
        public PlayerInputBindings player1;
        public PlayerInputBindings player2;
    }

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
    public static bool HasSavedFile => File.Exists(FilePath);

    private static BindingsData _cache;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoLoad()
    {
        Load();
    }

    /// <summary>
    /// Returns the cached bindings (loading from disk on first access, falling
    /// back to defaults). Mutating the returned bindings affects the cache;
    /// call Save() to persist.
    /// </summary>
    public static BindingsData GetOrLoadCache()
    {
        if (_cache != null) return _cache;

        Load();
        if (_cache == null)
        {
            _cache = new BindingsData
            {
                player1 = PlayerInputBindings.CreatePlayer1Defaults(),
                player2 = PlayerInputBindings.CreatePlayer2Defaults()
            };
        }
        return _cache;
    }

    /// <summary>Get a freshly cloned copy of the saved bindings for a given player index.</summary>
    public static PlayerInputBindings GetBindingsFor(int playerIndex)
    {
        var data = GetOrLoadCache();
        var src = playerIndex == 0 ? data.player1 : data.player2;
        if (src == null)
        {
            return playerIndex == 0
                ? PlayerInputBindings.CreatePlayer1Defaults()
                : PlayerInputBindings.CreatePlayer2Defaults();
        }
        return src.Clone();
    }

    public static bool Load()
    {
        var path = FilePath;
        if (!File.Exists(path)) return false;

        try
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<BindingsData>(json);
            if (data == null) return false;
            // Defensive: replace any null sub-fields with defaults so callers can
            // assume player1/player2 are non-null.
            if (data.player1 == null) data.player1 = PlayerInputBindings.CreatePlayer1Defaults();
            if (data.player2 == null) data.player2 = PlayerInputBindings.CreatePlayer2Defaults();
            _cache = data;
            Debug.Log($"Bindings loaded from {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load bindings from {path}: {ex.Message}");
            return false;
        }
    }

    public static bool Save()
    {
        var data = GetOrLoadCache();
        var path = FilePath;
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(path, json);
            Debug.Log($"Bindings saved to {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save bindings to {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reset the cached bindings (and what GetBindingsFor returns) to the
    /// hardcoded per-player defaults. Does not delete the JSON file - call Save() afterwards.
    /// </summary>
    public static void ResetToDefaults()
    {
        _cache = new BindingsData
        {
            player1 = PlayerInputBindings.CreatePlayer1Defaults(),
            player2 = PlayerInputBindings.CreatePlayer2Defaults()
        };
    }
}
