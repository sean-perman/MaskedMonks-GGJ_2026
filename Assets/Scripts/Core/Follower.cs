using UnityEngine;

/// <summary>
/// A single cultist that can be assigned to rooms.
/// Followers have commitment that decays while working and recovers in the Sanctuary.
/// </summary>
public class Follower : MonoBehaviour
{
    [Header("Commitment")]
    [SerializeField] private float commitment = 100f;
    
    private float MaxCommitmentConfig => GameConfig.Instance.followerMaxCommitment;
    private float DecayRatePerSecond => GameConfig.Instance.followerDecayRatePerSecond;
    
    [Header("References")]
    [SerializeField] private Room currentRoom;
    [SerializeField] private Cult cult;
    
    // === Properties ===
    
    public float Commitment => commitment;
    public float MaxCommitment => MaxCommitmentConfig;
    public float CommitmentPercent => MaxCommitmentConfig > 0 ? commitment / MaxCommitmentConfig : 0f;
    public Room CurrentRoom => currentRoom;
    public Cult Cult => cult;
    
    /// <summary>
    /// Whether this follower is currently assigned to a room.
    /// </summary>
    public bool IsAssigned => currentRoom != null;
    
    // === Initialization ===
    
    /// <summary>
    /// Initialize this follower with a cult reference.
    /// </summary>
    public void Initialize(Cult cult)
    {
        this.cult = cult;
        this.commitment = MaxCommitment;
    }
    
    // === Unity Lifecycle ===
    
    private void Update()
    {
        // Apply commitment decay if in a room that causes it
        if (currentRoom != null && currentRoom.CausesCommitmentDecay)
        {
            DecayCommitment(DecayRatePerSecond * Time.deltaTime);
        }
    }
    
    // === Room Assignment ===
    
    /// <summary>
    /// Set the room this follower is assigned to.
    /// Called by Room.AddFollower() and Room.RemoveFollower().
    /// </summary>
    public void SetRoom(Room room)
    {
        currentRoom = room;
    }
    
    /// <summary>
    /// Set the cult this follower belongs to.
    /// </summary>
    public void SetCult(Cult newCult)
    {
        cult = newCult;
    }
    
    // === Commitment Management ===
    
    /// <summary>
    /// Reduce commitment by the specified amount.
    /// If commitment reaches zero, the follower abandons the cult.
    /// </summary>
    public void DecayCommitment(float amount)
    {
        commitment = Mathf.Max(0f, commitment - amount);
        
        if (commitment <= 0f)
        {
            Abandon();
        }
    }
    
    /// <summary>
    /// Increase commitment by the specified amount (up to max).
    /// </summary>
    public void RecoverCommitment(float amount)
    {
        commitment = Mathf.Min(MaxCommitment, commitment + amount);
    }
    
    /// <summary>
    /// Set commitment to a specific value.
    /// </summary>
    public void SetCommitment(float value)
    {
        commitment = Mathf.Clamp(value, 0f, MaxCommitment);
        
        if (commitment <= 0f)
        {
            Abandon();
        }
    }
    
    // === Abandonment ===

    /// <summary>
    /// Called when commitment reaches zero.
    /// Removes follower from cult and returns them to the Marketplace.
    /// </summary>
    private void Abandon()
    {
        Debug.Log($"Follower abandoned the cult due to zero commitment!");

        // Play abandon sound
        AudioManager.PlayFollowerAbandon();

        // Remove from current room
        if (currentRoom != null)
        {
            currentRoom.RemoveFollower(this);
        }

        // Remove from cult
        if (cult != null)
        {
            cult.RemoveFollower(this);
        }

        // Add to marketplace as neutral citizen
        var marketplace = Marketplace.Instance;
        if (marketplace != null)
        {
            marketplace.AddAbandonedFollower(this);
        }
        else
        {
            // No marketplace - just destroy
            Destroy(gameObject);
        }
    }
}
