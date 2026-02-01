using UnityEngine;

/// <summary>
/// Sacrificial Altar room - Sacrifices a follower to deal direct damage to the enemy god.
/// When the progress bar fills, one follower is killed and a projectile is launched
/// at the enemy god, dealing configured damage.
/// </summary>
public class SacrificialAltarRoom : Room
{
    private int DamageToGod => GameConfig.Instance.sacrificialAltarDamage;

    public override ResourceType GeneratedResource => ResourceType.None;

    protected override void Awake()
    {
        type = RoomType.SacrificialAltar;
        duration = GameConfig.Instance.sacrificialAltarDuration;
    }

    /// <summary>
    /// When the sacrificial altar triggers, kill a follower and damage the enemy god.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null) return;

        // Need at least one follower to sacrifice
        if (followers.Count == 0)
        {
            Debug.Log($"Sacrificial Altar triggered but no followers to sacrifice!");
            return;
        }

        // Get the enemy cult
        var enemyCult = GameManager.Instance?.GetOpponent(cult);
        if (enemyCult?.god == null)
        {
            Debug.Log($"Sacrificial Altar triggered but no enemy god to target!");
            return;
        }

        // Sacrifice a follower (remove the last one)
        var sacrificedFollower = followers[followers.Count - 1];
        RemoveFollower(sacrificedFollower);
        cult.RemoveFollower(sacrificedFollower);

        Debug.Log($"Sacrificial Altar: {sacrificedFollower.name} was sacrificed!");

        // Launch projectile at enemy god
        LaunchProjectileAtGod(enemyCult.god, sacrificedFollower);
    }

    /// <summary>
    /// Launch a projectile from this room to the enemy god.
    /// </summary>
    private void LaunchProjectileAtGod(God targetGod, Follower sacrificedFollower)
    {
        // Create projectile
        var projectileObj = new GameObject("SacrificeProjectile");
        var projectile = projectileObj.AddComponent<MaskProjectile>();

        // Calculate positions
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetGod.transform.position;

        // Launch with callback to apply damage on impact
        projectile.Launch(startPos, targetPos, MaskType.Wrath, () =>
        {
            // Deal damage to enemy god
            targetGod.DecreaseStrength(DamageToGod);
            Debug.Log($"Sacrificial Altar dealt {DamageToGod} damage to enemy god!");
        });

        // Destroy the sacrificed follower after launching
        if (sacrificedFollower != null && sacrificedFollower.gameObject != null)
        {
            Destroy(sacrificedFollower.gameObject);
        }
    }
}
