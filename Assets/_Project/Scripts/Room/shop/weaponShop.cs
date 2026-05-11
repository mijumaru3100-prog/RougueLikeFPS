using UnityEngine;
using TMPro;

public class weaponshop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public GameObject barrier;
    
    [Header("Item Data")]
    public WeaponDatabase itempool;
    public WeaponShopEntry selected;
    private GameObject model;

    [Header("UI")]
    public TextMeshPro nameText;
    public TextMeshPro priceText;
    public TextMeshPro detailText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;


    private bool used = false;


    public Transform modelSpawnPos;
    public bool spin = false;
    public int spinSpeed = 1;
    
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
        selected = itempool.GetRandom(manager);
        nameText.text = selected.weaponName;
        priceText.text = selected.price.ToString();
        detailText.text = selected.detailtext;
        model = Instantiate(selected.model, transform);
    }

    public void buy()
    {
        if(selected == null) return;
        if(used == true) return;



        if(manager.TrySpendMoney(selected.price))
        {
            manager.ChangeWeapon(selected.weaponPrefab);
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


}
