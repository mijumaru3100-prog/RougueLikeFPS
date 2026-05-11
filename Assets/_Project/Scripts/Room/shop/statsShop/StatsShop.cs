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
        priceText.text = price.ToString(); 
        detailText.text = selected.detailtext;
        buffStats = selected.stats;
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
            price = Mathf.CeilToInt(price * nextpriceMultiple);
            ShopSetting();
            if (manager.currentWeapon != null)
            {
                manager.currentWeapon.UpdateAmmoDisplay();
            }
            PlayerHP hp = manager.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.SyncMaxHPWithStats();
            }
            Debug.Log($"[StatsShop] 購入しました。次の価格: {price}");
        }
    }    
}
