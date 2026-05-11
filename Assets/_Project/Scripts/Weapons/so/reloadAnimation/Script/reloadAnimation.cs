using UnityEngine;
using DG.Tweening;
public abstract class reloadAnimation : ScriptableObject
{
    public float reloadTime;
    public abstract void Play(GunBase gun);
}
