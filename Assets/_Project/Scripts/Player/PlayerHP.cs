using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHP : MonoBehaviour
{
    public int baseMaxHP = 6;
    public int maxHP => Mathf.RoundToInt((baseMaxHP + pManager.sharedStats.bonusMaxHP) * pManager.sharedStats.maxHPMultiple);
    public float currentHP;
    public PlayerManager pManager;

    [Header("無敵設定")]
    public float invincibleTime = 1.0f; // 無敵時間の長さ
    private bool isInvincible = false;

    public GameObject heartPrefab;
    public Transform heartParent;
    
    // Imageの代わりに、HeartControllerをリストに溜める
    private List<HeartController> hearts = new List<HeartController>();

    void Start()
    {
        currentHP = maxHP;
        SetupHearts();
        UpdateHeartUI();
    }

    void SetupHearts()
    {
        // 既存のハートをクリア
        foreach (var h in hearts)
        {
            if (h != null) Destroy(h.gameObject);
        }
        hearts.Clear();

        int heartCount = Mathf.CeilToInt(maxHP / 2f);

        for (int i = 0; i < heartCount; i++)
        {
            GameObject obj = Instantiate(heartPrefab, heartParent);
            HeartController hc = obj.GetComponent<HeartController>();
            hearts.Add(hc);
        }
    }

    public void SyncMaxHPWithStats()
    {
        float oldMaxHP = maxHP;
        // maxHPはプロパティなので自動的に最新のstatsを参照する、よ
        
        // ハートの数を再調整する必要があるかチェック
        int neededHearts = Mathf.CeilToInt(maxHP / 2f);
        if (neededHearts != hearts.Count)
        {
            SetupHearts();
        }

        // 最大HPが増えた分、現在のHPも回復させる（おまけだ、よ）
        if (maxHP > oldMaxHP)
        {
            currentHP += (maxHP - oldMaxHP);
        }
        
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHeartUI();
        
        Debug.Log($"[PlayerHP] MaxHP updated: {maxHP} (Base: {baseMaxHP})");
    }

    void UpdateHeartUI()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            // 1つのハートが受け持つHP範囲を計算（ハート1個 = 2HPの場合）
            float fillAmount = (currentHP - (i * 2)) / 2f;
            
            // 専用スクリプトの関数を呼ぶだけ！
            hearts[i].SetHeart(fillAmount);
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        UpdateHeartUI();
        foreach (var p in pManager.activePassives)
        {
            p.OnHeal(pManager);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        UpdateHeartUI();
        if (currentHP > 0)
        {
            // ハートを点滅させるコルーチンを開始
            StartCoroutine(FlashHearts());
        }
        
        foreach (var p in pManager.activePassives)
        {
            p.OnGetDamage(pManager);
        }
    }

    IEnumerator FlashHearts()
    {
        isInvincible = true;

        // 無敵時間が終わるまで繰り返す
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            // --- ハートを半透明にする ---
            SetHeartsAlpha(0.3f); 
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;

            // --- ハートを不透明に戻す ---
            SetHeartsAlpha(1.0f);
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        // 最後は必ず不透明（1.0）に戻して無敵終了
        SetHeartsAlpha(1.0f);
        isInvincible = false;
    }

    // すべてのハートの透明度を一括で変える便利メソッド
    void SetHeartsAlpha(float alpha)
    {
        foreach (var heart in hearts)
        {
            // HeartController経由でImageの色を変える
            // fillImage（中身）と、もしあれば背景も変えたい場合はここを調整
            Color c = heart.fillImage.color;
            c.a = alpha;
            heart.fillImage.color = c;
        }
    }
}