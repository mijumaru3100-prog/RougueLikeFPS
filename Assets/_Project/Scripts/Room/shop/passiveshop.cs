using UnityEngine;
using TMPro;

public class passiveshop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public GameObject barrier;
    
    [Header("Item Data")]
    public PassiveDatabase itempool;
    public PassiveShopEntry selected;
    private GameObject model;

    [Header("UI")]
    public TextMeshPro nameText;
    public TextMeshPro priceText;
    public TextMeshPro detailText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;
    public Transform modelSpawnPos;


    public bool spin = false;
    public int spinSpeed = 1;
    private bool used = false;

    
    void Start()
    {
        ShopSetting();
    }

    void Update()
    {
        if (model != null && spin == true)
        {
            model.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }

        if (isInside && Input.GetKeyDown(KeyCode.E))
        {
            buy();
        }
    }

    void ShopSetting()
    {
        ResetShop();
        selected = itempool.GetRandom(manager);
        nameText.text = selected.passiveName;
        priceText.text = selected.price.ToString();
        detailText.text = selected.detailtext;
        
        if(model != null) Destroy(model);
        model = Instantiate(selected.model, modelSpawnPos.transform.position, modelSpawnPos.transform.rotation);
    }

    public void ResetShop()
    {
        if(barrier != null)
        {
            barrier.SetActive(true);
        }
    }

    private bool isInside = false;
    void OnTriggerEnter(Collider other)
    {
        if(used) return;
        if(other.gameObject.CompareTag("Player"))
        {
            isInside = true;
            PlayerBuyText.SetActive(true);
            buyTextData.text = "Press E to buy for " + selected.price.ToString();
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
        if(selected == null) return;
        if(used == true) return;

        if(manager.TrySpendMoney(selected.price))
        {
            // メインのパッシブを適用
            if (selected.effect != null)
            {
                manager.AddPassive(selected.effect);
            }

            // 追加のパッシブがあればすべて適用
            if (selected.additionalEffects != null)
            {
                foreach (var extra in selected.additionalEffects)
                {
                    if (extra != null)
                    {
                        manager.AddPassive(extra);
                    }
                }
            }

            // ステータスバフがあれば適用
            if (selected.statBuff != null)
            {
                manager.sharedStats.ApplyModifier(selected.statBuff, true, true);
                
                // 武器の表示を更新（弾数など）
                if (manager.currentWeapon != null)
                {
                    manager.currentWeapon.UpdateAmmoDisplay();
                }

                // 最大HPが変わる可能性があるので更新
                PlayerHP hp = manager.GetComponent<PlayerHP>();
                if (hp != null)
                {
                    hp.SyncMaxHPWithStats();
                }
            }

            used = true;
            
            if(barrier != null)
            {
                barrier.SetActive(false);
            }

            if(model != null)
            {
                Destroy(model);
                model = null;
            }

            DeliteText();
            PlayerBuyText.SetActive(false);
        }
    }    

    void DeliteText()
    {
        if(nameText != null)
        {
            nameText.text = "Sold Out";
        }

        if(priceText != null)
        {
            priceText.text = "";
        }

        if(detailText != null)
        {
            detailText.text = "";
        }
    }

}
