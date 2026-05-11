using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/reloadAnimation/SCARType")]
public class SCARTypeReload : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    public float baseTiltTime = 0.5f;
    public float baseDropTime = 0.4f;
    public float baseInsertTime = 0.5f;
    public float baseDropToInsertTime = 0.1f; 
    public float baseBoltMoveTime = 0.5f;
    public float baseHand_boltMoveTime = 0.5f;
    [Header("リロードモーション設定")]
    public Vector3 tiltAngle;
    public Vector3 dropDistance;
    public Vector3 spawnPoint; 
    public float shakeStrength;
    public Vector3 handMagazinePoint;
    public Vector3 tekubiHandMagazineRotation;
    [Header("ボルトを引くモーション設定")]
    public Vector3 boltHandPosition;
    public Vector3 boltHandPosition2;
    public Vector3 boltMoveDistance;
    public Vector3 tekubiBoltRotation;
    public float releaseTime = 0.05f; 
    public AudioClip magOutClip;    
    public AudioClip magInsertClip; 
    public AudioClip magHitClip;    
    public AudioClip boltClip;      
    public override void Play(GunBase gun)
    {
        bool isEmptyReload = false;
        if (gun.currentAmmo == 0)
        {
            isEmptyReload = true;
        }
        float speedMult = gun.GetTotalReloadSpeedMultiplier();
        float tiltTime = baseTiltTime * speedMult;
        float dropTime = baseDropTime * speedMult;
        float insertTime = baseInsertTime * speedMult;
        float dropToInsertTime = baseDropToInsertTime * speedMult;
        float boltMoveTime = baseBoltMoveTime * speedMult;
        float hand_boltMoveTime = baseHand_boltMoveTime * speedMult;
        Vector3 defaultGunRot = gun.gunModel.transform.localEulerAngles;
        Vector3 defaultArmPosition = gun.LeftArm.transform.localPosition;
        Vector3 defaultTekubiRot = gun.LeftTekubi.transform.localEulerAngles;
        Vector3 counterTekubiRot = defaultTekubiRot + tekubiHandMagazineRotation;
        Vector3 defaultBoltPos = gun.SpecialPart_1.transform.localPosition;
        Sequence seq = DOTween.Sequence();
        seq.Append(gun.gunModel.transform.DOLocalRotate(tiltAngle, tiltTime));
        seq.Join(gun.LeftArm.transform.DOLocalMove(handMagazinePoint, tiltTime));
        seq.Join(gun.LeftTekubi.transform.DOLocalRotate(counterTekubiRot, tiltTime));
        seq.AppendCallback(() => {
            gun.PlayReloadSound(magOutClip);    
            gun.currentAmmo = 0;
            gun.UpdateAmmoDisplay();
            Debug.Log("マガジン脱着");
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
        seq.Join(gun.LeftArm.transform.DOLocalMove(defaultArmPosition, tiltTime));
        reloadTime = seq.Duration();
        if(isEmptyReload)
        {
            seq.Append(gun.LeftArm.transform.DOLocalMove(boltHandPosition, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.Join(gun.LeftTekubi.transform.DOLocalRotate(tekubiBoltRotation, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.InsertCallback(seq.Duration() - 0.2f, () => { 
                gun.sources[1].PlayOneShot(boltClip); 
            });
            seq.Append(gun.LeftArm.transform.DOLocalMove(boltHandPosition2, boltMoveTime).SetEase(Ease.OutQuad));
            seq.Append(gun.SpecialPart_1.transform.DOLocalMove(boltMoveDistance, releaseTime).SetEase(Ease.InQuad));
            seq.Append(gun.LeftArm.transform.DOLocalMove(defaultArmPosition, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.Join(gun.LeftTekubi.transform.DOLocalRotate(defaultTekubiRot, hand_boltMoveTime).SetEase(Ease.OutCubic));
        }
        reloadTime = seq.Duration();
        seq.OnComplete(() => {
            gun.isReloading = false;
            Debug.Log("リロード完了。さあ、続きを楽しもうか");
        });
    }
}
