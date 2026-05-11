using UnityEngine;
using DG.Tweening; // DOTweenをインポート

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Parts")]
    [SerializeField] private RectTransform topLine;
    [SerializeField] private RectTransform bottomLine;
    [SerializeField] private RectTransform leftLine;
    [SerializeField] private RectTransform rightLine;

    [Header("Settings")]
    [SerializeField] private float minSpread = 10f;
    [SerializeField] private float maxSpread = 100f;
    [SerializeField] private float returnDuration = 0.2f; // 戻るまでの時間
    [SerializeField] private Ease returnEase = Ease.OutQuad; // 戻り方のキレ

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup; // 透明度で消す場合に便利

    private float currentSpread;
    private Tween spreadTween; // 実行中のアニメーションを保持

    [Header("Hit Marker")]
    [SerializeField] private CanvasGroup hitMarkerCanvasGroup; // ヒットマーカーのCanvasGroup
    [SerializeField] private float hitEffectDuration = 0.15f; // 表示されている時間
    [SerializeField] private Vector3 hitScaleEffect = new Vector3(1.2f, 1.2f, 1f); // ヒット時の揺れ

    private Tween hitTween;

    void Start()
    {
        currentSpread = minSpread;
        UpdateCrosshairPosition(minSpread);

        if (hitMarkerCanvasGroup != null) hitMarkerCanvasGroup.alpha = 0f;
    }

    // これを射撃スクリプトから呼ぶ
    public void AddSpread(float amount)
    {
        // 1. 現在の広がりに加算（最大値を超えないように）
        float targetSpread = Mathf.Min(maxSpread, currentSpread + amount);
        
        // 2. 実行中のアニメーションがあれば止める
        spreadTween.Kill();

        // 3. DOTweenで値を変化させる
        // 「currentSpreadをtargetSpreadまで一瞬で上げ、その後minSpreadまで戻す」動き
        currentSpread = targetSpread;
        
        spreadTween = DOTween.To(() => currentSpread, x => {
            currentSpread = x;
            UpdateCrosshairPosition(currentSpread);
        }, minSpread, returnDuration)
        .SetEase(returnEase);
    }

    private void UpdateCrosshairPosition(float spread)
    {
        topLine.anchoredPosition = new Vector2(0f, spread);
        bottomLine.anchoredPosition = new Vector2(0f, -spread);
        leftLine.anchoredPosition = new Vector2(-spread, 0f);
        rightLine.anchoredPosition = new Vector2(spread, 0f);
    }


    // 表示・非表示を切り替えるメソッド
    public void SetVisible(bool visible, float duration = 0.1f)
    {
        if (canvasGroup == null) return;

        // DOTweenでふわっと消したり出したりする
        canvasGroup.DOFade(visible ? 1f : 0f, duration);
    }

    public void OnHit()
    {
        if (hitMarkerCanvasGroup == null) return;

        // 実行中のアニメーションをリセット
        hitTween.Kill();
        hitMarkerCanvasGroup.alpha = 1f;
        hitMarkerCanvasGroup.transform.localScale = hitScaleEffect;

        // 1. 透明度を0に戻す
        // 2. スケールを1に戻す（ちょっとした反動演出）
        Sequence hitSeq = DOTween.Sequence();
        hitSeq.Append(hitMarkerCanvasGroup.DOFade(0f, hitEffectDuration).SetEase(Ease.InExpo));
        hitSeq.Join(hitMarkerCanvasGroup.transform.DOScale(1f, hitEffectDuration));
    
        hitTween = hitSeq;
    }
}