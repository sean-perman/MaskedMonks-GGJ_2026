using UnityEngine;

/// <summary>
/// Mission room - Recruits a citizen from the Marketplace when triggered.
/// Followers decay commitment while working here.
/// </summary>
public class MissionRoom : Room
{
    public override ResourceType GeneratedResource => ResourceType.Follower;
    
    protected override void Awake()
    {
        type = RoomType.Mission;
        duration = GameConfig.Instance.missionDuration;
    }
    
    /// <summary>
    /// When the mission triggers, recruit a citizen from the marketplace.
    /// </summary>
    protected override void OnClockTrigger()
    {
        var marketplace = Marketplace.Instance;
        if (marketplace != null && cult != null)
        {
            var citizen = marketplace.RecruitCitizen();
            if (citizen != null)
            {
                AudioManager.PlayRoomTriggerMission();
                cult.AddFollower(citizen);
                NotifyResourceGenerated(ResourceType.Follower, 1);

                // Shake the sanctuary - cult.AddFollower places newly-recruited
                // citizens there if there's space, so this is the receiving room.
                var sanctuary = cult.church != null ? cult.church.GetRoomOfType(RoomType.Sanctuary) : null;
                if (sanctuary != null)
                {
                    var shake = sanctuary.GetComponent<ShakeEffect>();
                    if (shake == null) shake = sanctuary.gameObject.AddComponent<ShakeEffect>();
                    shake.Trigger();
                }

                Debug.Log("Mission triggered! Recruited a new follower.");
            }
            else
            {
                Debug.Log("Mission triggered but no citizens available in marketplace.");
            }
        }
    }
}
