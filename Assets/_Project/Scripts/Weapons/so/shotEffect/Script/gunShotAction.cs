using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/PartAction")]
public class GunPartAction : ScriptableObject
{
    public Vector3 targetPos;       
    public Vector3 targetRot;       
    public float duration = 0.05f;  
    public float returnTime = 0.1f; 
    public Ease easeType = Ease.OutQuad;
    public bool isFinalShotCancel = false;
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
            if (targetPos != Vector3.zero)
                part.DOLocalMove(defaultPos + targetPos, duration).SetEase(easeType)
                    .OnComplete(() => part.DOLocalMove(defaultPos, returnTime));
            if (targetRot != Vector3.zero)
                part.DOLocalRotate(defaultRot + targetRot, duration).SetEase(easeType)
                .OnComplete(() => part.DOLocalRotate(defaultRot, returnTime));
        }
    }
}