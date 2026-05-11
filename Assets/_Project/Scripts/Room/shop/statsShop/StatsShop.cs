using UnityEngine;
using TMPro;

public class StatsShop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public Transform modelPos;
    
    [Header("statsData")]
    public StatsDatabase pool;
    private StatsShopEntry selected;
    private PlayerStats buffStats;
    public int defaultPrice;
    private int price;
    public bool isApplying = true;
    public bool isAdditive = true;
    public float nextpriceMultiple = 1.5f;
    private GameObject currentModel;

    [Header("UI")]
    public TextMeshPro nameText;
    public TextMeshPro priceText;
    public TextMeshPro detailText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;
    
    void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<PlayerManager>();
        }
        price = defaultPrice;
        ShopSetting();
    }

    void Update()
    {
        if (isInside && Input.GetKeyDown(KeyCode.E))
        {
            buy();
        }
    }

    void ShopSetting()
    {
        selected = pool.GetRandom();
        nameText.text = selected.statName;
        priceText.text = price.ToString(); // 現在の価格を表示
        detailText.text = selected.detailtext;
        buffStats = selected.stats;

        // 前のモデルを削除
        if (currentModel != null) Destroy(currentModel);
        currentModel = Instantiate(selected.model, modelPos.position, modelPos.rotation, modelPos);


        UpdateBuyText();
    }

    void UpdateBuyText()
    {
        if (isInside && PlayerBuyText != null && buyTextData != null)
        {
            buyTextData.text = "Press E to buy for " + price.ToString();
        }
    }

    public void ResetShop()
    {
        price = defaultPrice;
    }

    private bool isInside = false;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            isInside = true;
            if (PlayerBuyText != null) PlayerBuyText.SetActive(true);
            UpdateBuyText();
        }
    }

    void OnTriggerExit(Collider other)
    {
        
        if(other.gameObject.CompareTag("Player"))
        {
            isInside = false;
            PlayerBuyText.SetActive(false);
        }
    }

    public void buy()
    {
        if(manager != null && manager.TrySpendMoney(price))
        {
            manager.sharedStats.ApplyModifier(buffStats, isApplying, isAdditive);
            
            // 価格を更新
            price = Mathf.CeilToInt(price * nextpriceMultiple);
            
            // 次のアイテムをセット
            ShopSetting();

            // 武器の表示を更新（弾薬最大値が変わる可能性があるため）
            if (manager.currentWeapon != null)
            {
                manager.currentWeapon.UpdateAmmoDisplay();
            }

            // HPの更新
            PlayerHP hp = manager.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.SyncMaxHPWithStats();
            }

            Debug.Log($"[StatsShop] を購入した、よ！ 次の価格: {price}");
        }
    }    

    
}
