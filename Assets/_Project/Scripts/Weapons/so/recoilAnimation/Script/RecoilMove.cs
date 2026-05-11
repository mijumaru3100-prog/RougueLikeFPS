using UnityEngine;

// 抽象クラス。これ自体は実体化できない、概念だけの存在……。
public abstract class recoilAnimation : ScriptableObject
{
    // gunbase_saigenのfireから呼び出される、よ
    public abstract void Play(Transform target, Vector3 posAmount, Vector3 rotAmount, float duration);
}