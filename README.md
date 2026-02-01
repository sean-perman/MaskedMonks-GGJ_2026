# Masked Monks

A 2-player local multiplayer real-time strategy game where two rival cults battle for divine supremacy.

## Overview

Each player controls a cult with a floating god above their church. Manage your followers, build rooms, generate resources, and craft powerful masks to attack your opponent while defending your own god from destruction.

## Objective

Destroy the enemy god or cause their cult to collapse. The first cult to meet a loss condition loses the game.

## Loss Conditions

You lose if ANY of these happen:
- **God Strength reaches 0** - Your god is destroyed
- **God Favor reaches 0** - Your god loses divine power
- **No Followers remaining** - Your cult collapses

## Resources

| Resource | Description | How to Gain |
|----------|-------------|-------------|
| **Strength** | God's health | Altar room |
| **Favor** | Powers masks & shields | Pews room |
| **Money** | Build & upgrade rooms | Fundraising room |
| **Followers** | Your workforce | Mission room |

## Rooms

### Core Rooms
- **Sanctuary** - Followers recover commitment here. Your home base.
- **Pews** - Followers pray to generate Favor.
- **Altar** - Followers worship to restore God Strength.

### Economy Rooms
- **Mission** - Recruit new followers from the Marketplace.
- **Fundraising** - Generate Money (costs Favor).
- **Workshop** - Repair damaged rooms.

### Ritual Rooms (Create Masks)
- **Wrath Ritual Hall** - Creates Strike masks (damage one room).
- **Lightning Ritual** - Creates Lightning masks (damage entire column).
- **Flood Ritual** - Creates Flood masks (damage bottom row).
- **Shield Ritual** - Creates Shield masks (auto-block attacks).
- **Sacrificial Altar** - Sacrifice a follower to damage enemy god directly.

## Masks

Masks are special attacks stored on your god (max 5). Use them to attack the enemy or defend yourself.

| Mask | Effect | Favor Cost |
|------|--------|------------|
| Strike | Damage one enemy room | 2 |
| Lightning | Damage all rooms in a column | 3 |
| Flood | Damage all rooms in bottom row | 2 |
| Shield | Auto-blocks one attack | 4 (when triggered) |

**Note:** Masks have a shelf life and will expire if not used!

## Follower Commitment

Followers have a Commitment meter (0-100):
- **Working in rooms** = Commitment decays at 1/second
- **Resting in Sanctuary** = Commitment recovers at 2/second
- **Commitment reaches 0** = Follower abandons your cult!

Keep your followers happy by rotating them through the Sanctuary.

## Controls

### Player 1 (Left Side)
| Action | Key |
|--------|-----|
| Move Cursor | W / A / S / D |
| Send to Sanctuary | Z |
| Send from Sanctuary | X |
| Upgrade Room | Q |
| Use Mask 1-4 | 1 / 2 / 3 / 4 |
| Confirm Target | ` (backtick) |
| Cancel | Left Ctrl |

### Player 2 (Right Side)
| Action | Key |
|--------|-----|
| Move Cursor | Arrow Keys |
| Send to Sanctuary | , (comma) |
| Send from Sanctuary | . (period) |
| Upgrade Room | / (slash) |
| Use Mask 1-4 | K / L / ; / ' |
| Confirm Target | Enter |
| Cancel | Right Ctrl |

### Gamepad
| Action | Button |
|--------|--------|
| Move Cursor | Left Stick |
| Send to Sanctuary | B |
| Send from Sanctuary | A |
| Upgrade Room | Y |
| Use Mask 1-4 | D-Pad |
| Confirm Target | Right Trigger |
| Cancel | B |

## God Combat

Every 15 seconds, both gods automatically attack each other. Damage dealt is based on current Strength (Strength / 10, minimum 1).

**This means you can't just turtle!** Build up your god's strength or your opponent will chip away at you.

## Tips

1. **Rotate followers** - Don't let commitment hit 0 or they leave!
2. **Build Pews early** - You need Favor for everything
3. **Watch the timer** - God attacks happen every 15 seconds
4. **Shield masks are defensive** - They auto-trigger when you're attacked
5. **Upgrade rooms** - Higher level = more follower capacity
6. **Repair with Workshop** - Or place followers in damaged slots to slowly repair

## Credits

Created for Global Game Jam 2026

---

*May your god reign supreme!*
