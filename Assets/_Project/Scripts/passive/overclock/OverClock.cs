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
            // ScriptableObjectからは直接コルーチンを呼べないため、manager(MonoBehaviour)に頼む
            manager.StartCoroutine(ApplyBuff(manager.sharedStats));
        }
    }

    private IEnumerator ApplyBuff(PlayerStats stats)
    {
        // バフを適用 (true = 追加)
        stats.ApplyModifier(buffStats, true, isAdditive);

        yield return new WaitForSeconds(buffDuration);

        // バフを解除 (false = 削除)
        stats.ApplyModifier(buffStats, false, isAdditive);
    }
}
