using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/PartAction")]
public class GunPartAction : ScriptableObject
{
    public Vector3 targetPos;       // 移動先（相対）
    public Vector3 targetRot;       // 回転先（相対）
    public float duration = 0.05f;  // 行きの時間
    public float returnTime = 0.1f; // 帰りの時間
    public Ease easeType = Ease.OutQuad;
    public bool isFinalShotCancel = false;

    // 実際の動きを実行するメソッド
    public void Execute(GunBase gun, Transform part, Vector3 defaultPos, Vector3 defaultRot)
    {
        if (part == null) return;

        part.DOKill();

        if (isFinalShotCancel && gun.currentAmmo == 0)
        {
            if (targetPos != Vector3.zero)
            {
                part.DOLocalMove(defaultPos + targetPos, duration).SetEase(easeType);
            }

            if (targetRot != Vector3.zero)
            {
                part.DOLocalRotate(defaultRot + targetRot, duration).SetEase(easeType);
            }
        }
        else
        {
            // --- 往路 ---
            if (targetPos != Vector3.zero)
                part.DOLocalMove(defaultPos + targetPos, duration).SetEase(easeType)
                    .OnComplete(() => part.DOLocalMove(defaultPos, returnTime));

            if (targetRot != Vector3.zero)
                part.DOLocalRotate(defaultRot + targetRot, duration).SetEase(easeType)
                .OnComplete(() => part.DOLocalRotate(defaultRot, returnTime));
        }
    }
}