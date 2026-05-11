using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public GameObject Player;
    public PlayerHP playerHP;
    public GunBase currentWeapon; // 今持っている銃
    public Transform weaponHolder;       // 銃を出す場所（カメラの子要素など）
    public int money = 0;

    public List<PassiveEffect> activePassives = new List<PassiveEffect>();
    public PlayerStats sharedStats;      // 全武器共通のステータス
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
    public float minEnemyDistance { get; private set; } = float.MaxValue; // ★ これで外部から読み取れる、よ
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

        // 初期装備の銃がある場合、初期設定を行う
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

    // 武器を変更するメインの関数
    public void ChangeWeapon(GameObject newWeaponPrefab)
    {
        // 1. 今の銃があれば消す
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        // 2. 新しい銃を生成する
        GameObject newWeaponObj = Instantiate(newWeaponPrefab, weaponHolder);
        newWeaponObj.transform.localPosition = currentWeapon.offsetPosition;
        newWeaponObj.transform.localRotation = Quaternion.Euler(currentWeapon.offsetRotation);
        
        // 3. 生成した銃のスクリプトを取得して、自分(Manager)とStatsを教え込む
        currentWeapon = newWeaponObj.GetComponent<GunBase>();
        currentWeapon.pManager = this;
        currentWeapon.stats = sharedStats;
        currentWeapon.ammoText = ammoTextUI;   
        currentWeapon.playerCamera = mainCamera;
        currentWeapon.adsPivot = adsPivot;
        currentWeapon.recoilPivot = recoilPivot;

        // 4. UIの更新などを呼ぶ（あれば）
        currentWeapon.UpdateAmmoDisplay();
        
        Debug.Log("武器を変更した、よ：" + newWeaponObj.name);
    }

    void Update() 
    {
        // 移動状態の監視とパッシブへの通知
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

        // 敵の接近検知（0.1秒間隔）
        proximityCheckTimer += Time.deltaTime;
        if (proximityCheckTimer >= PROXIMITY_CHECK_INTERVAL)
        {
            proximityCheckTimer = 0f;
            UpdateProximityStatus();
        }

        // 全パッシブの定期更新
        foreach (var p in activePassives)
        {
            p.OnUpdate(this);
        }
    }

    private void UpdateProximityStatus()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, proximityRadius, proximityBuffer, enemyLayer);
        bool currentlyNear = count > 0;

        // ★ 数値としての最小距離も計算しておく、よ
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
                Debug.Log("<color=orange>[Proximity] 敵が近くに来た、よ！</color>");
            }
            else
            {
                foreach (var p in activePassives) p.OnEnemyAway(this);
                Debug.Log("<color=cyan>[Proximity] 敵が離れていったんだ、ね</color>");
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
    
        Debug.Log($"{newPassive.effectName} を習得した、よ！");
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
            Debug.Log($"{p.effectName} を失った、よ");
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
            Debug.Log("お金が足りない、よ");
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