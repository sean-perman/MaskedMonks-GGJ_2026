using UnityEngine;

/// <summary>
/// Workshop room - Generates Architecture masks for building new rooms.
/// Followers decay commitment while working here.
/// </summary>
public class WorkshopRoom : Room
{
    [Header("Workshop Settings")]
    [Tooltip("The types of architecture masks this workshop can generate")]
    [SerializeField] private MaskType[] possibleBlueprints = new MaskType[]
    {
        MaskType.ArchitectSanctuary,
        MaskType.ArchitectAltar,
        MaskType.ArchitectPews,
        MaskType.ArchitectMission,
        MaskType.ArchitectRitualHall,
        MaskType.ArchitectWorkshop
    };
    
    protected override void Awake()
    {
        type = RoomType.Workshop;
    }
    
    /// <summary>
    /// When the workshop triggers, generate an architecture mask (blueprint).
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult != null && cult.god != null)
        {
            // Pick a random blueprint type
            if (possibleBlueprints.Length > 0)
            {
                var blueprintType = possibleBlueprints[Random.Range(0, possibleBlueprints.Length)];
                
                // TODO: Create the actual mask and add to god's storage
                // For now, just log
                Debug.Log($"Workshop triggered! Generated a {blueprintType} blueprint.");
                
                // cult.god.AddMaskToStorage(newMask);
            }
        }
    }
}
