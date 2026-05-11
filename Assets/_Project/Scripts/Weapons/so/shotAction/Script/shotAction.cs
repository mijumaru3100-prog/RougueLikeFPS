using UnityEngine;

public abstract class shotAction:ScriptableObject
{
    // gunbase_saigenのfireから呼び出される、よ
    public abstract void shot(GunBase baseGun);
}