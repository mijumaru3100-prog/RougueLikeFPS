using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GunBase : MonoBehaviour
{
    public int baseMaxAmmo = 30;
    public int maxAmmo => Mathf.RoundToInt((baseMaxAmmo + stats.bonusMaxAmmo) * stats.maxAmmoMultiple);
    public int currentAmmo = 30;
    public float defaultRPM = 450;
    public float fireRate
    {
        get
        {
            float totalMult = stats.fireRateMultiple;
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives) totalMult *= p.GetFireRateMultiplier(pManager);
            }
            return (60f / defaultRPM) * totalMult;
        }
    }
    
    
    public int baseDamage = 1;
    public int damage
    {
        get
        {
            float totalMult = stats.damageMultiple;
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives) totalMult *= p.GetDamageMultiplier(pManager);
            }
            return Mathf.RoundToInt((baseDamage + stats.bonusDamage) * totalMult);
        }
    }
    public GameObject bulletPrefab;

    [Header("ヒート設定")]
    public float currentHeat = 0f;
    public float heatPerShot = 1f;    // 1発でどれくらい溜まるか
    public float coolDownRate = 4f;    // 1秒でどれくらい冷めるか

    [Header("UI設定")]
    public TextMeshProUGUI ammoText; 

    [Header("カメラ反動")]
    public float baseRecoilForce = 0.25f; 
    public float recoilForce
    {
        get
        {
            float totalMult = stats.recoilForceMultiple;
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives) totalMult *= p.GetRecoilMultiplier(pManager);
            }
            return baseRecoilForce * totalMult;
        }
    }
    public float baseAdsMagnification = 0.5f;
    public float adsMagnification => baseAdsMagnification * stats.adsMagnificationMultiple; 
    public float horizontalRecoilForce = 0.1f;

    public float GetTotalReloadSpeedMultiplier()
    {
        float totalMult = stats.reloadSpeedMultiple;
        if (pManager != null)
        {
            foreach (var p in pManager.activePassives) 
                totalMult *= p.GetReloadSpeedMultiplier(pManager);
        }
        return totalMult;
    }

    [Header("銃反動")]    
    public Vector3 posRecoil = new Vector3(0, 0, -0.1f);
    public Vector3 adsPosRecoil = new Vector3(0, 0, -0.1f);
    public Vector3 rotRecoil = new Vector3(5f, 2f, 0); // x:垂直, y:水平ランダム幅//
    public float gunRotation_adsMagnification = 0.8f; // 覗き込み時の倍率
    public float animDuration = 0.05f;

    float lastFireTime;

    [Header("設定")]
    public PlayerManager pManager;
    public Camera playerCamera;
    public Transform adsPivot;
    public Transform recoilPivot;
    public CrosshairController crosshair;
    
    [Header("銃パーツのアニメーションリスト")]
    public List<GunPartSet> gunPartSets = new List<GunPartSet>();

    [Header("薬莢排出設定")]
    public Transform ejectPoint;      
    public Vector2 ejectForceRange = new Vector2(3f, 5f);
    public Vector2 ejectTorqueRange = new Vector2(10f, 20f);

    [Header("リロードのための武器パーツ")]
    public Transform gunModel;
    public Transform muzzlePoint;
    public Transform magazinePoint;
    public GameObject magazineObject;
    public GameObject LeftArm;
    public GameObject LeftShoulder;
    public GameObject LeftTekubi;
    public GameObject SpecialPart_1;

    [Header("プラグイン")]
    public shotMode shotMode;
    public shotAction shotAction; 
    public recoilAnimation recoilAnimation;
    public reloadAnimation reloadAnimation; 
    public PlayerStats stats;


    [Header("音設定")]
    [SerializeField] public AudioSource[] sources;
    [SerializeField] private AudioClip[] ShotSounds;
    //[SerializeField, Range(0, 1)] private float volume = 0.5f;
    [SerializeField, Range(0, 2)] private float pitchRandomness = 0.1f;

    [Header("----------空撃ち設定----------")]
    public AudioClip dryFireSound;

    [Header("----------残弾演出設定----------")]
    [SerializeField] private AudioClip kinKinSound;

    [Header("キンキン演出の調整（ミキサー）")]
    [Range(0f, 1f)] 
    public float changeStartThreshold = 0.3f; // いつから始める？（0.3 = 残り30%）

    [Range(0.5f, 2.0f)] 
    public float maxPitchShift = 1.1f;        // 最後の1発のピッチはどこまで上げる？

    [Range(0f, 1f)] 
    public float maxKinKinVolume = 0.8f;      // キンキン音の最大音量は？

    [Range(0f, 0.05f)]
    public float kinKinDelay = 0.02f; // これをインスペクターでいじる

    [Header("マズルフラッシュ設定（ライト式）")]
    public Light muzzleFlashLight; // マズルフラッシュ用のPoint Light
    public float flashDuration = 0.05f; // 閃光が光っている時間（ごく短く）
    public float maxIntensity = 15f;  // 最大の眩しさ

    [Header("覗き込み（ADS）設定")]
    public Vector3 adsPosition;
    public float adsSpeed = 0.1f;  // 覗き込む速さ（秒）
    public float adsFieldOfView = 40f;
    public Vector3 adsRotation;

    [Header("銃取得時の配置設定")]
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;

    private Vector3 defaultGunPosition;
    private Vector3 defaultGunRotation;
    private float defaultFOV;
    public bool isAiming = false;
    public bool isReloading = false;


    [SerializeField]private bool isNPC = false;
    [SerializeField]private bool useMuzzleFlashLight = true;
    

    void Start()
    {
        if(isNPC)
        {
            if (pManager == null)
            {
                pManager = GameObject.FindObjectOfType<PlayerManager>();
            }
            return;
        }

        isReloading = false;
        currentAmmo = maxAmmo;
        crosshair = pManager.crosshair;

        defaultGunPosition = adsPivot.localPosition;
        defaultGunRotation = adsPivot.localEulerAngles;
        foreach (var set in gunPartSets) set.Init();

        if (playerCamera == null) 
        {
            playerCamera = Camera.main;
        }

        if (playerCamera != null)
        {
            defaultFOV = playerCamera.fieldOfView;
        }


        UpdateAmmoDisplay();
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        if(isNPC){return;}

        
        if (shotMode != null && shotMode.IsFiring())
        {
           tryFire();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(tryReload());
        }

        HandleADS();

        if (currentHeat > 0) 
        {
            currentHeat = Mathf.Max(0, currentHeat - coolDownRate * Time.deltaTime);
        }
    }


    public void tryFire()
    {
        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            if (dryFireSound != null)
            {
                sources[0].pitch = 1.2f; 
                sources[0].PlayOneShot(dryFireSound);
            }

            StartCoroutine(tryReload());
            return;
        }
        
        if(Time.time < lastFireTime + fireRate)
        {
            return;
        }
        fire();
    }


    void fire()
    {
        lastFireTime = Time.time;
        
        if (shotAction == null && isNPC == false) 
        {
            Debug.Log("shotActionが未設定です。");
            return;
        }

        currentAmmo --;
        shotAction.shot(this);
        currentHeat += heatPerShot;
        PlayShotSound();
        PlayMuzzleFlash();
        foreach (var set in gunPartSets)
        {
            if (set.actionData != null)
            {
                set.actionData.Execute(this,set.partTransform, set.defaultPos, set.defaultRot);
            }
        }
        
        if(isNPC == false)
        {
            UpdateAmmoDisplay();
            ApplyGunRecoil();
            foreach (var p in pManager.activePassives)
            {
                p.OnShot(pManager);
            }
        }
        

        EjectShell();

        if (crosshair != null)
        {
            crosshair.AddSpread(10f); 
        }

    }

    public void ApplyGunRecoil()
    {
        if (recoilAnimation == null || gunModel == null || isNPC) return;

        float horizontalRecoil = Random.Range(-rotRecoil.y,rotRecoil.y);

        // 「偏り」を検知して補正する
        float currentY = gunModel.localEulerAngles.y;
        if (currentY > 180) currentY -= 360; 
        float bias = currentY * 0.5f; 
        horizontalRecoil -= bias; 

        Vector3 finalRot = new Vector3(rotRecoil.x, horizontalRecoil, 0);
        Vector3 finalPos = isAiming ? adsPosRecoil : posRecoil;

        if (isAiming)
        {
            finalRot *= gunRotation_adsMagnification;
        }

        recoilAnimation.Play(recoilPivot, finalPos, finalRot, animDuration);
    }

    public void ApplyCameraRecoil()
    {
        if(isNPC)return;

        var mouseLook = playerCamera.GetComponent<MouseLook>();
        if (mouseLook != null)
        {
            float fX = recoilForce;
            float fY = horizontalRecoilForce;
            if (isAiming)
            {
                fX *= adsMagnification;
                fY *= adsMagnification;
            }
            mouseLook.AddRecoil(fX, fY);
        }
    }
    
    private int currentIndex = 0;
    public void PlayShotSound()
    {
        if (sources == null || sources.Length == 0) return;
        if (ShotSounds == null || ShotSounds.Length == 0) return;

        var source = sources[currentIndex];
        AudioClip clipToPlay = ShotSounds[Random.Range(0, ShotSounds.Length)];

        float ammoRatio = (float)currentAmmo / maxAmmo;
        float dynamicPitch = 1.0f;
        float kinKinVolume = 0f;

        if (ammoRatio < changeStartThreshold)
        {
            float effectProgress = 1.0f - (ammoRatio / changeStartThreshold);
            dynamicPitch = Mathf.Lerp(1.0f, maxPitchShift, effectProgress);
            kinKinVolume = effectProgress * maxKinKinVolume;
        }

        source.clip = clipToPlay;
        source.pitch = (1.0f + Random.Range(-pitchRandomness, pitchRandomness)) * dynamicPitch;

        currentIndex = (currentIndex + 1) % sources.Length;

        if (kinKinVolume > 0 && kinKinSound != null)
        {
            StartCoroutine(DelayedKinKin(source, kinKinVolume));
        }
    }

    private IEnumerator DelayedKinKin(AudioSource targetSource, float volume)
    {
    yield return new WaitForSeconds(kinKinDelay);
    targetSource.PlayOneShot(kinKinSound, volume);
    }

    public void PlayReloadSound(AudioClip clip)
    {
    if (clip == null || sources == null || sources.Length == 0) return;
    
    sources[0].pitch = 1.0f;
    sources[0].PlayOneShot(clip, 1.0f);
    }

    void EjectShell()
    {
        if(pManager.shellPool == null ||ejectPoint == null)
        {
            Debug.Log("薬莢の排出設定が未完了です。");
            return;
        }

        GameObject shell = pManager.shellPool.Get();
        shell.transform.position = ejectPoint.position;
        shell.transform.rotation = ejectPoint.rotation;
        Rigidbody rb = shell.GetComponent<Rigidbody>();
       
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Vector3 force = ejectPoint.right * Random.Range(ejectForceRange.x, ejectForceRange.y) 
                        + ejectPoint.up * Random.Range(1f, 2f);
            
            rb.AddForce(force, ForceMode.Impulse);
       }

        StartCoroutine(ReturnShellAfterTime(shell, 3.0f));
    }

    private IEnumerator ReturnShellAfterTime(GameObject shell, float delay)
    {
        yield return new WaitForSeconds(delay);
        pManager.shellPool.ReturnToPool(shell);
    }

    void PlayMuzzleFlash()
    {
        if (muzzleFlashLight == null) return;
        if (useMuzzleFlashLight == false) return;

        muzzleFlashLight.enabled = true;
        muzzleFlashLight.intensity = maxIntensity;

        StartCoroutine(ExtinguishFlash());
    }

    private IEnumerator ExtinguishFlash()
    {
        yield return new WaitForSeconds(flashDuration);
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
        }
   }


   void HandleADS()
   {
        if(isNPC){return;}

        if(isReloading)
        {
            // リロード開始時にADSを解除
            if (isAiming)
            {
                isAiming = false;
                crosshair.SetVisible(true);
                PlayADSTween(defaultGunPosition, defaultFOV, defaultGunRotation);
            }
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            crosshair.SetVisible(false);
            PlayADSTween(adsPosition, adsFieldOfView,adsRotation);
        }
    
        if (Input.GetMouseButtonUp(1))
        {
            crosshair.SetVisible(true);
            isAiming = false;
            PlayADSTween(defaultGunPosition, defaultFOV,defaultGunRotation);
        }
    }

    void PlayADSTween(Vector3 targetPos, float targetFOV,Vector3 targetLotate)
    {
        adsPivot.DOLocalMove(targetPos, adsSpeed).SetEase(Ease.OutQuad);
        adsPivot.DOLocalRotate(targetLotate, adsSpeed).SetEase(Ease.OutQuad);
    
        playerCamera.DOFieldOfView(targetFOV, adsSpeed).SetEase(Ease.OutQuad);
    }

    


    IEnumerator tryReload()
    {
        if(isReloading) yield break;
        
        isReloading = true;

        if(reloadAnimation && isNPC == false)
        {
            // アニメーション再生（内部で弾数リセット等を行う）
            reloadAnimation.Play(this);

            yield return new WaitForSeconds(reloadAnimation.reloadTime);
            foreach (var p in pManager.activePassives)
            {
                p.OnReloadComplete(pManager);
            }
        }
        else
        {
            yield return new WaitForSeconds(3f);
            currentAmmo = maxAmmo;
            isReloading = false;

            
            if(isNPC == false)
            {
                UpdateAmmoDisplay();
                foreach (var p in pManager.activePassives)
                {
                p.OnReloadComplete(pManager);
                }
            }
        }
    }

    public void UpdateAmmoDisplay()
    {
        if(isNPC){return;}

        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";

            if (currentAmmo <= maxAmmo * 0.1)
            {
                ammoText.color = Color.red; 
            }
            else if (currentAmmo <= maxAmmo * 0.3f)
            {
                ammoText.color = Color.yellow;
            }
            else 
            {
                ammoText.color = Color.white;
            }
        }
    }
}