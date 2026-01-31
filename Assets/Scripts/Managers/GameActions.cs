using UnityEngine;

/// <summary>
/// Static utility class containing action methods that can be called by masks or rooms
/// to update game values.
/// </summary>
public static class GameActions
{
    #region Room Actions

    /// <summary>
    /// Repairs a room by reducing its damage level.
    /// </summary>
    /// <param name="room">The room to repair.</param>
    /// <param name="value">Amount of damage to repair.</param>
    public static void FixRoom(Room room, int value = 1)
    {
        if (room == null)
        {
            Debug.LogWarning("GameActions.FixRoom: room is null!");
            return;
        }
        room.RepairDamage(value);
    }

    /// <summary>
    /// Damages a room by increasing its damage level.
    /// </summary>
    /// <param name="room">The room to damage.</param>
    /// <param name="value">Amount of damage to apply.</param>
    public static void DamageRoom(Room room, int value = 1)
    {
        if (room == null)
        {
            Debug.LogWarning("GameActions.DamageRoom: room is null!");
            return;
        }
        room.TakeDamage(value);
    }

    #endregion

    #region God Health Actions

    /// <summary>
    /// Instantly injures a god by reducing their strength.
    /// </summary>
    /// <param name="god">The god to injure.</param>
    /// <param name="value">Amount of damage to deal.</param>
    public static void InjureGod(God god, int value)
    {
        if (god == null)
        {
            Debug.LogWarning("GameActions.InjureGod: god is null!");
            return;
        }
        god.DecreaseStrength(value);
    }

    /// <summary>
    /// Applies a bleeding effect to a god (damage over time).
    /// </summary>
    /// <param name="god">The god to apply bleed to.</param>
    /// <param name="dps">Damage per second.</param>
    public static void BleedGod(God god, float dps)
    {
        if (god == null)
        {
            Debug.LogWarning("GameActions.BleedGod: god is null!");
            return;
        }
        god.SetBleed(dps);
    }

    /// <summary>
    /// Instantly heals a god by restoring strength.
    /// </summary>
    /// <param name="god">The god to heal.</param>
    /// <param name="value">Amount of strength to restore.</param>
    public static void HealGod(God god, int value)
    {
        if (god == null)
        {
            Debug.LogWarning("GameActions.HealGod: god is null!");
            return;
        }
        god.IncreaseStrength(value);
    }

    /// <summary>
    /// Applies a regeneration effect to a god (healing over time).
    /// </summary>
    /// <param name="god">The god to apply regen to.</param>
    /// <param name="hps">Health per second.</param>
    public static void RegenGod(God god, float hps)
    {
        if (god == null)
        {
            Debug.LogWarning("GameActions.RegenGod: god is null!");
            return;
        }
        god.SetRegen(hps);
    }

    #endregion

    #region God Favor Actions

    /// <summary>
    /// Lowers a god's favor towards the cult.
    /// </summary>
    /// <param name="god">The god whose favor to lower.</param>
    /// <param name="value">Amount to decrease favor by.</param>
    public static void LowerFavor(God god, int value)
    {
        if (god == null)
        {
            Debug.LogWarning("GameActions.LowerFavor: god is null!");
            return;
        }
        god.DecreaseFavor(value);
    }

    /// <summary>
    /// Raises a god's favor towards the cult.
    /// </summary>
    /// <param name="god">The god whose favor to raise.</param>
    /// <param name="value">Amount to increase favor by.</param>
    public static void RaiseFavor(God god, int value)
    {
        if (god == null)
        {
            Debug.LogWarning("GameActions.RaiseFavor: god is null!");
            return;
        }
        god.IncreaseFavor(value);
    }

    #endregion

    #region Cult Money Actions

    /// <summary>
    /// Generates money for a cult.
    /// </summary>
    /// <param name="cult">The cult to give money to.</param>
    /// <param name="value">Amount of money to generate.</param>
    public static void GenerateMoney(Cult cult, float value)
    {
        if (cult == null)
        {
            Debug.LogWarning("GameActions.GenerateMoney: cult is null!");
            return;
        }
        cult.AddMoney(value);
    }

    /// <summary>
    /// Decreases money from a cult.
    /// </summary>
    /// <param name="cult">The cult to take money from.</param>
    /// <param name="value">Amount of money to decrease.</param>
    /// <returns>True if the cult had enough money, false otherwise.</returns>
    public static bool DecreaseMoney(Cult cult, float value)
    {
        if (cult == null)
        {
            Debug.LogWarning("GameActions.DecreaseMoney: cult is null!");
            return false;
        }
        return cult.SpendMoney(value);
    }

    #endregion
}
