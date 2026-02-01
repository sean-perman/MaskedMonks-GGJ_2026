using UnityEngine;
using FMODUnity;

/// <summary>
/// Centralized audio manager for playing FMOD events.
/// Provides static methods for playing sound effects from anywhere in the codebase.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // === UI Events ===
    [Header("UI Events")]
    [SerializeField] private EventReference uiError;
    [SerializeField] private EventReference uiCursorMove;
    [SerializeField] private EventReference uiSelect;
    [SerializeField] private EventReference uiCancel;
    [SerializeField] private EventReference uiMenuOpen;
    [SerializeField] private EventReference uiMenuClose;

    // === Follower Events ===
    [Header("Follower Events")]
    [SerializeField] private EventReference followerAbandon;
    [SerializeField] private EventReference followerRecruited;
    [SerializeField] private EventReference followerAssign;
    [SerializeField] private EventReference followerUnassign;
    [SerializeField] private EventReference followerSacrifice;
    [SerializeField] private EventReference followerWorkingLoop;

    // === God Events ===
    [Header("God Events")]
    [SerializeField] private EventReference godAttack;
    [SerializeField] private EventReference godDeath;
    [SerializeField] private EventReference godFavorGain;
    [SerializeField] private EventReference godFavorSpend;
    [SerializeField] private EventReference godHeal;
    [SerializeField] private EventReference godHit;

    // === Room Events ===
    [Header("Room Events")]
    [SerializeField] private EventReference roomBuilt;
    [SerializeField] private EventReference roomDamaged;
    [SerializeField] private EventReference roomRepaired;
    [SerializeField] private EventReference roomUpgrade;
    [SerializeField] private EventReference roomLightning;
    [SerializeField] private EventReference roomShield;
    [SerializeField] private EventReference roomTriggerAltar;
    [SerializeField] private EventReference roomTriggerMission;
    [SerializeField] private EventReference roomTriggerPew;
    [SerializeField] private EventReference roomTriggerRitual;
    [SerializeField] private EventReference roomTriggerSanctuary;
    [SerializeField] private EventReference roomTriggerWorkshop;

    // === Game State Events ===
    [Header("Game State Events")]
    [SerializeField] private EventReference gameStart;
    [SerializeField] private EventReference gameWin;
    [SerializeField] private EventReference gameLose;
    [SerializeField] private EventReference marketplaceSpawn;

    // === Music ===
    [Header("Music")]
    [SerializeField] private EventReference musicGameplay;

    private FMOD.Studio.EventInstance musicInstance;

    // === Unity Lifecycle ===

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeEventReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            StopMusic();
            Instance = null;
        }
    }

    /// <summary>
    /// Initialize event references with FMOD paths.
    /// </summary>
    private void InitializeEventReferences()
    {
        // UI Events
        uiError = RuntimeManager.PathToEventReference("event:/UI/ui_error");
        uiCursorMove = RuntimeManager.PathToEventReference("event:/UI/ui_cursor_move");
        uiSelect = RuntimeManager.PathToEventReference("event:/UI/ui_select");
        uiCancel = RuntimeManager.PathToEventReference("event:/UI/ui_cancel");
        uiMenuOpen = RuntimeManager.PathToEventReference("event:/UI/ui_menu_open");
        uiMenuClose = RuntimeManager.PathToEventReference("event:/UI/ui_menu_close");

        // Follower Events
        followerAbandon = RuntimeManager.PathToEventReference("event:/Followers/follower_abandon");
        followerRecruited = RuntimeManager.PathToEventReference("event:/Followers/follower_recruited");
        followerAssign = RuntimeManager.PathToEventReference("event:/Followers/follower_assign");
        followerUnassign = RuntimeManager.PathToEventReference("event:/Followers/follower_unassign");
        followerSacrifice = RuntimeManager.PathToEventReference("event:/Followers/follower_sacrifice");
        followerWorkingLoop = RuntimeManager.PathToEventReference("event:/Followers/follower_working_loop");

        // God Events
        godAttack = RuntimeManager.PathToEventReference("event:/GodSounds/god_attack");
        godDeath = RuntimeManager.PathToEventReference("event:/GodSounds/god_death");
        godFavorGain = RuntimeManager.PathToEventReference("event:/GodSounds/god_favor_gain");
        godFavorSpend = RuntimeManager.PathToEventReference("event:/GodSounds/god_favor_spend");
        godHeal = RuntimeManager.PathToEventReference("event:/GodSounds/god_heal");
        godHit = RuntimeManager.PathToEventReference("event:/GodSounds/god_hit");

        // Room Events
        roomBuilt = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_built");
        roomDamaged = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_damaged");
        roomRepaired = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_repaired");
        roomUpgrade = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_upgrade");
        roomLightning = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_lightning");
        roomShield = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_shield");
        roomTriggerAltar = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_trigger_altar");
        roomTriggerMission = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_trigger_mission");
        roomTriggerPew = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_trigger_pew");
        roomTriggerRitual = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_trigger_ritual");
        roomTriggerSanctuary = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_trigger_sanctuary");
        roomTriggerWorkshop = RuntimeManager.PathToEventReference("event:/RoomsOrMasks/room_trigger_workshop");

        // Game State Events
        gameStart = RuntimeManager.PathToEventReference("event:/GameState/game_start");
        gameWin = RuntimeManager.PathToEventReference("event:/GameState/game_win");
        gameLose = RuntimeManager.PathToEventReference("event:/GameState/game_lose");
        marketplaceSpawn = RuntimeManager.PathToEventReference("event:/GameState/marketplace_spawn");

        // Music
        musicGameplay = RuntimeManager.PathToEventReference("event:/Music/music_gameplay");
    }

    // === Helper Methods ===

    private void PlayOneShot(EventReference eventRef)
    {
        if (!eventRef.IsNull)
        {
            RuntimeManager.PlayOneShot(eventRef);
        }
    }

    private void PlayOneShotAttached(EventReference eventRef, GameObject target)
    {
        if (!eventRef.IsNull && target != null)
        {
            RuntimeManager.PlayOneShotAttached(eventRef, target);
        }
    }

    // === UI Sound Methods ===

    public static void PlayUIError()
    {
        Instance?.PlayOneShot(Instance.uiError);
    }

    public static void PlayUICursorMove()
    {
        Instance?.PlayOneShot(Instance.uiCursorMove);
    }

    public static void PlayUISelect()
    {
        Instance?.PlayOneShot(Instance.uiSelect);
    }

    public static void PlayUICancel()
    {
        Instance?.PlayOneShot(Instance.uiCancel);
    }

    public static void PlayUIMenuOpen()
    {
        Instance?.PlayOneShot(Instance.uiMenuOpen);
    }

    public static void PlayUIMenuClose()
    {
        Instance?.PlayOneShot(Instance.uiMenuClose);
    }

    // === Follower Sound Methods ===

    public static void PlayFollowerAbandon()
    {
        Instance?.PlayOneShot(Instance.followerAbandon);
    }

    public static void PlayFollowerRecruited()
    {
        Instance?.PlayOneShot(Instance.followerRecruited);
    }

    public static void PlayFollowerAssign()
    {
        Instance?.PlayOneShot(Instance.followerAssign);
    }

    public static void PlayFollowerUnassign()
    {
        Instance?.PlayOneShot(Instance.followerUnassign);
    }

    public static void PlayFollowerSacrifice()
    {
        Instance?.PlayOneShot(Instance.followerSacrifice);
    }

    // === God Sound Methods ===

    public static void PlayGodAttack()
    {
        Instance?.PlayOneShot(Instance.godAttack);
    }

    public static void PlayGodDeath()
    {
        Instance?.PlayOneShot(Instance.godDeath);
    }

    public static void PlayGodFavorGain()
    {
        Instance?.PlayOneShot(Instance.godFavorGain);
    }

    public static void PlayGodFavorSpend()
    {
        Instance?.PlayOneShot(Instance.godFavorSpend);
    }

    public static void PlayGodHeal()
    {
        Instance?.PlayOneShot(Instance.godHeal);
    }

    public static void PlayGodHit()
    {
        Instance?.PlayOneShot(Instance.godHit);
    }

    // === Room Sound Methods ===

    public static void PlayRoomBuilt()
    {
        Instance?.PlayOneShot(Instance.roomBuilt);
    }

    public static void PlayRoomDamaged()
    {
        Instance?.PlayOneShot(Instance.roomDamaged);
    }

    public static void PlayRoomRepaired()
    {
        Instance?.PlayOneShot(Instance.roomRepaired);
    }

    public static void PlayRoomUpgrade()
    {
        Instance?.PlayOneShot(Instance.roomUpgrade);
    }

    public static void PlayRoomLightning()
    {
        Instance?.PlayOneShot(Instance.roomLightning);
    }

    public static void PlayRoomShield()
    {
        Instance?.PlayOneShot(Instance.roomShield);
    }

    public static void PlayRoomTriggerAltar()
    {
        Instance?.PlayOneShot(Instance.roomTriggerAltar);
    }

    public static void PlayRoomTriggerMission()
    {
        Instance?.PlayOneShot(Instance.roomTriggerMission);
    }

    public static void PlayRoomTriggerPew()
    {
        Instance?.PlayOneShot(Instance.roomTriggerPew);
    }

    public static void PlayRoomTriggerRitual()
    {
        Instance?.PlayOneShot(Instance.roomTriggerRitual);
    }

    public static void PlayRoomTriggerSanctuary()
    {
        Instance?.PlayOneShot(Instance.roomTriggerSanctuary);
    }

    public static void PlayRoomTriggerWorkshop()
    {
        Instance?.PlayOneShot(Instance.roomTriggerWorkshop);
    }

    // === Game State Sound Methods ===

    public static void PlayGameStart()
    {
        Instance?.PlayOneShot(Instance.gameStart);
    }

    public static void PlayGameWin()
    {
        Instance?.PlayOneShot(Instance.gameWin);
    }

    public static void PlayGameLose()
    {
        Instance?.PlayOneShot(Instance.gameLose);
    }

    public static void PlayMarketplaceSpawn()
    {
        Instance?.PlayOneShot(Instance.marketplaceSpawn);
    }

    // === Music Methods ===

    public static void StartGameplayMusic()
    {
        if (Instance == null || Instance.musicGameplay.IsNull) return;

        Instance.StopMusic();
        Instance.musicInstance = RuntimeManager.CreateInstance(Instance.musicGameplay);
        Instance.musicInstance.start();
    }

    public static void StopGameplayMusic()
    {
        Instance?.StopMusic();
    }

    private void StopMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }
}
