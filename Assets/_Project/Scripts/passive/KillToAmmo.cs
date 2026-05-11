using UnityEngine;

[CreateAssetMenu(menuName = "Passives/KillToAmmo")]
public class KillToAmmo : PassiveEffect
{
    public int resurveAmmo = 1;

    public override void OnKillEnemy(PlayerManager manager)
    {
        manager.currentWeapon.currentAmmo += resurveAmmo;
        manager.currentWeapon.UpdateAmmoDisplay();
    }
}
