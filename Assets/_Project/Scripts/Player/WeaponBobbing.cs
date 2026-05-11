using UnityEngine;

public class WeaponBobbing : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Move playerMovement; // 移動スクリプトの参照

    [Header("揺れの設定")]
    public float walkingBobbingSpeed = 10f; // 揺れる速さ
    public float bobbingAmount = 0.05f;     // 揺れる大きさ（上下）
    public float sideBobbingAmount = 0.03f; // 揺れる大きさ（左右）

    [Header("ADS時の揺れの設定")]
    public float adsMultiple_v = 0.3f;
    public float adsMultiple_h = 0.3f;

    private float timer = 0f;
    private Vector3 defaultPosition;

    void Start()
    {
        // 初期位置を記憶
        defaultPosition = transform.localPosition;
    }

    void Update()
    {
        // 移動しているかどうかをチェック
        if (playerMovement.isMoving)
        {
            // 移動速度に応じてタイマーを進める（サイン波の計算用）
            timer += Time.deltaTime * walkingBobbingSpeed;

            // サイン波を使って揺れを作る
            // 上下は sin の 2倍の速さにすると自然な「8の字」のような動きになります
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
            // 止まったらゆっくり元の位置に戻す
            timer = 0;
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPosition, Time.deltaTime * 5f);
        }
    }
}
