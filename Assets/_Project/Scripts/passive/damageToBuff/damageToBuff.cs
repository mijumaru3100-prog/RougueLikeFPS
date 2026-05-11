using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/damageToBuff")]
public class damageToBuff : PassiveEffect
{
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private float buffTime;
    
    public override void OnTakeDamage(PlayerManager manager, float damage)
    {
        manager.sharedStats.ApplyModifier(buffStats, true, false); 
        manager.StartCoroutine(RemoveBuffAfterDelay(manager, buffTime));
    }

    private IEnumerator RemoveBuffAfterDelay(PlayerManager manager, float delay)
    {
        yield return new WaitForSeconds(delay);
        manager.sharedStats.ApplyModifier(buffStats, false, false); 
    }
}
