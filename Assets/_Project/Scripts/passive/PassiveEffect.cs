using UnityEngine;

public abstract class PassiveEffect : ScriptableObject
{
    public string effectName;
    public bool canStack = false;

    public virtual float GetFireRateMultiplier(PlayerManager manager) => 1f;    // 連射速度
    public virtual float GetDamageMultiplier(PlayerManager manager) => 1f;      // ダメージ
    public virtual float GetReloadSpeedMultiplier(PlayerManager manager) => 1f; // リロード速度
    public virtual float GetRecoilMultiplier(PlayerManager manager) => 1f;      // 反動の大きさ
    
    // gun 
    public virtual void OnShot(PlayerManager manager) { }
    public virtual void OnReloadComplete(PlayerManager manager) { }

    public virtual void OnMiss(PlayerManager manager) { }

    // PlayerManager
    public virtual void OnGetThisPassive(PlayerManager manager) { }
    public virtual void OnGetPassive(PlayerManager manager) { }

    // PlayerHP
    public virtual void OnGetDamage(PlayerManager manager) { }
    public virtual void OnHeal(PlayerManager manager) { }

    // EnemyHP
    public virtual void OnTakeDamage(PlayerManager manager, float damage) { }
    public virtual void OnKillEnemy(PlayerManager manager) { }

    // 移動・状態変化
    public virtual void OnStopping(PlayerManager manager) { }
    public virtual void OnMoving(PlayerManager manager) { }

    // 敵の接近に応じたフック ( 判定距離は PlayerManager 側で設定 )
    public virtual void OnEnemyNear(PlayerManager manager) { }
    public virtual void OnEnemyAway(PlayerManager manager) { }

    // 毎フレームの更新（PlayerManagerのUpdateから定期的に呼ばれる）
    public virtual void OnUpdate(PlayerManager manager) { }
}