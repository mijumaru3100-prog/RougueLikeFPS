using UnityEngine;
using TMPro;
using DG.Tweening; 

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro _tmp;
    [SerializeField] private float _resetTime = 1.0f;

    private Sequence _activeSequence;
    private int _totalDamage;
    private float _lastShowTime;
    private Camera _mainCamera;

    void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
        _mainCamera = Camera.main;
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (_mainCamera != null)
        {
            transform.rotation=(_mainCamera.transform.rotation);
        }
    }

    public void ShowDamage(int damage ,bool isWeakPointDamage)
    {
        // 1. 真っ先にアクティブにし、以前の流れを完全に断ち切る
        gameObject.SetActive(true);
        if (_tmp == null) _tmp = GetComponent<TextMeshPro>();

        _activeSequence?.Kill();
        _tmp.DOKill();
        transform.DOKill(); 

        // 2. 表示内容のリセット
        if(Time.time >= _lastShowTime + _resetTime)
        {
            _totalDamage = 0;
        }
        _totalDamage += damage;
        _tmp.text =_totalDamage.ToString();
        _lastShowTime =Time.time;

        _tmp.alpha = 1.0f;
        _tmp.color = isWeakPointDamage ? Color.yellow : Color.red;
        
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.1f);

        // 3. 「待機 → フェードアウト → 非アクティブ化」という一連の流れ（Sequence）を予約する
        _activeSequence = DOTween.Sequence();
        _activeSequence.AppendInterval(_resetTime); // 指定時間待つ
        _activeSequence.Append(_tmp.DOFade(0, 0.5f)); // 0.5秒で透明になる
        _activeSequence.OnComplete(() => {
                                         gameObject.SetActive(false); // 終わったら消える、よ
                                         });
        _activeSequence.SetId(this).SetLink(gameObject); // 万が一オブジェクトが即消えてもエラーにならないように、ひも付け
    }
}