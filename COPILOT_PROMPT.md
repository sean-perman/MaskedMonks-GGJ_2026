# Masked Gods - Game Documentation

## Current Implementation Status (January 2026)

### ✅ COMPLETED SYSTEMS
- **Core Classes**: Cult, God, Church, Room, Follower, Mask - all implemented
- **Room Types**: Sanctuary, Altar, Pews, Mission, RitualHall, Workshop - all with clock/trigger logic
- **Game Loop**: Win/loss detection, god combat timer, commitment decay
- **Input System**: Keyboard controls for 2 players, rebindable via in-game menu
- **Visual Systems**: Room visuals, cursor/targeting, debug dashboard
- **Marketplace**: Citizen spawning, abandoned follower return

### 🔄 IN PROGRESS
- Controller support (currently keyboard only)
- Room building with Architecture masks

### ❌ NOT YET IMPLEMENTED
- Audio system / Sound effects
- Visual polish / Animations
- Main menu / Game over screens
- Save/load system

---

## Game Overview

Two players each control a cult. Each cult has a god, a church with rooms, and followers. Players assign followers to rooms to generate resources. Gods automatically attack each other on a timer. Players can activate masks (one-use abilities) to deal damage, buff their own cult, or debuff the enemy.

### Win/Loss Conditions

