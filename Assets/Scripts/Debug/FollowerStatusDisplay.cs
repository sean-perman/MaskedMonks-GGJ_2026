using UnityEngine;

/// <summary>
/// Displays follower commitment as a floating bar.
/// Attach to each follower GameObject for visual debugging.
/// </summary>
public class FollowerStatusDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Follower follower;
    
    [Header("Display Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0);
    [SerializeField] private float barWidth = 40f;
    [SerializeField] private float barHeight = 6f;
    [SerializeField] private bool autoFindFollower = true;
    [SerializeField] private bool showName = false;
    
    private void Start()
    {
        if (autoFindFollower && follower == null)
        {
            follower = GetComponent<Follower>();
        }
    }
    
    private void OnGUI()
    {
        if (follower == null || Camera.main == null) return;
        
        Vector3 worldPos = transform.position + offset;
        Vector3 screenPos3D = Camera.main.WorldToScreenPoint(worldPos);
        
        // Behind camera check
        if (screenPos3D.z < 0) return;
        
        Vector2 screenPos = new Vector2(screenPos3D.x, Screen.height - screenPos3D.y);
        
        float commitment = follower.CommitmentPercent;
        
        // Background bar
        Rect bgRect = new Rect(screenPos.x - barWidth / 2, screenPos.y - barHeight / 2, barWidth, barHeight);
        GUI.color = Color.black;
        GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
        
        // Commitment bar
        GUI.color = GetCommitmentColor(commitment);
        Rect fillRect = new Rect(bgRect.x + 1, bgRect.y + 1, (bgRect.width - 2) * commitment, bgRect.height - 2);
        GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
        
        // Border
        GUI.color = Color.white;
        
        // Optional name label
        if (showName)
        {
            GUI.Label(new Rect(screenPos.x - 30, screenPos.y + barHeight, 60, 16), follower.name);
        }
        
        GUI.color = Color.white;
    }
    
    private Color GetCommitmentColor(float percent)
    {
        if (percent < 0.25f) return Color.red;
        if (percent < 0.5f) return new Color(1f, 0.5f, 0f); // Orange
        if (percent < 0.75f) return Color.yellow;
        return Color.green;
    }
    
    /// <summary>
    /// Set the follower to display.
    /// </summary>
    public void SetFollower(Follower newFollower)
    {
        follower = newFollower;
    }
}
