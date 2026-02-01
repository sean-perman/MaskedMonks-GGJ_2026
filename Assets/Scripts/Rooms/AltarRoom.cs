using UnityEngine;

/// <summary>
/// Altar room - Generates God Strength when triggered.
/// Followers decay commitment while working here.
/// </summary>
public class AltarRoom : Room
{
    [Header("Altar Settings")]
    [SerializeField] private int strengthPerTrigger = 1;
    
    public override ResourceType GeneratedResource => ResourceType.Strength;
    
    protected override void Awake()
    {
        type = RoomType.Altar;
    }
    
    /// <summary>
    /// When the altar triggers, increase the cult's god strength.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult != null && cult.god != null)
        {
            cult.god.IncreaseStrength(strengthPerTrigger);
            NotifyResourceGenerated(ResourceType.Strength, strengthPerTrigger);
            Debug.Log($"Altar triggered! God gained {strengthPerTrigger} strength.");
        }
    }
}
