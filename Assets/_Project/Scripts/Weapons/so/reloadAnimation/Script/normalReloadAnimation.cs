using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/reloadAnimation/normal")]
public class normalReloadAnimation : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    public float baseTiltTime = 0.5f;
    public float baseDropTime = 0.4f;
    public float baseInsertTime = 0.5f;
    public float baseDropToInsertTime = 0.1f; 
    [Header("リロードモーション設定")]
    public Vector3 tiltAngle;
    public Vector3 dropDistance;
    public Vector3 spawnPoint; 
    public float shakeStrength;
    public Vector3 handMagazinePoint;
    public Vector3 tekubiHandMagazineRotation;
    public AudioClip magOutClip;    
    public AudioClip magInsertClip; 
    public AudioClip magHitClip;    
    public override void Play(GunBase gun)
    {
        float speedMult = gun.GetTotalReloadSpeedMultiplier();
        float tiltTime = baseTiltTime * speedMult;
        float dropTime = baseDropTime * speedMult;
        float insertTime = baseInsertTime * speedMult;
        float dropToInsertTime = baseDropToInsertTime * speedMult;
        Vector3 defaultGunRot = gun.gunModel.transform.localEulerAngles;
        Vector3 defaultHandPosition = gun.LeftArm.transform.localPosition;
        Vector3 defaultTekubiRot = gun.LeftTekubi.transform.localEulerAngles;
        Vector3 counterTekubiRot = defaultTekubiRot + tekubiHandMagazineRotation;
        Sequence seq = DOTween.Sequence();
        seq.Append(gun.gunModel.transform.DOLocalRotate(tiltAngle, tiltTime));
        seq.Join(gun.LeftArm.transform.DOLocalMove(handMagazinePoint, tiltTime));
        seq.Join(gun.LeftTekubi.transform.DOLocalRotate(counterTekubiRot, tiltTime));
        seq.AppendCallback(() => {
            gun.PlayReloadSound(magOutClip);    
            gun.currentAmmo = 0;
            gun.UpdateAmmoDisplay();
            Debug.Log("マガジンを脱着");
            gun.LeftArm.transform.SetParent(gun.magazineObject.transform, true);
        });
        seq.Append(gun.magazineObject.transform.DOLocalMove(dropDistance, dropTime).SetEase(Ease.InBack, 1f));
        seq.Append(gun.magazineObject.transform.DOLocalMove(spawnPoint, dropToInsertTime));
        seq.Append(gun.magazineObject.transform.DOLocalMove(gun.magazinePoint.localPosition, insertTime).SetEase(Ease.OutBack, 0.75f));
        float totalDurationStage1 = seq.Duration();
        float magInsertTiming = Mathf.Max(0, totalDurationStage1 - 0.7f * speedMult); 
        seq.InsertCallback(magInsertTiming, () => { gun.sources[1].PlayOneShot(magInsertClip); });
        float magHitTiming = Mathf.Max(0, totalDurationStage1 - 0.65f * speedMult); 
        seq.InsertCallback(magHitTiming, () => { gun.sources[2].PlayOneShot(magHitClip); });
        seq.AppendCallback(() => {
            gun.LeftArm.transform.SetParent(gun.LeftShoulder.transform, true);
            gun.currentAmmo = gun.maxAmmo;
            gun.UpdateAmmoDisplay();
        });
        seq.Append(gun.gunModel.transform.DOLocalRotate(defaultGunRot, tiltTime));
        seq.Join(gun.LeftTekubi.transform.DOLocalRotate(defaultTekubiRot, tiltTime));
        seq.Join(gun.LeftArm.transform.DOLocalMove(defaultHandPosition, tiltTime));
        reloadTime = seq.Duration();
        seq.OnComplete(() => {
            gun.isReloading = false;
            Debug.Log("リロード完了。さあ、続きを楽しもうか");
        });
    }
}
