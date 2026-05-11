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
    public float invincibleTime = 1.0f; 
    private bool isInvincible = false;
    public GameObject heartPrefab;
    public Transform heartParent;
    private List<HeartController> hearts = new List<HeartController>();
    void Start()
    {
        currentHP = maxHP;
        SetupHearts();
        UpdateHeartUI();
    }
    void SetupHearts()
    {
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
        int neededHearts = Mathf.CeilToInt(maxHP / 2f);
        if (neededHearts != hearts.Count)
        {
            SetupHearts();
        }
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
            float fillAmount = (currentHP - (i * 2)) / 2f;
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
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            SetHeartsAlpha(0.3f); 
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
            SetHeartsAlpha(1.0f);
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        SetHeartsAlpha(1.0f);
        isInvincible = false;
    }
    void SetHeartsAlpha(float alpha)
    {
        foreach (var heart in hearts)
        {
            Color c = heart.fillImage.color;
            c.a = alpha;
            heart.fillImage.color = c;
        }
    }
}