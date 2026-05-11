using UnityEngine;
[CreateAssetMenu(menuName = "Passives/ContinuosBuff")]
public class continuos_buff : PassiveEffect
{
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private float damagebuffMultiple;
    [SerializeField] private float fireratebuffMultiple;
    [SerializeField] private int buffCount;
    [SerializeField] private int maxbuffCount = 5;
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        return 1f + (damagebuffMultiple * buffCount);
    }
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        return 1f + (fireratebuffMultiple * buffCount);
    }
    public override void OnTakeDamage(PlayerManager manager, float damage)
    {
        if (buffCount < maxbuffCount)
        {
            buffCount ++;
        }
    }
    public override void OnMiss(PlayerManager manager)
    {
            buffCount = 0;
    }
}
