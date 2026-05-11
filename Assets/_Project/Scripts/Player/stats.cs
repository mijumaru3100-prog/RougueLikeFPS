using UnityEngine;

[CreateAssetMenu(menuName = "stats")]
public class PlayerStats : ScriptableObject
{
    public int bonusMaxAmmo =0;
    public float maxAmmoMultiple = 1f;
    public float fireRateMultiple= 1f;
    public float recoilForceMultiple= 1f;
    public float adsMagnificationMultiple = 1f;
    public float reloadSpeedMultiple = 1f;
    public float bonusWeakPointMultiple  =0f;
    public int bonusMaxHP = 0;
    public float maxHPMultiple = 1f;
    public int bonusDamage = 0;
    public float damageMultiple = 1f;
    [Header("prefabShot用")]
    public float bonusBulletSpeed = 0;
    public float bulletSpeedMultiple = 1f;
    public float bonusLifeTime = 0;
    public float lifeTimeMultiple = 1f;
    public int bonusThrouthCount = 0;
    public float throuthCountMultiple = 1f;

    /// <summary>
    /// ゲーム開始時に呼び出して、ステータスをすべて初期設定に戻します。
    /// ScriptableObject はプレイモード終了後も変更が残ってしまうため、このリセットが必要です。
    /// </summary>
    public void ResetToDefault()
    {
        bonusMaxAmmo = 0;
        maxAmmoMultiple = 1f;
        fireRateMultiple = 1f;
        recoilForceMultiple = 1f;
        adsMagnificationMultiple = 1f;
        reloadSpeedMultiple = 1f;
        bonusWeakPointMultiple = 0f;
        bonusMaxHP = 0;
        maxHPMultiple = 1f;
        bonusDamage = 0;
        damageMultiple = 1f;
        bonusBulletSpeed = 0;
        bulletSpeedMultiple = 1f;
        bonusLifeTime = 0;
        lifeTimeMultiple = 1f;
        bonusThrouthCount = 0;
        throuthCountMultiple = 1f;
    }

    /// <summary>
    /// パッシブスキル等のステータス補正を適用・除外するための関数。
    /// isAdditive が true の場合は加減算、false の場合は乗除算を行います。
    /// isApplying が true の場合は適用（バフ）、false の場合は解除（デバフ/除去）を行います。
    /// </summary>
    public void ApplyModifier(PlayerStats modifier, bool isApplying, bool isAdditive)
    {
        if (modifier == null) return;
        
        int sign = isApplying ? 1 : -1;
        
        // 加算・減算プロパティ
        if(isAdditive)
        {
            bonusMaxAmmo += modifier.bonusMaxAmmo * sign;
            bonusMaxHP += modifier.bonusMaxHP * sign;
            bonusDamage += modifier.bonusDamage * sign;
            bonusBulletSpeed += modifier.bonusBulletSpeed * sign;
            bonusLifeTime += modifier.bonusLifeTime * sign;
            bonusThrouthCount += modifier.bonusThrouthCount * sign;
            bonusWeakPointMultiple += modifier.bonusWeakPointMultiple * sign;

            // Multiple系は 0 を基準とした値を加算する
            maxAmmoMultiple += modifier.maxAmmoMultiple * sign;
            fireRateMultiple += modifier.fireRateMultiple * sign;
            recoilForceMultiple += modifier.recoilForceMultiple * sign;
            adsMagnificationMultiple += modifier.adsMagnificationMultiple * sign;
            reloadSpeedMultiple += modifier.reloadSpeedMultiple * sign;
            maxHPMultiple += modifier.maxHPMultiple * sign;
            damageMultiple += modifier.damageMultiple * sign;
            bulletSpeedMultiple += modifier.bulletSpeedMultiple * sign;
            lifeTimeMultiple += modifier.lifeTimeMultiple * sign;
            throuthCountMultiple += modifier.throuthCountMultiple * sign;
        }
        // 乗算・除算プロパティ
        else if (isApplying)
        {
            maxAmmoMultiple *= modifier.maxAmmoMultiple;
            fireRateMultiple *= modifier.fireRateMultiple;
            recoilForceMultiple *= modifier.recoilForceMultiple;
            adsMagnificationMultiple *= modifier.adsMagnificationMultiple;
            reloadSpeedMultiple *= modifier.reloadSpeedMultiple;
            maxHPMultiple *= modifier.maxHPMultiple;
            damageMultiple *= modifier.damageMultiple;
            bulletSpeedMultiple *= modifier.bulletSpeedMultiple;
            lifeTimeMultiple *= modifier.lifeTimeMultiple;
            throuthCountMultiple *= modifier.throuthCountMultiple;
        }
        else
        {
            // 0除算防止
            if (modifier.maxAmmoMultiple != 0) maxAmmoMultiple /= modifier.maxAmmoMultiple;
            if (modifier.fireRateMultiple != 0) fireRateMultiple /= modifier.fireRateMultiple;
            if (modifier.recoilForceMultiple != 0) recoilForceMultiple /= modifier.recoilForceMultiple;
            if (modifier.adsMagnificationMultiple != 0) adsMagnificationMultiple /= modifier.adsMagnificationMultiple;
            if (modifier.reloadSpeedMultiple != 0) reloadSpeedMultiple /= modifier.reloadSpeedMultiple;
            if (modifier.maxHPMultiple != 0) maxHPMultiple /= modifier.maxHPMultiple;
            if (modifier.damageMultiple != 0) damageMultiple /= modifier.damageMultiple;
            if (modifier.bulletSpeedMultiple != 0) bulletSpeedMultiple /= modifier.bulletSpeedMultiple;
            if (modifier.lifeTimeMultiple != 0) lifeTimeMultiple /= modifier.lifeTimeMultiple;
            if (modifier.throuthCountMultiple != 0) throuthCountMultiple /= modifier.throuthCountMultiple;
        }
    }
}
