using UnityEngine;

/// <summary>
/// Helper component that sets up all debug visualizers in one place.
/// Attach to an empty GameObject in your scene.
/// </summary>
public class DebugSetup : MonoBehaviour
{
    [Header("Dashboard")]
    [SerializeField] private bool enableDashboard = true;
    [SerializeField] private KeyCode dashboardToggle = KeyCode.F12;
    
    [Header("Console")]
    [SerializeField] private bool enableConsole = true;
    [SerializeField] private KeyCode consoleToggle = KeyCode.BackQuote;
    
    [Header("World Space Displays")]
    [SerializeField] private bool showCultDisplays = true;
    [SerializeField] private bool showRoomDisplays = true;
    [SerializeField] private bool showFollowerDisplays = true;
    
    [Header("Color Schemes")]
    public Color cultOneColor = new Color(0.2f, 0.6f, 1f); // Blue
    public Color cultTwoColor = new Color(1f, 0.4f, 0.2f); // Orange
    public Color healthColor = Color.green;
    public Color damageColor = Color.red;
    public Color commitmentHighColor = Color.green;
    public Color commitmentLowColor = Color.red;
    
    private DebugDashboard dashboard;
    private DebugConsole console;
    
    private void Start()
    {
        SetupComponents();
        SetupWorldDisplays();
    }
    
    private void SetupComponents()
    {
        // Setup Dashboard
        if (enableDashboard)
        {
            dashboard = gameObject.AddComponent<DebugDashboard>();
            // Dashboard will auto-configure with default F12 toggle
        }
        
        // Setup Console
        if (enableConsole)
        {
            console = gameObject.AddComponent<DebugConsole>();
        }
    }
    
    private void SetupWorldDisplays()
    {
        // Setup cult displays
        if (showCultDisplays && GameManager.Instance != null)
        {
            SetupCultDisplay(GameManager.Instance.Cult1);
            SetupCultDisplay(GameManager.Instance.Cult2);
        }
        
        // Setup room displays
        if (showRoomDisplays)
        {
            SetupRoomDisplays();
        }
        
        // Setup follower displays
        if (showFollowerDisplays)
        {
            SetupFollowerDisplays();
        }
    }
    
    private void SetupCultDisplay(Cult cult)
    {
        if (cult == null) return;
        
        // CultStatusDisplay will auto-reference the Cult on the same GameObject via SerializeField
        cult.gameObject.AddComponent<CultStatusDisplay>();
    }
    
    private void SetupRoomDisplays()
    {
        var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (var room in rooms)
        {
            room.gameObject.AddComponent<RoomStatusDisplay>();
        }
    }
    
    private void SetupFollowerDisplays()
    {
        var followers = FindObjectsByType<Follower>(FindObjectsSortMode.None);
        foreach (var follower in followers)
        {
            follower.gameObject.AddComponent<FollowerStatusDisplay>();
        }
    }
    
    /// <summary>
    /// Call this to refresh world displays after spawning new entities.
    /// </summary>
    public void RefreshWorldDisplays()
    {
        if (showRoomDisplays)
        {
            // Add displays to any rooms that don't have them
            var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
            foreach (var room in rooms)
            {
                if (room.GetComponent<RoomStatusDisplay>() == null)
                {
                    room.gameObject.AddComponent<RoomStatusDisplay>();
                }
            }
        }
        
        if (showFollowerDisplays)
        {
            // Add displays to any followers that don't have them
            var followers = FindObjectsByType<Follower>(FindObjectsSortMode.None);
            foreach (var follower in followers)
            {
                if (follower.GetComponent<FollowerStatusDisplay>() == null)
                {
                    follower.gameObject.AddComponent<FollowerStatusDisplay>();
                }
            }
        }
    }
    
    private void Update()
    {
        // Quick refresh with F11
        if (Input.GetKeyDown(KeyCode.F11))
        {
            RefreshWorldDisplays();
            Debug.Log("Debug displays refreshed");
        }
    }
    
    private void OnGUI()
    {
        // Show help text
        GUI.color = new Color(1, 1, 1, 0.6f);
        GUI.Label(new Rect(Screen.width - 220, Screen.height - 80, 210, 70),
            "Debug Controls:\n" +
            "F12 - Toggle Dashboard\n" +
            "` (backtick) - Toggle Console\n" +
            "F11 - Refresh Displays");
        GUI.color = Color.white;
    }
}
