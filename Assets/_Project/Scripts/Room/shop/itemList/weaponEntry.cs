using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class WeaponShopEntry
{
    public string weaponName;
    public int price;
    public Sprite icon;
    public GameObject model;
    public string detailtext;
    public GameObject weaponPrefab; // GunBaseがついたプレハブ
}

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Shop/WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponShopEntry> weapons = new List<WeaponShopEntry>();

    public WeaponShopEntry GetRandom(PlayerManager manager)
    {
        // 1. 現在装備中の武器プレハブを除外して候補を作る
        var candidates = weapons.Where(w => 
            manager.currentWeapon == null || w.weaponPrefab != manager.currentWeapon.gameObject
        ).ToList();

        // 2. 候補がない（リストに1種類しかなくて、それを装備中の場合など）
        if (candidates.Count == 0)
        {
            // 全リストからランダムに返すか、nullを返すかは仕様に合わせて
            return weapons[Random.Range(0, weapons.Count)];
        }

        // 3. 候補から抽選
        return candidates[Random.Range(0, candidates.Count)];
    }
}