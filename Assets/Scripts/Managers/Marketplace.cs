using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central area where neutral citizens spawn and abandoned followers return.
/// Singleton that both cults can recruit from.
/// </summary>
public class Marketplace : MonoBehaviour
{
    public static Marketplace Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private int maxCapacity = 10;
    [SerializeField] private int startingCitizens = 5;
    [SerializeField] private float spawnInterval = 10f;  // Seconds between spawns
    
    [Header("Citizen Prefab")]
    [SerializeField] private GameObject citizenPrefab;
    
    [Header("Runtime State")]
    [SerializeField] private List<Follower> citizens = new();
    [SerializeField] private float spawnTimer = 0f;
    
    // === Properties ===
    
    public IReadOnlyList<Follower> Citizens => citizens;
    public int CitizenCount => citizens.Count;
    public bool HasCitizens => citizens.Count > 0;
    public bool IsFull => citizens.Count >= maxCapacity;
    
    // === Unity Lifecycle ===
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple Marketplace instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Spawn starting citizens
        for (int i = 0; i < startingCitizens; i++)
        {
            SpawnCitizen();
        }
    }
    
    private void Update()
    {
        // Spawn timer
        if (!IsFull)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnCitizen();
                spawnTimer = 0f;
            }
        }
        else
        {
            // Reset timer when full so we don't get instant spawn when a slot opens
            spawnTimer = 0f;
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // === Citizen Management ===
    
    /// <summary>
    /// Spawn a new citizen in the marketplace.
    /// </summary>
    public Follower SpawnCitizen()
    {
        if (IsFull)
        {
            Debug.Log("Marketplace is full, cannot spawn citizen.");
            return null;
        }
        
        Follower citizen;
        
        if (citizenPrefab != null)
        {
            var go = Instantiate(citizenPrefab, transform);
            citizen = go.GetComponent<Follower>();
            if (citizen == null)
            {
                citizen = go.AddComponent<Follower>();
            }
        }
        else
        {
            // Create a basic follower without prefab
            var go = new GameObject("Citizen");
            go.transform.SetParent(transform);
            citizen = go.AddComponent<Follower>();
        }
        
        // Citizens in marketplace are neutral (no cult)
        citizen.SetCult(null);
        citizens.Add(citizen);

        // Play spawn sound
        AudioManager.PlayMarketplaceSpawn();

        return citizen;
    }
    
    /// <summary>
    /// Recruit a citizen from the marketplace (removes and returns them).
    /// </summary>
    public Follower RecruitCitizen()
    {
        if (!HasCitizens)
        {
            return null;
        }
        
        // Take the first available citizen
        var citizen = citizens[0];
        citizens.RemoveAt(0);
        
        // Reset their commitment to full when recruited
        citizen.SetCommitment(100f);
        
        return citizen;
    }
    
    /// <summary>
    /// Remove a specific citizen from the marketplace.
    /// </summary>
    public bool RemoveCitizen(Follower citizen)
    {
        return citizens.Remove(citizen);
    }
    
    /// <summary>
    /// Add an abandoned follower back to the marketplace as a neutral citizen.
    /// </summary>
    public void AddAbandonedFollower(Follower follower)
    {
        if (follower == null) return;
        
        if (IsFull)
        {
            Debug.Log("Marketplace is full, destroying abandoned follower.");
            Destroy(follower.gameObject);
            return;
        }
        
        // Clear cult reference and reset commitment
        follower.SetCult(null);
        follower.SetCommitment(100f);  // Fresh start
        follower.transform.SetParent(transform);
        
        citizens.Add(follower);
        Debug.Log("Abandoned follower returned to marketplace.");
    }
}
