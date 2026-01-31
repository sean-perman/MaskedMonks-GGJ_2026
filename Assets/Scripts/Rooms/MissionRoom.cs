using UnityEngine;

/// <summary>
/// Mission room - Recruits a citizen from the Marketplace when triggered.
/// Followers decay commitment while working here.
/// </summary>
public class MissionRoom : Room
{
    protected override void Awake()
    {
        type = RoomType.Mission;
    }
    
    /// <summary>
    /// When the mission triggers, recruit a citizen from the marketplace.
    /// </summary>
    protected override void OnClockTrigger()
    {
        // TODO: Get reference to Marketplace and recruit a citizen
        var marketplace = Marketplace.Instance;
        if (marketplace != null && cult != null)
        {
            var citizen = marketplace.RecruitCitizen();
            if (citizen != null)
            {
                cult.AddFollower(citizen);
                Debug.Log("Mission triggered! Recruited a new follower.");
            }
            else
            {
                Debug.Log("Mission triggered but no citizens available in marketplace.");
            }
        }
    }
}
