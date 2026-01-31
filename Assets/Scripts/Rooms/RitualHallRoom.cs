using UnityEngine;

/// <summary>
/// Ritual Hall room - Generates a new Mask when triggered.
/// Followers decay commitment while working here.
/// </summary>
public class RitualHallRoom : Room
{
    [Header("Ritual Hall Settings")]
    [Tooltip("The types of masks this ritual hall can generate")]
    [SerializeField] private MaskType[] possibleMasks = new MaskType[]
    {
        MaskType.Smiting,
        MaskType.Wrath,
        MaskType.Whispers,
        MaskType.Sanctuary,
        MaskType.Plenty,
        MaskType.Sacrifice
    };
    
    protected override void Awake()
    {
        type = RoomType.RitualHall;
    }
    
    /// <summary>
    /// When the ritual hall triggers, generate a new mask for the god.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult != null && cult.god != null)
        {
            // Pick a random mask type from possible masks
            if (possibleMasks.Length > 0)
            {
                var maskType = possibleMasks[Random.Range(0, possibleMasks.Length)];
                
                // TODO: Create the actual mask and add to god's storage
                // For now, just log
                Debug.Log($"Ritual Hall triggered! Generated a {maskType} mask.");
                
                // cult.god.AddMaskToStorage(newMask);
            }
        }
    }
}
