using UnityEngine;
public abstract class PassiveEffect : ScriptableObject
{
    public string effectName;
    public bool canStack = false;
    public virtual float GetFireRateMultiplier(PlayerManager manager) => 1f;    
    public virtual float GetDamageMultiplier(PlayerManager manager) => 1f;      
    public virtual float GetReloadSpeedMultiplier(PlayerManager manager) => 1f; 
    public virtual float GetRecoilMultiplier(PlayerManager manager) => 1f;      
    public virtual void OnShot(PlayerManager manager) { }
    public virtual void OnReloadComplete(PlayerManager manager) { }
    public virtual void OnMiss(PlayerManager manager) { }
    public virtual void OnGetThisPassive(PlayerManager manager) { }
    public virtual void OnGetPassive(PlayerManager manager) { }
    public virtual void OnGetDamage(PlayerManager manager) { }
    public virtual void OnHeal(PlayerManager manager) { }
    public virtual void OnTakeDamage(PlayerManager manager, float damage) { }
    public virtual void OnKillEnemy(PlayerManager manager) { }
    public virtual void OnStopping(PlayerManager manager) { }
    public virtual void OnMoving(PlayerManager manager) { }
    public virtual void OnEnemyNear(PlayerManager manager) { }
    public virtual void OnEnemyAway(PlayerManager manager) { }
    public virtual void OnUpdate(PlayerManager manager) { }
}