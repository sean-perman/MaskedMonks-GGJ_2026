using UnityEngine;

/// <summary>
/// Workshop room - Repairs all rooms in the church.
/// On a configurable pawn-second timer, applies one repair to every damaged room.
/// Followers decay commitment while working here.
/// </summary>
public class WorkshopRoom : Room
{
    public override ResourceType GeneratedResource => ResourceType.Repair;
    
    protected override void Awake()
    {
        type = RoomType.Workshop;
        duration = GameConfig.Instance.workshopRepairDuration;
    }
    
    /// <summary>
    /// When the workshop triggers, apply one repair to every damaged room in the church.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null || cult.church == null)
        {
            Debug.LogWarning("Workshop cannot repair - missing cult/church reference!");
            return;
        }
        
        int roomsRepaired = 0;
        
        // Repair all damaged rooms in the church
        foreach (var room in cult.church.Rooms)
        {
            if (room.Damage > 0)
            {
                room.RepairDamage(1);
                roomsRepaired++;
            }
        }
        
        // Always notify to spawn visual indicator (shows wrench icon)
        NotifyResourceGenerated(ResourceType.Repair, Mathf.Max(1, roomsRepaired));
        
        if (roomsRepaired > 0)
        {
            Debug.Log($"Workshop repaired {roomsRepaired} room(s)!");
        }
        else
        {
            Debug.Log("Workshop triggered but no rooms needed repair.");
        }
    }
}
