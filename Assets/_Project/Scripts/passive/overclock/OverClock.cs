using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "NewOverClock", menuName = "Passives/OverClock")]
public class OverClock : PassiveEffect
{
    [SerializeField] private float buffDuration = 1.5f;
    public PlayerStats buffStats;
    public bool isAdditive = false;
    public override void OnReloadComplete(PlayerManager manager)
    {
        if (manager != null && buffStats != null)
        {
            manager.StartCoroutine(ApplyBuff(manager.sharedStats));
        }
    }
    private IEnumerator ApplyBuff(PlayerStats stats)
    {
        stats.ApplyModifier(buffStats, true, isAdditive);
        yield return new WaitForSeconds(buffDuration);
        stats.ApplyModifier(buffStats, false, isAdditive);
    }
}
