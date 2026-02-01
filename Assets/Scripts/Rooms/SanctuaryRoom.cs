using UnityEngine;

/// <summary>
/// Sanctuary room - Followers recover commitment here. No decay.
/// This is the hub for reassigning followers to other rooms.
/// </summary>
public class SanctuaryRoom : Room
{
    private float CommitmentRecoveryPerSecond => GameConfig.Instance.sanctuaryCommitmentRecoveryPerSecond;
    
    protected override void Awake()
    {
        type = RoomType.Sanctuary;
    }
    
    /// <summary>
    /// Sanctuary does not cause commitment decay.
    /// </summary>
    public override bool CausesCommitmentDecay => false;
    
    protected override void Update()
    {
        base.Update();
        
        // Recover commitment for all followers in the sanctuary
        foreach (var follower in followers)
        {
            if (follower != null)
            {
                follower.RecoverCommitment(CommitmentRecoveryPerSecond * Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Sanctuary doesn't have a clock trigger effect - it just passively recovers commitment.
    /// </summary>
    protected override void OnClockTrigger()
    {
        // Sanctuary doesn't trigger effects - it's purely passive recovery
        // Could add a bonus effect here if desired
    }
}
