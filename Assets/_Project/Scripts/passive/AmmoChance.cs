using UnityEngine;

[CreateAssetMenu(menuName = "Passives/AmmoChance")]
public class AmmoChancePassive : PassiveEffect
{
    [Range(0, 1)] public float chance = 0.3f;

    public override void OnShot(PlayerManager manager)
    {
        if (Random.value < chance)
        {
            GunBase gun = manager.currentWeapon;
            gun.currentAmmo++; // 減った分を戻す
            gun.UpdateAmmoDisplay();
            Debug.Log("弾薬節約発動！");
        }
    }
}