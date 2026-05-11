using UnityEngine;
using DG.Tweening;

public class ElevatorDoor : MonoBehaviour
{
    public Vector3 openPositionOffset; 
    public Vector3 openScale = new Vector3(0.1f, 1f, 1f); 
    public float duration = 0.8f; 

    private Vector3 _initialPosition;
    private Vector3 _initialScale;
    private bool _isOpen = false;

    void Awake()
    {
        _initialPosition = transform.localPosition;
        _initialScale = transform.localScale;
    }

    public void Open()
    {
        if(_isOpen) return;
        _isOpen = true;

        // 一旦すべてのTweenを止めてから実行（連続クリック対策）
        transform.DOKill(); 

        // 移動とスケールを同時に実行
        // Relative(true)を使うと、現在の位置からの相対値で動かせるので計算ミスが減ります
        transform.DOLocalMove(_initialPosition + openPositionOffset, duration).SetEase(Ease.OutCubic);
        transform.DOScale(openScale, duration).SetEase(Ease.OutCubic);
    }

    public void Close()
    {
        if(!_isOpen) return;
        _isOpen = false;

        transform.DOKill();

        transform.DOLocalMove(_initialPosition, duration).SetEase(Ease.InCubic);
        transform.DOScale(_initialScale, duration).SetEase(Ease.InCubic);
    }
}