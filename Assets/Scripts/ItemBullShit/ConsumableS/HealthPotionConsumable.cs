using UnityEngine;

[CreateAssetMenu(fileName = "HealthPotion", menuName = "Items/Consumables/HealthPotion")]
public class HealthPotionConsumable : ConsumableSO
{
    public int healAmount = 20;

    protected override void ApplyEffect()
    {
        PlayerRunStats.Instance?.ModifyHealth(healAmount);
    }
}