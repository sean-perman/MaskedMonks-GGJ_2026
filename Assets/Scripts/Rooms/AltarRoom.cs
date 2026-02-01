using UnityEngine;

/// <summary>
/// Altar room - Generates God Strength when triggered.
/// Followers decay commitment while working here.
/// </summary>
public class AltarRoom : Room
{
    private int StrengthPerTrigger => GameConfig.Instance.altarStrengthPerTrigger;
    
    public override ResourceType GeneratedResource => ResourceType.Strength;
    
    protected override void Awake()
    {
        type = RoomType.Altar;
        duration = GameConfig.Instance.altarDuration;
    }
    
    /// <summary>
    /// When the altar triggers, increase the cult's god strength.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult != null && cult.god != null)
        {
            AudioManager.PlayRoomTriggerAltar();
            cult.god.IncreaseStrength(StrengthPerTrigger);
            NotifyResourceGenerated(ResourceType.Strength, StrengthPerTrigger);
            Debug.Log($"Altar triggered! God gained {StrengthPerTrigger} strength.");
        }
    }
}
