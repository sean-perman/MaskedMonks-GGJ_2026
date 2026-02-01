using UnityEngine;

/// <summary>
/// Pews room - Generates Favor when triggered.
/// Followers do NOT decay commitment while working here.
/// </summary>
public class PewsRoom : Room
{
    [Header("Pews Settings")]
    [SerializeField] private int favorPerTrigger = 1;
    
    public override ResourceType GeneratedResource => ResourceType.Favor;
    
    protected override void Awake()
    {
        type = RoomType.Pews;
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
            cult.god.IncreaseFavor(favorPerTrigger);
            NotifyResourceGenerated(ResourceType.Favor, favorPerTrigger);
            Debug.Log($"Pews triggered! God gained {favorPerTrigger} favor.");
        }
    }
}
