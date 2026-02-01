using UnityEngine;

/// <summary>
/// Fundraising room - Generates money at the expense of favor.
/// Every 30 seconds of pawn-time: +3 money, -2 favor.
/// Followers decay commitment while working here.
/// </summary>
public class FundraisingRoom : Room
{
    [Header("Fundraising Settings")]
    [Tooltip("Money generated per trigger")]
    [SerializeField] private int moneyPerTrigger = 3;
    
    [Tooltip("Favor cost per trigger")]
    [SerializeField] private int favorCostPerTrigger = 2;
    
    [Tooltip("Duration in pawn-seconds to trigger")]
    [SerializeField] private float triggerDuration = 30f;
    
    public override ResourceType GeneratedResource => ResourceType.Money;
    
    protected override void Awake()
    {
        type = RoomType.Fundraising;
        duration = triggerDuration;
    }
    
    /// <summary>
    /// When the fundraising room triggers, generate money and spend favor.
    /// </summary>
    protected override void OnClockTrigger()
    {
        if (cult == null || cult.god == null)
        {
            Debug.LogWarning("Fundraising room cannot trigger - missing cult/god reference!");
            return;
        }
        
        // Check if we have enough favor
        if (cult.god.Favor < favorCostPerTrigger)
        {
            Debug.Log($"Fundraising failed - not enough favor! Need {favorCostPerTrigger}, have {cult.god.Favor}");
            return;
        }
        
        // Spend favor, gain money
        cult.god.DecreaseFavor(favorCostPerTrigger);
        cult.AddMoney(moneyPerTrigger);
        NotifyResourceGenerated(ResourceType.Money, moneyPerTrigger);
        
        Debug.Log($"Fundraising success! +{moneyPerTrigger} money, -{favorCostPerTrigger} favor. Total money: {cult.Money}");
    }
}
