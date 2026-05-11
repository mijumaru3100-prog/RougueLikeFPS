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
    [SerializeField] private AnimationCurve distancePowerCurve = AnimationCurve.Linear(0, 1, 1, 0); // 0mで最強(1), 10mで加算なし(0)

    // 連射速度の倍率計算
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        if (!scaleByDistance) return 1f;
        float factor = GetDistanceFactor(manager.minEnemyDistance);
        return 1f + (buffStats.fireRateMultiple - 1f) * factor;
    }

    // ダメージの倍率計算
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if (!scaleByDistance) return 1f;
        float factor = GetDistanceFactor(manager.minEnemyDistance);
        return 1f + (buffStats.damageMultiple - 1f) * factor;
    }

    // 反動の倍率計算（反動は小さいほうが嬉しいので、数値を下げる方向に働く）
    public override float GetRecoilMultiplier(PlayerManager manager)
    {
        if (!scaleByDistance) return 1f;
        float factor = GetDistanceFactor(manager.minEnemyDistance);
        // 設定値が 0.5 (半分) なら、至近距離で 0.5 に近づくように計算
        return 1f - (1f - buffStats.recoilForceMultiple) * factor;
    }

    private float GetDistanceFactor(float distance)
    {
        // 0m 〜 checkDistance を 0.0 〜 1.0 の割合に変換して、カーブを通す
        float t = Mathf.Clamp01(distance / checkDistance);
        return distancePowerCurve.Evaluate(t);
    }

    // ------------------------------------------------------------------------
    // 【使い分けの例】
    // 倍率以外の「固定的なモード変更」などは、今まで通りイベント型を使う、よ
    // ------------------------------------------------------------------------
    public override void OnEnemyNear(PlayerManager manager)
    {
        // 例えば「近くに敵がいる時だけリロード速度を固定で上げる」ならここでもOK
        //（今回は倍率型に統一したので、ここは空でも大丈夫だ、ね）
    }

    public override void OnEnemyAway(PlayerManager manager)
    {
    }
}
