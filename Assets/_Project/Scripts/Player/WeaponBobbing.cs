using UnityEngine;
public class WeaponBobbing : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Move playerMovement; 
    [Header("揺れの設定")]
    public float walkingBobbingSpeed = 10f; 
    public float bobbingAmount = 0.05f;     
    public float sideBobbingAmount = 0.03f; 
    [Header("ADS時の揺れの設定")]
    public float adsMultiple_v = 0.3f;
    public float adsMultiple_h = 0.3f;
    private float timer = 0f;
    private Vector3 defaultPosition;
    void Start()
    {
        defaultPosition = transform.localPosition;
    }
    void Update()
    {
        if (playerMovement.isMoving)
        {
            timer += Time.deltaTime * walkingBobbingSpeed;
            float verticalOffset = Mathf.Sin(timer) * bobbingAmount;
            float horizontalOffset = Mathf.Cos(timer * 0.5f) * sideBobbingAmount;
            if(playerMovement.pManager.currentWeapon.isAiming)
            {
                verticalOffset *= adsMultiple_v;
                horizontalOffset *= adsMultiple_h;
            }
            transform.localPosition = new Vector3(
                defaultPosition.x + horizontalOffset,
                defaultPosition.y + verticalOffset,
                defaultPosition.z
            );
        }
        else
        {
            timer = 0;
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPosition, Time.deltaTime * 5f);
        }
    }
}
