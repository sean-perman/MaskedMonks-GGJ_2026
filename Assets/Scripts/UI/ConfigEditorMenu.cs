using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// Runtime editor for GameConfig values. Reflects over GameConfig fields,
/// renders an OnGUI form grouped by [Header] attributes, and writes changes
/// to a JSON file via GameConfigPersistence.
///
/// Designed to be attached alongside MainMenuController (or any GameObject)
/// and toggled via SetVisible() / Toggle(). Pauses the game while open.
/// </summary>
public class ConfigEditorMenu : MonoBehaviour
{
    private bool isOpen;
    private float previousTimeScale = 1f;

    private Vector2 scroll;
    private readonly Dictionary<string, string> textBuffers = new();
    private string statusMessage = "";
    private float statusUntilTime;

    private GUIStyle headerStyle;
    private GUIStyle sectionStyle;
    private GUIStyle labelStyle;
    private GUIStyle pathStyle;
    private GUIStyle buttonStyle;
    private GUIStyle textFieldStyle;
    private bool stylesInit;

    private FieldInfo[] cachedFields;

    public bool IsOpen => isOpen;

    public void Toggle() => SetVisible(!isOpen);

    public void SetVisible(bool visible)
    {
        if (visible == isOpen) return;
        isOpen = visible;

        if (isOpen)
        {
            RefreshBuffers();
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = previousTimeScale;
        }
    }

    private FieldInfo[] GetConfigFields()
    {
        if (cachedFields == null)
        {
            cachedFields = typeof(GameConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);
        }
        return cachedFields;
    }

    private void RefreshBuffers()
    {
        textBuffers.Clear();
        var config = GameConfig.Instance;
        foreach (var f in GetConfigFields())
        {
            textBuffers[f.Name] = ValueToString(f.GetValue(config));
        }
    }

    private static string ValueToString(object v)
    {
        if (v is float f) return f.ToString("0.###", CultureInfo.InvariantCulture);
        return v?.ToString() ?? "";
    }

