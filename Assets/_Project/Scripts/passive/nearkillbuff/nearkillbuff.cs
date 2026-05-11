using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/NearKillBuff")]
public class nearkillbuff : PassiveEffect
{
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private float range = 5f;
    [SerializeField] private float buffTime = 5f;
    [SerializeField] private bool isAdditive;

    public override void OnKillEnemy(PlayerManager manager)
    {
        if(manager.minEnemyDistance < range)
        {
            manager.sharedStats.ApplyModifier(buffStats, true, isAdditive);
            manager.StartCoroutine(RemoveBuff(manager));
        }   
    }
    
    private IEnumerator RemoveBuff(PlayerManager manager)
    {
        yield return new WaitForSeconds(buffTime);
        manager.sharedStats.ApplyModifier(buffStats, false, isAdditive);
    }
}
