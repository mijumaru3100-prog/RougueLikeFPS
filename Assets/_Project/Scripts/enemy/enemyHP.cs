using UnityEngine;
using System.Collections;
using TMPro;
public class EnemyHP : MonoBehaviour 
{
    public int maxHP = 100;
    private int currentHP;
    [Header("Effects")]
    public GameObject damageTextPrefab;
    private TextMeshPro text;
    public float damageTextTime = 1f;
    private float lastDamageTime = 0f;
    private int totalDamage = 0;
    [Header("ライト演出")]
    public Light flashLight; 
    public float flashIntensity = 20f; 
    private Coroutine flashCoroutine;
    [Header("弱点")]
    [SerializeField]private Collider weakPointCollider;
    public float defaultWeakPointMultiple = 2f;
    float weakPointMultiple => defaultWeakPointMultiple + stats.bonusWeakPointMultiple;
    public PlayerStats stats;
    public EnemyAI _enemyAI;
    private bool isWeakPointDamage = false;
    void Start()
    {
        currentHP = maxHP;
        if (_enemyAI == null) _enemyAI = GetComponentInParent<EnemyAI>();
        if (damageTextPrefab != null)
        {
            text = damageTextPrefab.GetComponent<TextMeshPro>();
        }
    }
    public void TakeDamage(float damage, Collider hitColider, PlayerManager pManager)
    {
        float finalDamage = damage;
        isWeakPointDamage = false;
        if (hitColider == weakPointCollider && stats != null)
        {
            float bonus = (pManager != null && pManager.sharedStats != null) ? pManager.sharedStats.bonusWeakPointMultiple : 0f;
            finalDamage = Mathf.RoundToInt(damage * (weakPointMultiple + bonus));
            isWeakPointDamage = true;
        }
        currentHP -= Mathf.RoundToInt(finalDamage);
        if (damageTextPrefab != null && text != null)
        {
            HandleDamageText(Mathf.RoundToInt(finalDamage), isWeakPointDamage);
        }
        if (pManager != null)
        {
            foreach (var p in pManager.activePassives)
            {
                p.OnTakeDamage(pManager, finalDamage);
            }
        }
        if (_enemyAI != null) _enemyAI.OnDamage();
        PlayFlash();
        if (currentHP <= 0)
        {
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives)
                {
                    p.OnKillEnemy(pManager);
                }
            }
            Death();
        }
    }
    private void HandleDamageText(int damage,bool isWeakPointDamage)
    {
        if (damageTextPrefab == null) return;
        if (Time.time - lastDamageTime > damageTextTime)
        {
            totalDamage = 0;
        }
        if(isWeakPointDamage)
        {
            text.color = Color.yellow;
        }
        else
        {
            text.color = Color.red;
        }
        totalDamage += damage;
        text.fontSize = Mathf.Min(100+ 0.1f * totalDamage,200);
        lastDamageTime = Time.time;
        text.text = totalDamage.ToString();
        damageTextPrefab.SetActive(true);
        StartCoroutine(HideDamageText());
    }
    IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(damageTextTime);
        if(Time.time - lastDamageTime > damageTextTime)
        {
            damageTextPrefab.SetActive(false);
        }
    }
    void Death()
    {
        _enemyAI.OnDie();
    }
    public void PlayFlash(float duration = 0.1f)
    {
        if (flashLight == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(LightFlashRoutine(duration));
    }
    private IEnumerator LightFlashRoutine(float duration)
    {
        flashLight.color = Color.white;
        flashLight.intensity = flashIntensity;
        yield return new WaitForSeconds(duration * 0.5f);
        flashLight.color = Color.red;
        flashLight.intensity = flashIntensity * 0.5f;
        yield return new WaitForSeconds(duration * 0.5f);
        flashLight.intensity = 0f;
    }
}