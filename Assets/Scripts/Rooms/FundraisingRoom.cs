using UnityEngine;

/// <summary>
/// Fundraising room - Generates money at the expense of favor.
/// Every 30 seconds of pawn-time: +3 money, -2 favor.
/// Followers decay commitment while working here.
/// </summary>
public class FundraisingRoom : Room
{
    private int MoneyPerTrigger => GameConfig.Instance.fundraisingMoneyPerTrigger;
    private int FavorCostPerTrigger => GameConfig.Instance.fundraisingFavorCost;
    
    public override ResourceType GeneratedResource => ResourceType.Money;
    
    protected override void Awake()
    {
        type = RoomType.Fundraising;
        duration = GameConfig.Instance.fundraisingDuration;
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
        if (cult.god.Favor < FavorCostPerTrigger)
        {
            Debug.Log($"Fundraising failed - not enough favor! Need {FavorCostPerTrigger}, have {cult.god.Favor}");
            return;
        }
        
        // Spend favor, gain money
        cult.god.DecreaseFavor(FavorCostPerTrigger);
        cult.AddMoney(MoneyPerTrigger);
        NotifyResourceGenerated(ResourceType.Money, MoneyPerTrigger);
        
        Debug.Log($"Fundraising success! +{MoneyPerTrigger} money, -{FavorCostPerTrigger} favor. Total money: {cult.Money}");
    }
}
