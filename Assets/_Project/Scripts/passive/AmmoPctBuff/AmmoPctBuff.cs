using UnityEngine;
[CreateAssetMenu(menuName = "Passives/AmmoPctBuff")]
public class AmmoPctBuff : PassiveEffect
{
    [SerializeField] private float damagebuff;
    [SerializeField] private float fireratebuff;
    [SerializeField][Range(0, 1)] private float buffAmmoThreshold = 0.3f;
    [SerializeField] private bool triggerWhenHigher = false; 
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if (IsConditionMet(manager))
        {
            return 1f + damagebuff;
        }
        return 1f;
    }
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        if (IsConditionMet(manager))
        {
            return 1f + fireratebuff;
        }
        return 1f;
    }
    private bool IsConditionMet(PlayerManager manager)
    {
        if (manager.currentWeapon == null) return false;
        float ammoPercent = (float)manager.currentWeapon.currentAmmo / manager.currentWeapon.maxAmmo;
        if (triggerWhenHigher)
        {
            return ammoPercent >= buffAmmoThreshold;
        }
        else
        {
            return ammoPercent <= buffAmmoThreshold;
        }
    }
}
