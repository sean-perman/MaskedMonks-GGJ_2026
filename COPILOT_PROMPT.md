# Masked Gods - Code Generation Prompt

You are helping build a Unity game called "Masked Gods" - a real-time competitive cult management game for two players using controllers on a shared screen.

## Game Overview

Two players each control a cult. Each cult has a god, a church with rooms, and followers. Players assign followers to rooms to generate resources. Gods automatically attack each other on a timer. Players can activate masks (one-use abilities) to deal damage, buff their own cult, or debuff the enemy.

### Win/Loss Conditions

A player loses if ANY of these reach zero:
- **Followers** (cult has no followers left)
- **Favor** (god's goodwill toward the cult)
- **God Strength** (god's health)

To win, reduce any one of your opponent's three resources to zero.

### Core Loop

1. Followers are assigned to rooms
2. Rooms accumulate progress based on number of followers (1 progress per follower per second)
3. When a room's progress reaches its duration threshold, it triggers an effect and resets
4. Followers lose commitment over time while working (except in Sanctuary and Pews)
5. Followers at zero commitment abandon the cult and return to the neutral Marketplace
6. Gods attack each other automatically every few seconds (damage based on Strength)
7. Players activate masks by spending Favor to trigger powerful effects

### Room Types

- **Sanctuary**: Followers recover commitment. No decay. Hub for reassigning followers.
- **Altar**: Generates God Strength when triggered. Followers decay commitment.
- **Pews**: Generates Favor when triggered. Followers do NOT decay commitment.
- **Mission**: Recruits a citizen from the Marketplace when triggered. Followers decay commitment.
- **Ritual Hall**: Generates a new Mask when triggered. Followers decay commitment.

### Masks

Masks are one-use abilities. Gods can wear one mask at a time (active effect) and store additional masks. Masks have:
- A type (determines effect)
- A duration (how long the effect lasts, or instant)
- A shelf life (how long before it decays in storage)
- A cost (Favor required, and optionally a follower sacrifice)

Masks can target: nothing (global), an enemy room, or the player's own room.

---

## Class Specifications

Generate Unity C# scripts for the following classes. Use the exact field names and method signatures specified. Add appropriate Unity attributes, serialization, and documentation comments.

### Cult

The top-level container for one player's game state.

**Fields:**
- `God` : God - The deity this cult worships
- `Church` : Church - The building containing rooms
- `Followers` : List<Follower> - All followers belonging to this cult

**Methods:**
- `AddFollower()` - Add a new follower to the cult
- `RemoveFollower()` - Remove a follower from the cult (when they die or abandon)

---

### God

The floating deity above each church. Handles combat and mask management.

**Fields:**
- `Strength` : int - Health and attack power (higher = more damage dealt)
- `Mask` : Mask - Currently worn mask (can be null)
- `Masks` : List<Mask> - Masks in storage, available to equip
- `Favor` : int - Resource spent to activate masks; loss condition if zero

**Methods:**
- `SetMask()` - Equip a mask from storage (replaces current mask)
- `IncreaseStrength()` - Add to god's strength (from Altar)
- `DecreaseStrength()` - Reduce god's strength (from enemy attacks/masks)

---

### Church

Container for the room layout.

**Fields:**
- `Rooms` : List<Room> - All rooms in this church

**Methods:**
- `AddRoom()` - Add a room to the church (used during setup)

---

### Room

A location where followers work. Base class - consider making abstract with subclasses per room type.

**Fields:**
- `Level` : int - Upgrade tier (affects capacity and effect strength)
- `Damage` : int - Current damage level (reduces effective capacity)
- `Type` : RoomType (struct/enum) - What kind of room this is
- `Followers` : List<Follower> - Followers currently assigned to this room
- `Location` : Tuple<int, int> or Vector2Int - Grid position in the church
- `Duration` : float - Clock threshold (pawn-seconds needed to trigger effect)

**Methods:**
- `TakeDamage()` - Increase damage level (from enemy mask attacks)
- `IncreaseLevel()` - Upgrade the room
- `AddFollower()` - Assign a follower to this room
- `RemoveFollower()` - Remove a follower from this room
- `SetDuration()` - Modify the clock threshold
- `ReduceDamage()` - Repair damage to the room

**Additional logic needed:**
- Clock accumulation: each Update, add `assignedFollowers.Count * Time.deltaTime` to internal clock
- When clock >= Duration, trigger room effect and reset clock
- Capacity = Level - Damage (cannot add followers beyond capacity)

---

### Follower

A single cultist that can be assigned to rooms.

**Fields:**
- `Commitment` : int - Loyalty level (0-100). At zero, follower abandons cult.

**Methods:**
- `DecayLevel()` - Reduce commitment (called each frame when working in most rooms)
- `IncreaseLevel()` - Increase commitment (called when in Sanctuary)

**Additional logic needed:**
- Reference to current Room assignment
- Reference to owning Cult
- When Commitment reaches 0, trigger abandonment (remove from cult, add to Marketplace)

---

### Mask

A one-use ability that can be equipped by a god.

**Fields:**
- `Type` : MaskType (struct/enum) - Determines what effect this mask has
- `Duration` : float - How long the effect lasts when worn (0 = instant)
- `ShelfLife` : float - Time remaining before this mask decays in storage
- `Cost` : Tuple<int, int> or (int favor, int sacrifice) - Resources required to activate

**Additional logic needed:**
- Method to check if player can afford this mask
- Method to apply the mask's effect based on Type
- Shelf life should tick down while in storage; at zero, mask is destroyed

---

## Enums/Structs Needed

### RoomType
```csharp
public enum RoomType
{
    Sanctuary,
    Altar,
    Pews,
    Mission,
    RitualHall
}
```

### MaskType
```csharp
public enum MaskType
{
    Smiting,      // Damage followers in target enemy room
    Wrath,        // Direct damage to enemy god strength
    Whispers,     // Reduce commitment in target enemy room
    Sanctuary,    // Boost commitment in target own room
    Plenty,       // Instant favor gain
    Sacrifice     // Sacrifice follower to heal god strength
}
```

### MaskTargetType
```csharp
public enum MaskTargetType
{
    None,         // Global effect, no targeting needed
    EnemyRoom,    // Must select an enemy room
    OwnRoom       // Must select one of your own rooms
}
```

---

## Additional Systems Needed

### GameManager
Singleton that owns game state, runs the update loop, and checks win/loss conditions.
- References to both Cult instances
- Reference to Marketplace
- Game timer
- Win/loss detection each frame

### Marketplace
Central area where neutral citizens spawn.
- List of available citizens
- Spawn timer (one citizen every ~10 seconds)
- Max capacity (spawning pauses when full)
- Method to remove a citizen (when recruited)
- Abandoned followers return here

### InputHandler
Reads controller input for one player and translates to game actions.
- Left stick: navigate own church rooms
- Right stick: target enemy rooms (during mask targeting)
- A button: select/confirm
- B button: cancel
- Plus button: assign follower from Sanctuary
- D-pad: activate mask slots

---

## File Structure

```
Assets/Scripts/
├── Core/
│   ├── Cult.cs
│   ├── God.cs
│   ├── Church.cs
│   ├── Room.cs
│   ├── Follower.cs
│   └── Mask.cs
├── Enums/
│   ├── RoomType.cs
│   └── MaskType.cs
├── Rooms/
│   ├── SanctuaryRoom.cs
│   ├── AltarRoom.cs
│   ├── PewsRoom.cs
│   ├── MissionRoom.cs
│   └── RitualHallRoom.cs
├── Managers/
│   ├── GameManager.cs
│   └── Marketplace.cs
└── Input/
    └── InputHandler.cs
```

---

## Implementation Notes

1. Use `[SerializeField]` for private fields that need inspector access
2. Use ScriptableObjects for room/mask data definitions if time permits
3. Room subclasses should override a virtual `OnClockTrigger()` method
4. Follower commitment decay rate: ~1 per second while working
5. Sanctuary recovery rate: ~2 per second
6. God attack interval: ~5 seconds
7. God attack damage: `Strength / 10` (minimum 1)
8. Starting values: 5 followers, 50 favor, 100 god strength

Generate clean, well-documented C# code following Unity conventions. Include TODO comments for areas that need game-specific tuning or additional implementation.
