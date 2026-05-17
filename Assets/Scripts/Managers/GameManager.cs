using System;
using UnityEngine;

/// <summary>
/// Singleton that owns game state, runs the update loop, and checks win/loss conditions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Cult References")]
    [SerializeField] private Cult cult1;
    [SerializeField] private Cult cult2;
    
    [Header("Game Settings")]
    [SerializeField] private float godAttackInterval = 15f;
    [SerializeField] private int startingFollowers = 5;
    [SerializeField] private int startingFavor = 50;
    [SerializeField] private int startingStrength = 200;
    
    [Header("Game State")]
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private float gameTime = 0f;
    [SerializeField] private float godAttackTimer = 0f;
    
    [Header("UI References")]
    [SerializeField] private GameOverScreen gameOverScreen;
    
    // === Events ===
    
    public event Action<Cult> OnCultLost;
    public event Action<Cult> OnCultWon;
    public event Action OnGameStarted;
    public event Action OnGameEnded;
    
    // === Properties ===
    
    public Cult Cult1 => cult1;
    public Cult Cult2 => cult2;
    public bool IsGameRunning => gameRunning;
    public float GameTime => gameTime;
    
    // === Public Methods ===
    
    /// <summary>
    /// Set the game over screen reference (called by GameInitializer).
    /// </summary>
    public void SetGameOverScreen(GameOverScreen screen)
    {
        gameOverScreen = screen;
    }
    
    /// <summary>
    /// Register cults with the game manager (called by GameInitializer).
    /// </summary>
    public void SetCults(Cult c1, Cult c2)
    {
        cult1 = c1;
        cult2 = c2;
        Debug.Log($"GameManager: Cults registered - {c1?.name}, {c2?.name}");
        
        // Auto-start if both cults are now assigned
        if (cult1 != null && cult2 != null && !gameRunning)
        {
            StartGame();
        }
    }
    
    // === Unity Lifecycle ===
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GameManager instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Auto-start if cults are assigned
        if (cult1 != null && cult2 != null)
        {
            StartGame();
        }
    }
    
    private void Update()
    {
        if (!gameRunning) return;
        
        gameTime += Time.deltaTime;
        
        // God combat timer
        godAttackTimer += Time.deltaTime;
        if (godAttackTimer >= godAttackInterval)
        {
            ProcessGodCombat();
            godAttackTimer = 0f;
        }
        
        CheckFavor();
        // Check win/loss conditions
        CheckWinLossConditions();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // === Game Flow ===
    
    /// <summary>
    /// Start a new game with the assigned cults.
    /// </summary>
    public void StartGame()
    {
        if (cult1 == null || cult2 == null)
        {
            Debug.LogError("Cannot start game: Both cults must be assigned!");
            return;
        }

        Debug.Log("=== GAME STARTED ===");

        gameRunning = true;
        gameTime = 0f;
        godAttackTimer = 0f;

        // Play game start sound and music
        AudioManager.PlayGameStart();
        AudioManager.StartGameplayMusic();

        OnGameStarted?.Invoke();
    }
    
    /// <summary>
    /// End the current game.
    /// </summary>
    public void EndGame(Cult winner, Cult loser, string reason)
    {
        if (!gameRunning) return;

        gameRunning = false;

        Debug.Log($"=== GAME ENDED ===");
        Debug.Log($"Winner: {winner?.name ?? "None"}");
        Debug.Log($"Loser: {loser?.name ?? "None"}");
        Debug.Log($"Reason: {reason}");

        // Stop gameplay music and play end sounds
        AudioManager.StopGameplayMusic();
        AudioManager.PlayGameWin();
        AudioManager.PlayGameLose();

        // Show game over screen
        if (gameOverScreen != null)
        {
            gameOverScreen.Show(winner, loser, reason);
        }

        OnCultLost?.Invoke(loser);
        OnCultWon?.Invoke(winner);
        OnGameEnded?.Invoke();
    }
    
    // === God Combat ===
    
    /// <summary>
    /// Process automatic god attacks.
    /// </summary>
    private void ProcessGodCombat()
    {
        if (cult1?.god == null || cult2?.god == null) return;

        // Calculate damage: Strength / 10, minimum 1
        // int damage1 = Mathf.Max(1, cult1.god.Strength / 10);
        // int damage2 = Mathf.Max(1, cult2.god.Strength / 10);
        int damage1 = 5;
        int damage2 = 5;

        // Play god attack sound
        AudioManager.PlayGodAttack();

        // Get god positions
        Vector3 god1Pos = cult1.god.transform.position;
        Vector3 god2Pos = cult2.god.transform.position;

        // Spawn projectile from god1 to god2 (damage applied on impact)
        MaskProjectile.Create(god1Pos, god2Pos, MaskType.Wrath, () =>
        {
            cult2.god.DecreaseStrength(damage1);
        });

        // Spawn projectile from god2 to god1 (damage applied on impact)
        MaskProjectile.Create(god2Pos, god1Pos, MaskType.Wrath, () =>
        {
            cult1.god.DecreaseStrength(damage2);
        });

        Debug.Log($"God Combat! Cult1 dealt {damage1} damage, Cult2 dealt {damage2} damage.");
    }
    
    /// <summary>
    /// If God Favor is depleted, reduce God Health
    /// Add back single tick of favor
    /// </summary>
    private void CheckFavor()
    {
        if (cult1?.god != null && cult1.god.Favor <= 0)
        {
            cult1.god.DecreaseStrength(30); // Arbitrary damage value when favor is depleted
            Debug.Log("Cult 1's God is losing strength due to depleted favor!");
            cult1.god.IncreaseFavor(1); // Add back a single tick of favor to prevent instant death
        }
        
        if (cult2?.god != null && cult2.god.Favor <= 0)
        {
            cult2.god.DecreaseStrength(30); // Arbitrary damage value when favor is depleted
            Debug.Log("Cult 2's God is losing strength due to depleted favor!");
            cult2.god.IncreaseFavor(1); // Add back a single tick of favor to prevent instant death
        }
    }

    // === Win/Loss Detection ===
    
    /// <summary>
    /// Check if any cult has met a loss condition.
    /// </summary>
    private void CheckWinLossConditions()
    {
        // Check Cult 1 loss conditions
        string cult1Loss = GetLossReason(cult1);
        if (cult1Loss != null)
        {
            EndGame(cult2, cult1, $"Cult 1 lost: {cult1Loss}");
            return;
        }
        
        // Check Cult 2 loss conditions
        string cult2Loss = GetLossReason(cult2);
        if (cult2Loss != null)
        {
            EndGame(cult1, cult2, $"Cult 2 lost: {cult2Loss}");
            return;
        }
    }
    
    /// <summary>
    /// Get the reason a cult has lost, or null if they haven't lost.
    /// </summary>
    private string GetLossReason(Cult cult)
    {
        if (cult == null) return "Cult is null";
        
        // No followers remaining
        if (cult.FollowerCount <= 0)
        {
            return "No followers remaining";
        }
        
        if (cult.god == null) return "God is null";
        
        // God strength depleted
        if (cult.god.Strength <= 0)
        {
            return "God strength depleted";
        }
        
        // God favor depleted
        // if (cult.god.Favor <= 0)
        // {
        //     return "God favor depleted";
        // }
        
        return null; // No loss condition met
    }
    
    // === Utility ===
    
    /// <summary>
    /// Get the opposing cult.
    /// </summary>
    public Cult GetOpponent(Cult cult)
    {
        if (cult == cult1) return cult2;
        if (cult == cult2) return cult1;
        return null;
    }
}
