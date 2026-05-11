using UnityEngine;
using System.Collections;
[CreateAssetMenu(menuName = "Passives/TaletMode")]
public class TaletMode : PassiveEffect
{
    [SerializeField] private float buffCoolTime = 1f;
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private bool isAdditive;
    private int buffCount = 0;
    private bool isRunning = false;
    private Coroutine activeCoroutine;
    public override void OnStopping(PlayerManager manager)
    {
        if (manager == null || buffStats == null || isRunning) return;
        isRunning = true;
        activeCoroutine = manager.StartCoroutine(WaitTime(manager.sharedStats));
    }
    private IEnumerator WaitTime(PlayerStats stats)
    {
        for(int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(buffCoolTime);
            stats.ApplyModifier(buffStats, true, isAdditive);
            buffCount++;
        }
        isRunning = false;
    }
    public override void OnMoving(PlayerManager manager)
    {
        if (manager == null || buffStats == null) return;
        if (activeCoroutine != null)
        {
            manager.StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        for(int i = 0; i < buffCount; i++)
        {
            manager.sharedStats.ApplyModifier(buffStats, false, isAdditive);
        }
        buffCount = 0;
        isRunning = false;
    }
}