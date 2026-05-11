using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PassiveShopEntry
{
    public string passiveName;
    public int price;
    
    public string detailtext; 
    public Sprite icon;
    public GameObject model;
    public PassiveEffect effect; // メインのパッシブ効果
    public List<PassiveEffect> additionalEffects = new List<PassiveEffect>(); // 追加のパッシブ効果
    public PlayerStats statBuff; // 一緒に得られるステータスバフ
}

[CreateAssetMenu(fileName = "PassiveDatabase", menuName = "Shop/PassiveDatabase")]
public class PassiveDatabase : ScriptableObject
{
    public List<PassiveShopEntry> passives = new List<PassiveShopEntry>();

    public PassiveShopEntry GetRandom(PlayerManager manager)
    {
        // メインの効果がスタック可能か、まだ持っていないものを候補にする
        var candidates = passives.Where(p => 
            p.effect != null && (p.effect.canStack || !manager.activePassives.Contains(p.effect))
        ).ToList();
        
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
}