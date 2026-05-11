using UnityEngine;
[CreateAssetMenu(fileName = "InfightBuff", menuName = "Passives/InfightBuff")]
public class InfightBuff : PassiveEffect
{
    [Header("基本設定")]
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private bool isAdditive;
    [SerializeField] private float checkDistance = 10f;
    [Header("距離によるリアルタイム・スケーリング")]
    [SerializeField] private bool scaleByDistance = true;
    [SerializeField] private AnimationCurve distancePowerCurve = AnimationCurve.Linear(0, 1, 1, 0); 
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        if (!scaleByDistance) return 1f;
        float factor = GetDistanceFactor(manager.minEnemyDistance);
        return 1f + (buffStats.fireRateMultiple - 1f) * factor;
    }
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if (!scaleByDistance) return 1f;
        float factor = GetDistanceFactor(manager.minEnemyDistance);
        return 1f + (buffStats.damageMultiple - 1f) * factor;
    }
    public override float GetRecoilMultiplier(PlayerManager manager)
    {
        if (!scaleByDistance) return 1f;
        float factor = GetDistanceFactor(manager.minEnemyDistance);
        return 1f - (1f - buffStats.recoilForceMultiple) * factor;
    }
    private float GetDistanceFactor(float distance)
    {
        float t = Mathf.Clamp01(distance / checkDistance);
        return distancePowerCurve.Evaluate(t);
    }
    public override void OnEnemyNear(PlayerManager manager)
    {
    }
    public override void OnEnemyAway(PlayerManager manager)
    {
    }
}
