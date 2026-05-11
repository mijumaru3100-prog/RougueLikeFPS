using UnityEngine;
[CreateAssetMenu(menuName = "Passives/SpinUp")]
public class SpinUpPassive : PassiveEffect 
{
    public PlayerStats buffStats;
    public float heatPerShot = 0f;
    public float maxHeat = 100f;
    public override float GetFireRateMultiplier(PlayerManager manager) 
    {
        float heat = manager.currentWeapon.currentHeat;
        heat = Mathf.Clamp(heat, 0f, maxHeat);
        return 1f + (heat * buffStats.fireRateMultiple);
    }
    public override float GetDamageMultiplier(PlayerManager manager) 
    {
        float heat = manager.currentWeapon.currentHeat;
        heat = Mathf.Clamp(heat, 0f, maxHeat);
        return 1f + (heat * buffStats.damageMultiple);
    }
    public override float GetReloadSpeedMultiplier(PlayerManager manager) 
    {
        float heat = manager.currentWeapon.currentHeat;
        heat = Mathf.Clamp(heat, 0f, maxHeat);
        return 1f + (heat * buffStats.reloadSpeedMultiple);
    }
    public override float GetRecoilMultiplier(PlayerManager manager) 
    {
        float heat = manager.currentWeapon.currentHeat;
        heat = Mathf.Clamp(heat, 0f, maxHeat);
        return 1f + (heat * buffStats.recoilForceMultiple);
    }
    public override void OnShot(PlayerManager manager) 
    {
        manager.currentWeapon.currentHeat += heatPerShot;   
    }
}