A player loses if ANY of these reach zero:
- **Followers** (cult has no followers left)
- **Favor** (god's goodwill toward the cult)
- **God Strength** (god's health)

### Core Loop

1. Followers are assigned to rooms
2. Rooms accumulate progress based on number of followers (1 progress per follower per second)
3. When a room's progress reaches its duration threshold, it triggers an effect and resets
4. Followers lose commitment over time while working (except in Sanctuary and Pews)
5. Followers at zero commitment abandon the cult and return to the neutral Marketplace
6. Gods attack each other automatically every 5 seconds (damage = Strength / 10)
7. Players activate masks by spending Favor to trigger powerful effects

---

## Prefab System

### Overview
All game objects should use prefabs where possible. GameInitializer has prefab reference fields for every object type. If a prefab is assigned, it will be instantiated; otherwise, the system falls back to programmatic creation.

### Required Prefabs (Assets/Prefabs/)

| Prefab | Components Required | Notes |
|--------|---------------------|-------|
| `Cult.prefab` | Cult | Root container for player |
| `God.prefab` | God, SpriteRenderer | Deity visual + logic |
| `Church.prefab` | Church | Room container |
| `Follower.prefab` | Follower, SpriteRenderer | Cultist visual + logic |
| `SanctuaryRoom.prefab` | SanctuaryRoom, RoomVisual, SpriteRenderer | Recovery room |
| `AltarRoom.prefab` | AltarRoom, RoomVisual, SpriteRenderer | Strength generation |
| `PewsRoom.prefab` | PewsRoom, RoomVisual, SpriteRenderer | Favor generation |
| `MissionRoom.prefab` | MissionRoom, RoomVisual, SpriteRenderer | Recruitment |
| `RitualHallRoom.prefab` | RitualHallRoom, RoomVisual, SpriteRenderer | Mask generation |
| `WorkshopRoom.prefab` | WorkshopRoom, RoomVisual, SpriteRenderer | Money generation |
| `EmptySlot.prefab` | RoomSlot, SpriteRenderer | Buildable slot visual |
| `Marketplace.prefab` | Marketplace, SpriteRenderer | Citizen pool |
| `Background.prefab` | SpriteRenderer | Background visual |
| `Cursor.prefab` | CursorVisual, SpriteRenderer | Selection highlight |
| `PlayerController.prefab` | PlayerController | Input handler |
| `ControlsMenu.prefab` | ControlsMenu | Rebinding UI |

### Creating New Objects at Runtime

**IMPORTANT**: When creating new objects at runtime (e.g., spawning followers, building rooms), always:
1. Check if a prefab reference exists
2. Use `Instantiate(prefab)` if available
3. Fall back to `new GameObject()` only if no prefab

Example pattern:
```csharp
GameObject obj;
if (prefab != null)
{
    obj = Instantiate(prefab);
    // Ensure required component exists
    var component = obj.GetComponent<MyComponent>() ?? obj.AddComponent<MyComponent>();
}
else
{
    obj = new GameObject("MyObject");
    obj.AddComponent<MyComponent>();
}
```

### GameInitializer Public Methods for Runtime Creation

| Method | Returns | Use Case |
|--------|---------|----------|
| `CreateFollower(Cult, string)` | Follower | Spawning new followers |
| `GetRoomPrefab(RoomType)` | GameObject | Getting room prefab for building |

---

## File Structure

```
Assets/
├── Prefabs/              - **CREATE PREFABS HERE**
│   ├── Core/
│   │   ├── Cult.prefab
│   │   ├── God.prefab
│   │   ├── Church.prefab
│   │   └── Follower.prefab
│   ├── Rooms/
│   │   ├── SanctuaryRoom.prefab
│   │   ├── AltarRoom.prefab
│   │   ├── PewsRoom.prefab
│   │   ├── MissionRoom.prefab
│   │   ├── RitualHallRoom.prefab
│   │   ├── WorkshopRoom.prefab
│   │   └── EmptySlot.prefab
│   ├── UI/
│   │   ├── Cursor.prefab
│   │   ├── ControlsMenu.prefab
│   │   └── Background.prefab
│   └── World/
│       └── Marketplace.prefab
└── Scripts/
    ├── Core/
    │   ├── Cult.cs           - Top-level player container
    │   ├── God.cs            - Deity with strength/favor/masks
    │   ├── Church.cs         - Room grid container (3x4)
    │   ├── Room.cs           - Abstract base for all rooms
    │   ├── Follower.cs       - Cultist with commitment
    │   ├── Mask.cs           - One-use ability
    │   └── RoomSlot.cs       - Empty buildable slot
    ├── Rooms/
    │   ├── SanctuaryRoom.cs  - Commitment recovery
    │   ├── AltarRoom.cs      - God strength generation
    │   ├── PewsRoom.cs       - Favor generation
    │   ├── MissionRoom.cs    - Citizen recruitment
    │   ├── RitualHallRoom.cs - Mask generation
    │   └── WorkshopRoom.cs   - Money generation
    ├── Enums/
    │   ├── RoomType.cs
    │   ├── MaskType.cs
    │   └── MaskTargetType.cs
    ├── Managers/
    │   ├── GameManager.cs    - Singleton, game loop
    │   ├── GameInitializer.cs - Scene setup, prefab instantiation
    │   ├── Marketplace.cs    - Neutral citizen pool
    │   └── GameActions.cs    - Static utility methods
    ├── Input/
    │   ├── PlayerController.cs - Per-player input handling
    │   └── InputHandler.cs     - (Legacy) controller input
    ├── Visuals/
    │   ├── RoomVisual.cs     - Room display component
    │   └── CursorVisual.cs   - Selection highlight
    ├── UI/
    │   └── ControlsMenu.cs   - Rebindable controls menu
    ├── Debug/
    │   ├── DebugDashboard.cs - IMGUI overlay (F12)
    │   ├── DebugConsole.cs   - Log display (`)
    │   └── ...StatusDisplay.cs files
    └── Testing/
        └── GameTester.cs     - F-key test functions
```

---

## Complete API Reference

### Cult (MonoBehaviour)
Container for one player's game state.

| Property | Type | Description |
|----------|------|-------------|
| `god` | God | The deity (public field) |
| `church` | Church | The building (public field) |
| `Followers` | IReadOnlyList<Follower> | All followers |
| `FollowerCount` | int | Number of followers |
| `Money` | float | Currency resource |

| Method | Returns | Description |
|--------|---------|-------------|
| `AddFollower(Follower)` | void | Add follower to cult |
| `RemoveFollower(Follower)` | void | Remove follower |
| `GetFollowersInSanctuary()` | List<Follower> | Followers in sanctuary |
| `AddMoney(float)` | void | Add currency |
| `SpendMoney(float)` | bool | Spend currency (returns success) |

---

### God (MonoBehaviour)
The floating deity with combat and mask management.

| Property | Type | Description |
|----------|------|-------------|
| `Strength` | int | Health and attack power |
| `MaxStrength` | int | Maximum strength |
| `Favor` | int | Mask activation resource |
| `MaxFavor` | int | Maximum favor |
| `CurrentMask` | Mask | Currently worn mask |
| `Masks` / `StoredMasks` | IReadOnlyList<Mask> | Masks in storage (max 4) |
| `MaskStorageRemaining` | int | Empty mask slots |

| Method | Returns | Description |
|--------|---------|-------------|
| `Initialize(int strength, int favor)` | void | Set starting stats |
| `IncreaseStrength(int)` | void | Heal god |
| `DecreaseStrength(int)` | void | Damage god |
| `IncreaseFavor(int)` | void | Add favor |
| `DecreaseFavor(int)` | void | Spend/lose favor |
| `CanAffordFavor(int)` | bool | Check if can afford cost |
| `AddMaskToStorage(Mask)` | bool | Store a mask |
| `SetMask(int index)` | bool | Equip mask from storage |
| `SetMask(Mask)` | void | Equip mask directly |
| `RemoveMaskFromStorage(int)` | bool | Remove by index |
| `RemoveMaskFromStorage(Mask)` | bool | Remove by reference |
| `ClearMask()` | void | Unequip current mask |
| `SetBleed(float dps)` | void | Apply damage over time |
| `SetRegen(float hps)` | void | Apply healing over time |

---

### Church (MonoBehaviour)
Container for the 3x4 room grid.

| Property | Type | Description |
|----------|------|-------------|
| `GridWidth` | int | 3 columns |
| `GridHeight` | int | 4 rows |
| `Rooms` | IReadOnlyList<Room> | All built rooms |

| Method | Returns | Description |
|--------|---------|-------------|
| `Initialize(Cult)` | void | Link to owning cult |
| `AddRoom(Room, Vector2Int)` | bool | Place room at position |
| `RemoveRoom(Room)` | bool | Remove a room |
| `GetRoomAt(Vector2Int)` | Room | Get room at grid position |
| `GetRoomAt(int x, int y)` | Room | Get room at coordinates |
| `IsValidPosition(Vector2Int)` | bool | Check if position in bounds |
| `GetEmptyPositions()` | List<Vector2Int> | Get buildable positions |
| `GetRoomOfType(RoomType)` | Room | Find first room of type |
| `GetRoomsOfType(RoomType)` | List<Room> | Find all rooms of type |

---

### Room (MonoBehaviour, Abstract)
Base class for all room types.

| Property | Type | Description |
|----------|------|-------------|
| `Type` | RoomType | Room type enum |
| `Location` | Vector2Int | Grid position |
| `Level` | int | Upgrade tier (affects capacity) |
| `Damage` | int | Current damage |
| `Duration` | float | Seconds to trigger effect |
| `Clock` | float | Current progress |
| `Progress` | float | Clock/Duration (0-1) |
| `Followers` | IReadOnlyList<Follower> | Assigned followers |
| `Capacity` | int | Level - Damage (min 0) |
| `HasSpace` | bool | Can accept more followers |
| `CausesCommitmentDecay` | bool | Virtual, override in subclass |

| Method | Returns | Description |
|--------|---------|-------------|
| `Initialize(Church, Cult, Vector2Int)` | void | Setup references |
| `AddFollower(Follower)` | bool | Assign follower |
| `RemoveFollower(Follower)` | bool | Unassign follower |
| `TakeDamage(int)` | void | Apply damage |
| `RepairDamage(int)` | void | Repair damage |
| `IncreaseLevel(int)` | void | Upgrade level |
| `Upgrade()` | void | Shortcut for IncreaseLevel(1) |
| `SetDuration(float)` | void | Change clock threshold |
| `OnClockTrigger()` | void | **Abstract** - effect logic |

**Room Subclasses:**
- `SanctuaryRoom` - `CausesCommitmentDecay = false`, recovers commitment
- `AltarRoom` - Triggers `god.IncreaseStrength()`
- `PewsRoom` - `CausesCommitmentDecay = false`, triggers `god.IncreaseFavor()`
- `MissionRoom` - Recruits citizen from Marketplace
- `RitualHallRoom` - Generates random Mask
- `WorkshopRoom` - Generates Money

---

### Follower (MonoBehaviour)
A single cultist with loyalty tracking.

| Property | Type | Description |
|----------|------|-------------|
| `Commitment` | float | 0-100 loyalty |
| `MaxCommitment` | float | Maximum (100) |
| `CommitmentPercent` | float | 0-1 ratio |
| `CurrentRoom` | Room | Assigned room |
| `CurrentCult` | Cult | Owning cult |
| `IsAssigned` | bool | Has a room |

| Method | Returns | Description |
|--------|---------|-------------|
| `Initialize(Cult)` | void | Setup with cult |
| `SetRoom(Room)` | void | Change room assignment |
| `SetCult(Cult)` | void | Change cult ownership |
| `DecayCommitment(float)` | void | Reduce commitment |
| `RecoverCommitment(float)` | void | Increase commitment |
| `SetCommitment(float)` | void | Set exact value |

---

### Mask (Serializable Class)
One-use ability with targeting and effects.

| Property | Type | Description |
|----------|------|-------------|
| `Type` | MaskType | Effect type |
| `TargetType` | MaskTargetType | Targeting mode |
| `Duration` | float | Effect duration (0 = instant) |
| `ShelfLife` | float | Time until decay |
| `MaxShelfLife` | float | Original shelf life |
| `ShelfLifePercent` | float | Remaining % |
| `FavorCost` | int | Favor to activate |
| `MoneyCost` | int | Money to activate |
| `FollowerSacrifice` | int | Followers to sacrifice |
| `EffectValue` | int | Damage/healing amount |
| `IsExpired` | bool | ShelfLife <= 0 |
| `IsInstant` | bool | Duration <= 0 |

| Method | Returns | Description |
|--------|---------|-------------|
| `Constructor(...)` | Mask | Create with all params |
| `TickShelfLife(float)` | void | Reduce shelf life |
| `CanAfford(Cult)` | bool | Check if can pay cost |
| `PayCost(Cult)` | void | Deduct cost |
| `ApplyEffect(Cult, Room, God)` | void | Execute effect |

---

### GameManager (MonoBehaviour, Singleton)
Central game loop controller.

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | GameManager | Singleton |
| `Cult1` | Cult | Player 1's cult |
| `Cult2` | Cult | Player 2's cult |
| `IsGameRunning` | bool | Game active |
| `GameTime` | float | Elapsed seconds |

| Method | Returns | Description |
|--------|---------|-------------|
| `SetCults(Cult, Cult)` | void | Register cults |
| `StartGame()` | void | Begin game |
| `EndGame(Cult winner, Cult loser, string)` | void | End with result |
| `GetOpponent(Cult)` | Cult | Get the other cult |

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnCultLost` | Action<Cult> | Cult reached loss condition |
| `OnCultWon` | Action<Cult> | Cult won |
| `OnGameStarted` | Action | Game began |
| `OnGameEnded` | Action | Game finished |

---

### Marketplace (MonoBehaviour, Singleton)
Neutral citizen spawning area.

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | Marketplace | Singleton |
| `Citizens` | IReadOnlyList<Follower> | Available citizens |
| `CitizenCount` | int | Number available |
| `HasCitizens` | bool | Any available |
| `IsFull` | bool | At max capacity (10) |

| Method | Returns | Description |
|--------|---------|-------------|
| `RemoveCitizen(Follower)` | bool | Take citizen (recruitment) |
| `AddAbandonedFollower(Follower)` | void | Return abandoned follower |

---

### PlayerController (MonoBehaviour)
Per-player input handling.

| Property | Type | Description |
|----------|------|-------------|
| `PlayerIndex` | int | 0 or 1 |
| `Cult` | Cult | Controlled cult |
| `CursorPosition` | Vector2Int | Selected grid position |
| `TargetPosition` | Vector2Int | Enemy target position |
| `IsTargeting` | bool | In targeting mode |
| `ActiveMaskSlot` | int | Mask being used (-1 = none) |
| `Bindings` | PlayerInputBindings | Key bindings |

| Method | Returns | Description |
|--------|---------|-------------|
| `Initialize(int index, Cult)` | void | Setup controller |
| `GetSelectedRoom()` | Room | Room at cursor |
| `GetTargetedRoom()` | Room | Enemy room at target |

| Event | Signature |
|-------|-----------|
| `OnCursorMoved` | Action<Vector2Int> |
| `OnRoomSelected` | Action<Room> |
| `OnMaskActivated` | Action<int> |
| `OnTargetConfirmed` | Action<Room> |
| `OnTargetCancelled` | Action |

---

### GameActions (Static Class)
Utility methods for common game actions.

| Method | Description |
|--------|-------------|
| `FixRoom(Room, int)` | Repair room damage |
| `DamageRoom(Room, int)` | Apply room damage |
| `InjureGod(God, int)` | Damage god strength |
| `BleedGod(God, float)` | Apply damage over time |
| `HealGod(God, int)` | Restore god strength |
| `RegenGod(God, float)` | Apply healing over time |
| `LowerFavor(God, int)` | Reduce favor |
| `RaiseFavor(God, int)` | Increase favor |
| `GenerateMoney(Cult, float)` | Add money |
| `DecreaseMoney(Cult, float)` | Spend money |

---

## Enums

### RoomType
```csharp
Sanctuary, Altar, Pews, Mission, RitualHall, Workshop
```

### MaskType
```csharp
Smiting,              // Damage followers in enemy room
Wrath,                // Direct damage to enemy god
Whispers,             // Reduce commitment in enemy room
Sanctuary,            // Boost commitment in own room
Plenty,               // Instant favor gain
Sacrifice,            // Sacrifice follower for god healing
ArchitectSanctuary,   // Build Sanctuary room
ArchitectAltar,       // Build Altar room
ArchitectPews,        // Build Pews room
ArchitectMission,     // Build Mission room
ArchitectRitualHall,  // Build Ritual Hall room
ArchitectWorkshop     // Build Workshop room
```

### MaskTargetType
```csharp
None,           // Global effect
EnemyRoom,      // Target enemy room
OwnRoom,        // Target own room
OwnEmptySlot    // Target empty slot (for building)
```

---

## Default Controls

### Player 1 (Keyboard - Left Side)
| Action | Key |
|--------|-----|
| Cursor Up | W |
| Cursor Down | S |
| Cursor Left | A |
| Cursor Right | D |
| Send to Sanctuary | Z |
| Send from Sanctuary | X |
| Upgrade Room | Q |
| Use Mask 1-4 | 1, 2, 3, 4 |
| Confirm Target | ` (Backtick) |
| Cancel Target | Left Ctrl |

### Player 2 (Keyboard - Right Side)
| Action | Key |
|--------|-----|
| Cursor Up | Up Arrow |
| Cursor Down | Down Arrow |
| Cursor Left | Left Arrow |
| Cursor Right | Right Arrow |
| Send to Sanctuary | Comma (,) |
| Send from Sanctuary | Period (.) |
| Upgrade Room | Slash (/) |
| Use Mask 1-4 | Numpad 1-4 |
| Confirm Target | Numpad Enter |
| Cancel Target | Right Ctrl |

### System Keys
| Key | Action |
|-----|--------|
| F12 | Toggle Debug Dashboard |
| ` (Backtick) | Toggle Debug Console |
| Escape | Controls Menu |
| F1-F10 | Test functions (see GameTester) |

---

## Game Constants

| Constant | Value | Location |
|----------|-------|----------|
| God Attack Interval | 5 seconds | GameManager |
| God Attack Damage | Strength / 10 (min 1) | GameManager |
| Starting Followers | 5 | GameInitializer |
| Starting God Strength | 100 | GameInitializer |
| Starting God Favor | 50 | GameInitializer |
| Starting Money | 100 | GameInitializer |
| Max Marketplace Citizens | 10 | Marketplace |
| Starting Marketplace Citizens | 5 | Marketplace |
| Citizen Spawn Interval | 10 seconds | Marketplace |
| Commitment Decay Rate | ~1/sec | Follower.Update() |
| Commitment Recovery Rate | ~2/sec | SanctuaryRoom |
| Church Grid Size | 3 wide x 4 tall | Church |
| Max Masks in Storage | 4 | God |

---

## Sound Effects Needed

### UI Sounds
| Sound | Trigger | Priority |
|-------|---------|----------|
| `ui_cursor_move` | Cursor moves to new room | High |
| `ui_select` | Confirm selection | High |
| `ui_cancel` | Cancel action | High |
| `ui_error` | Invalid action attempted | Medium |
| `ui_menu_open` | Open controls menu | Low |
| `ui_menu_close` | Close controls menu | Low |

### Follower Sounds
| Sound | Trigger | Priority |
|-------|---------|----------|
| `follower_assign` | Follower sent to room | High |
| `follower_unassign` | Follower returned to sanctuary | High |
| `follower_swap` | Follower swapped with sanctuary | Medium |
| `follower_abandon` | Follower leaves cult (0 commitment) | High |
| `follower_recruited` | Citizen converted to follower | High |
| `follower_kicked` | Follower kicked from room due to damage | Medium |
| `follower_working_loop` | Ambient loop when followers in room | Low |

### Room Sounds
| Sound | Trigger | Priority |
|-------|---------|----------|
| `room_trigger_sanctuary` | Sanctuary recovers commitment | Medium |
| `room_trigger_altar` | Altar generates strength | High |
| `room_trigger_pews` | Pews generates favor | High |
| `room_trigger_mission` | Mission recruits citizen | High |
| `room_trigger_ritual` | Ritual hall creates offensive mask | High |
| `room_trigger_workshop` | Workshop generates architecture mask | High |
| `room_trigger_fundraising` | Fundraising generates money | High |
| `room_upgrade` | Room level increased | Medium |
| `room_damaged_orange` | Room takes damage (orange level) | High |
| `room_damaged_red` | Room takes damage (red level - slot lost) | High |
| `room_repaired` | Room damage healed one level | Medium |
| `room_built` | New room constructed from blueprint | High |

### God Sounds
| Sound | Trigger | Priority |
|-------|---------|----------|
| `god_attack` | God deals damage (every 5s) | High |
| `god_hit` | God takes damage | High |
| `god_heal` | God strength restored | Medium |
| `god_favor_gain` | Favor increased | Low |
| `god_favor_spend` | Favor spent on mask | Medium |
| `god_money_gain` | Money increased | Low |
| `god_money_spend` | Money spent (upgrade/build) | Medium |
| `god_low_health` | God strength < 25% | High |
| `god_death` | God strength hits 0 | High |

### Mask Sounds
| Sound | Trigger | Priority |
|-------|---------|----------|
| `mask_generated` | New mask added to storage | High |
| `mask_storage_full` | Mask generation blocked (no space) | Medium |
| `mask_activate_strike` | Strike mask used (room damage) | High |
| `mask_activate_smiting` | Smiting mask used | High |
| `mask_activate_wrath` | Wrath mask used | High |
| `mask_activate_whispers` | Whispers mask used | High |
| `mask_activate_sanctuary` | Sanctuary mask used | Medium |
| `mask_activate_plenty` | Plenty mask used | Medium |
| `mask_activate_sacrifice` | Sacrifice mask used | High |
| `mask_activate_architect` | Architecture mask used (build room) | High |
| `mask_expire` | Mask decays in storage | Low |
| `mask_target_enter` | Enter targeting mode | Medium |
| `mask_target_confirm` | Target selected and mask fired | High |
| `mask_target_cancel` | Targeting cancelled | Low |
| `mask_cannot_afford` | Not enough favor/money for mask | Medium |

### Game State Sounds
| Sound | Trigger | Priority |
|-------|---------|----------|
| `game_start` | Match begins | High |
| `game_win` | Player wins | High |
| `game_lose` | Player loses | High |
| `marketplace_spawn` | New citizen appears | Low |

### Ambient/Music
| Sound | Description | Priority |
|-------|-------------|----------|
| `music_gameplay` | Main gameplay loop | High |
| `music_tension` | Plays when either player low on resources | Medium |
| `ambient_church` | Background church atmosphere | Low |
| `ambient_marketplace` | Marketplace crowd noise | Low |

---

## Testing Checklist

### Quick Test Setup
1. Create empty scene
2. Add empty GameObject named "Game"
3. Add components: `GameManager`, `GameInitializer`, `GameTester`, `DebugDashboard`
4. Press Play

### Test Keys (GameTester.cs)
| Key | Test |
|-----|------|
| F1 | Damage Cult 1 God |
| F2 | Heal Cult 1 God |
| F3 | Damage Cult 2 God |
| F4 | Heal Cult 2 God |
| F5 | Give Cult 1 a random mask |
| F6 | Give Cult 2 a random mask |
| F7 | Spawn follower for Cult 1 |
| F8 | Spawn follower for Cult 2 |
| F9 | Damage random room |
| F10 | Trigger god combat |
| H | Print help to console |
