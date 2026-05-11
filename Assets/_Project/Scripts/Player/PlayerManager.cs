using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public GameObject Player;
    public PlayerHP playerHP;
    public GunBase currentWeapon;
    public Transform weaponHolder;
    public int money = 0;

    public List<PassiveEffect> activePassives = new List<PassiveEffect>();
    public PlayerStats sharedStats;
    public TextMeshProUGUI ammoTextUI; 
    public TextMeshProUGUI moneyTextUI;
    public Camera mainCamera;
    public Transform adsPivot;
    public Transform recoilPivot;
    public CrosshairController crosshair;
    public DungeonManager dungeonManager;

    private Move playerMover;
    private bool wasMoving = false;
    public float startStopTime;

    [Header("Proximity Settings")]
    public LayerMask enemyLayer;
    public float proximityRadius = 10f;
    public bool isEnemyNear = false;
    public float minEnemyDistance { get; private set; } = float.MaxValue;
    private float proximityCheckTimer = 0f;
    private const float PROXIMITY_CHECK_INTERVAL = 0.1f;
    private Collider[] proximityBuffer = new Collider[16];

    [Header("オブジェクトプール")]
    public ObjectPool bulletPool;
    public ObjectPool shellPool;
    public ObjectPool tracerPool;

    void Start()
    {
        playerMover = GetComponent<Move>();
        
        // ScriptableObjectの意図しない上書きを防ぐため、ゲーム開始時にステータスを初期化
        if (sharedStats != null)
        {
            sharedStats.ResetToDefault();
            Debug.Log("[PlayerManager] PlayerStats has been reset to default values.");
        }

        if (currentWeapon != null)
        {
            currentWeapon.pManager = this;
            currentWeapon.stats = sharedStats;
            currentWeapon.ammoText = ammoTextUI;
            currentWeapon.playerCamera = mainCamera;
            currentWeapon.adsPivot = adsPivot;
            currentWeapon.recoilPivot = recoilPivot;
            currentWeapon.UpdateAmmoDisplay();
        }

        UpdateMoneyText();
    }

    public void ChangeWeapon(GameObject newWeaponPrefab)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        GameObject newWeaponObj = Instantiate(newWeaponPrefab, weaponHolder);
        newWeaponObj.transform.localPosition = currentWeapon.offsetPosition;
        newWeaponObj.transform.localRotation = Quaternion.Euler(currentWeapon.offsetRotation);
        
        currentWeapon = newWeaponObj.GetComponent<GunBase>();
        currentWeapon.pManager = this;
        currentWeapon.stats = sharedStats;
        currentWeapon.ammoText = ammoTextUI;   
        currentWeapon.playerCamera = mainCamera;
        currentWeapon.adsPivot = adsPivot;
        currentWeapon.recoilPivot = recoilPivot;

        currentWeapon.UpdateAmmoDisplay();
        
        Debug.Log("武器を変更しました：" + newWeaponObj.name);
    }

    void Update() 
    {
        if (playerMover != null)
        {
            bool isCurrentlyMoving = playerMover.isMoving;
            if (isCurrentlyMoving != wasMoving)
            {
                if (isCurrentlyMoving)
                {
                    foreach (var p in activePassives) p.OnMoving(this);
                    startStopTime = Time.time;
                }
                else
                {
                    foreach (var p in activePassives) p.OnStopping(this);
                    startStopTime = 0f;
                }    
                wasMoving = isCurrentlyMoving;
            }
        }

        proximityCheckTimer += Time.deltaTime;
        if (proximityCheckTimer >= PROXIMITY_CHECK_INTERVAL)
        {
            proximityCheckTimer = 0f;
            UpdateProximityStatus();
        }

        foreach (var p in activePassives)
        {
            p.OnUpdate(this);
        }
    }

    private void UpdateProximityStatus()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, proximityRadius, proximityBuffer, enemyLayer);
        bool currentlyNear = count > 0;

        float minDist = float.MaxValue;
        if (currentlyNear)
        {
            for (int i = 0; i < count; i++)
            {
                float d = Vector3.Distance(transform.position, proximityBuffer[i].transform.position);
                if (d < minDist) minDist = d;
            }
        }
        minEnemyDistance = minDist;

        if (currentlyNear != isEnemyNear)
        {
            isEnemyNear = currentlyNear;
            if (isEnemyNear)
            {
                foreach (var p in activePassives) p.OnEnemyNear(this);
                Debug.Log("<color=orange>[Proximity] 敵が接近しました。</color>");
            }
            else
            {
                foreach (var p in activePassives) p.OnEnemyAway(this);
                Debug.Log("<color=cyan>[Proximity] 敵が離れました。</color>");
            }
        }
    }

    public void ToggleWeaponVisibility(bool isVisible)
    {
        if (weaponHolder != null)
        {
            weaponHolder.gameObject.SetActive(isVisible);
        }
    }

    public void AddPassive(PassiveEffect newPassive)
    {
        if (newPassive == null) return;

        activePassives.Add(newPassive);

        newPassive.OnGetThisPassive(this);

        // すでに立ち止まっている状態なら、追加した瞬間に OnStopping を適用する
        if (playerMover != null && !playerMover.isMoving)
        {
            newPassive.OnStopping(this);
        }
    
        foreach (var p in this.activePassives)
        {
            p.OnGetPassive(this);
        }
    
        Debug.Log($"{newPassive.effectName} を習得しました。");
    }

    public void RemovePassive(PassiveEffect p)
    {
        if (activePassives.Contains(p))
        {
            // 外す前に、もし立ち止まっていて効果が適用中なら、解除（OnMoving相当）させる
            if (playerMover != null && !playerMover.isMoving)
            {
                p.OnMoving(this); 
            }
            activePassives.Remove(p);
            Debug.Log($"{p.effectName} を削除しました。");
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyText();
    }

    public bool TrySpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyText();
            return true;
        }
        else
        {
            Debug.Log("所持金が不足しています。");
            return false;
        }
    }

    public void UpdateMoneyText()
    {
        if (moneyTextUI != null)
        {
            moneyTextUI.text = money.ToString("000");
        }
    }
}