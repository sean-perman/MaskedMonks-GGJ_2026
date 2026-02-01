using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The top-level container for one player's game state.
/// A cult has a god, a church with rooms, and followers.
/// </summary>
public class Cult : MonoBehaviour
{
    [Header("Core References")]
    public God god;
    public Church church;
    
    [Header("Input")]
    [SerializeField] private PlayerController playerController;
    
    [Header("Followers")]
    [SerializeField] private List<Follower> followers = new();
    
    [Header("Resources")]
    [SerializeField] private int money = 0;
    [SerializeField] private int maxMoney = 10;
    
    // === Properties ===
    
    public IReadOnlyList<Follower> Followers => followers;
    public int FollowerCount => followers.Count;
    public int Money => money;
    public int MaxMoney => maxMoney;
    public PlayerController PlayerController => playerController;
    
    // === Initialization ===
    
    private void Start()
    {
        // Initialize church with reference to this cult
        if (church != null)
        {
            church.Initialize(this);
        }
        
        // Initialize all existing followers
        foreach (var follower in followers)
        {
            if (follower != null)
            {
                follower.Initialize(this);
            }
        }
    }
    
    // === Follower Management ===
    
    /// <summary>
    /// Add a new follower to the cult.
    /// </summary>
    public void AddFollower(Follower follower)
    {
        if (follower == null) return;
        
        if (!followers.Contains(follower))
        {
            followers.Add(follower);
            follower.SetCult(this);
            
            // Optionally place in sanctuary if available
            var sanctuary = church?.GetRoomOfType(RoomType.Sanctuary);
            if (sanctuary != null && sanctuary.HasSpace)
            {
                sanctuary.AddFollower(follower);
            }
        }
    }
    
    /// <summary>
    /// Remove a follower from the cult (when they die or abandon).
    /// </summary>
    public void RemoveFollower(Follower follower)
    {
        if (follower == null) return;
        
        followers.Remove(follower);
        follower.SetCult(null);
        
        // Check for loss condition
        if (followers.Count <= 0)
        {
            OnNoFollowersRemaining();
        }
    }
    
    /// <summary>
    /// Get all followers currently in the sanctuary (unassigned/resting).
    /// </summary>
    public List<Follower> GetFollowersInSanctuary()
    {
        var sanctuary = church?.GetRoomOfType(RoomType.Sanctuary);
        if (sanctuary != null)
        {
            return new List<Follower>(sanctuary.Followers);
        }
        return new List<Follower>();
    }
    
    // === Money Management ===
    
    public void AddMoney(int value)
    {
        money = Mathf.Min(maxMoney, money + Mathf.Max(0, value));
    }

    public bool SpendMoney(int value)
    {
        if (value <= money)
        {
            money -= value;
            return true;
        }
        return false;
    }
    
    public void IncreaseMaxMoney(int amount)
    {
        maxMoney += Mathf.Max(0, amount);
    }
    
    // === Controller ===
    
    /// <summary>
    /// Set the player controller for this cult.
    /// </summary>
    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }
    
    /// <summary>
    /// Check if a room is currently selected by this cult's player.
    /// </summary>
    public bool IsRoomSelected(Room room)
    {
        if (playerController == null || church == null || room == null) return false;
        return church.GetRoomAt(playerController.CursorPosition) == room;
    }
    
    // === Loss Conditions ===
    
    private void OnNoFollowersRemaining()
    {
        Debug.LogWarning($"Cult has no followers remaining! LOSS CONDITION.");
        // TODO: Notify GameManager of loss
    }
}

