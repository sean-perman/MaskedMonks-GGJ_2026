using UnityEngine;

/// <summary>
/// Pews room - Generates Favor when triggered.
/// Followers do NOT decay commitment while working here.
/// </summary>
public class PewsRoom : Room
{
    private int FavorPerTrigger => GameConfig.Instance.pewsFavorPerTrigger;
    
    public override ResourceType GeneratedResource => ResourceType.Favor;
    
    protected override void Awake()
    {
        type = RoomType.Pews;
        duration = GameConfig.Instance.pewsDuration;
    }
    
    /// <summary>
    /// Pews do not cause commitment decay.
    /// </summary>
    public override bool CausesCommitmentDecay => false;
    
    /// <summary>
    /// When the pews trigger, increase the cult's god favor.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult != null && cult.god != null)
        {
            AudioManager.PlayRoomTriggerPew();
            cult.god.IncreaseFavor(FavorPerTrigger);
            NotifyResourceGenerated(ResourceType.Favor, FavorPerTrigger);
            Debug.Log($"Pews triggered! God gained {FavorPerTrigger} favor.");
        }
    }
}