    private static bool TryParseField(FieldInfo field, string text, out object result)
    {
        result = null;
        if (field.FieldType == typeof(int))
        {
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { result = v; return true; }
        }
        else if (field.FieldType == typeof(float))
        {
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) { result = v; return true; }
        }
        else if (field.FieldType == typeof(bool))
        {
            if (bool.TryParse(text, out var v)) { result = v; return true; }
        }
        else if (field.FieldType == typeof(string))
        {
            result = text;
            return true;
        }
        return false;
    }

    private void InitStyles()
    {
        if (stylesInit) return;

        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        headerStyle.normal.textColor = Color.white;

        sectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        sectionStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        labelStyle.normal.textColor = Color.white;

        pathStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
        pathStyle.normal.textColor = new Color(1, 1, 1, 0.55f);

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13 };
        textFieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = 13 };

        stylesInit = true;
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        InitStyles();

        // Dim background
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float w = Mathf.Min(960f, Screen.width - 80f);
        float h = Mathf.Min(720f, Screen.height - 80f);
        Rect window = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
        GUI.Box(window, GUIContent.none);

        GUILayout.BeginArea(new Rect(window.x + 16, window.y + 16, window.width - 32, window.height - 32));

        GUILayout.Label("CONFIG EDITOR", headerStyle);
        GUILayout.Label($"Save file: {GameConfigPersistence.FilePath}", pathStyle);
        if (Time.unscaledTime < statusUntilTime)
        {
            GUILayout.Label(statusMessage, labelStyle);
        }
        else
        {
            GUILayout.Space(15);
        }
        GUILayout.Space(4);

        scroll = GUILayout.BeginScrollView(scroll);

        var config = GameConfig.Instance;
        foreach (var f in GetConfigFields())
        {
            var headerAttr = f.GetCustomAttribute<HeaderAttribute>();
            if (headerAttr != null)
            {
                GUILayout.Space(10);
                GUILayout.Label(headerAttr.header, sectionStyle);
            }

            DrawFieldRow(f, config);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", buttonStyle, GUILayout.Height(32)))
        {
            int parseFails = ApplyAllBuffers();
            if (parseFails > 0)
            {
                SetStatus($"Save: {parseFails} field(s) failed to parse - see console.");
            }
            else if (GameConfigPersistence.Save())
            {
                SetStatus("Saved.");
            }
            else
            {
                SetStatus("Save failed - see console.");
            }
        }
        if (GUILayout.Button("Reload from File", buttonStyle, GUILayout.Height(32)))
        {
            if (GameConfigPersistence.Load())
            {
                RefreshBuffers();
                SetStatus("Reloaded from file.");
            }
            else
            {
                SetStatus(GameConfigPersistence.HasSavedFile ? "Reload failed - see console." : "No saved file yet.");
            }
        }
        if (GUILayout.Button("Reset to Defaults", buttonStyle, GUILayout.Height(32)))
        {
            GameConfigPersistence.ResetToAssetDefaults();
            RefreshBuffers();
            SetStatus("Reset to asset defaults (not saved).");
        }
        if (GUILayout.Button("Open Save Folder", buttonStyle, GUILayout.Height(32)))
        {
            OpenSaveFolder();
        }
        if (GUILayout.Button("Close", buttonStyle, GUILayout.Height(32)))
        {
            SetVisible(false);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void DrawFieldRow(FieldInfo field, GameConfig config)
    {
        GUILayout.BeginHorizontal();

        string label = NicifyName(field.Name);
        string tooltip = field.GetCustomAttribute<TooltipAttribute>()?.tooltip ?? "";
        GUILayout.Label(new GUIContent(label, tooltip), labelStyle, GUILayout.Width(280));

        if (!textBuffers.TryGetValue(field.Name, out var current))
        {
            current = ValueToString(field.GetValue(config));
            textBuffers[field.Name] = current;
        }
        string updated = GUILayout.TextField(current, textFieldStyle, GUILayout.Width(160));
        if (updated != current)
        {
            textBuffers[field.Name] = updated;
        }

        string typeHint = field.FieldType == typeof(int) ? "int"
                        : field.FieldType == typeof(float) ? "float"
                        : field.FieldType == typeof(bool) ? "bool"
                        : field.FieldType.Name;
        GUILayout.Label($"({typeHint})", labelStyle, GUILayout.Width(60));

        GUILayout.EndHorizontal();
    }

    private int ApplyAllBuffers()
    {
        int fails = 0;
        var config = GameConfig.Instance;
        foreach (var f in GetConfigFields())
        {
            if (!textBuffers.TryGetValue(f.Name, out var text)) continue;
            if (TryParseField(f, text, out var parsed))
            {
                f.SetValue(config, parsed);
            }
            else
            {
                Debug.LogWarning($"GameConfig: could not parse '{text}' as {f.FieldType.Name} for field {f.Name}");
                fails++;
            }
        }
        return fails;
    }

    private void SetStatus(string msg)
    {
        statusMessage = msg;
        statusUntilTime = Time.unscaledTime + 4f;
    }

    private static void OpenSaveFolder()
    {
        string folder = System.IO.Path.GetDirectoryName(GameConfigPersistence.FilePath);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.RevealInFinder(GameConfigPersistence.FilePath);
#else
        Application.OpenURL("file:///" + folder.Replace('\\', '/'));
#endif
    }

    private static string NicifyName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        var sb = new StringBuilder(raw.Length + 8);
        for (int i = 0; i < raw.Length; i++)
        {
            char c = raw[i];
            if (i == 0)
            {
                sb.Append(char.ToUpper(c));
            }
            else if (char.IsUpper(c) && !char.IsUpper(raw[i - 1]))
            {
                sb.Append(' ').Append(c);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
