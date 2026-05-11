using UnityEngine;
public abstract class recoilAnimation : ScriptableObject
{
    public abstract void Play(Transform target, Vector3 posAmount, Vector3 rotAmount, float duration);
}