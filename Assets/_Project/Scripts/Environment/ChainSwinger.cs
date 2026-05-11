using UnityEngine;

public class ChainSwinger : MonoBehaviour
{
    [Header("揺れの設定")]
    public float speed = 0.5f;     // 揺れる速さ
    public float amount = 2.0f;    // 揺れる角度（大きくすると激しくなる）
    
    private Quaternion startRotation;
    private float randomOffset;

    void Start()
    {
        // 最初の向きを記録
        startRotation = transform.localRotation;
        // 全部同じ動きだと不自然なので、個別にズレを作る
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        // サイン波を使って左右にゆらゆらさせる
        float angle = Mathf.Sin(Time.time * speed + randomOffset) * amount;
        
        // Z軸を中心に揺らす（鎖の向きに合わせてXやYに変えてもOK）
        transform.localRotation = startRotation * Quaternion.Euler(0, 0, angle);
    }
}