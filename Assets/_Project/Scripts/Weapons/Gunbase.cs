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
    public Transform adsPivot;      // 旧 handpoint (ADSの移動用)
    public Transform recoilPivot;   // 新 handpoint2 (反動・アニメ用)
    public CrosshairController crosshair;
    
    [Header("銃パーツのアニメーションリスト")]
    public List<GunPartSet> gunPartSets = new List<GunPartSet>();

    [Header("薬莢排出設定")]
    public Transform ejectPoint;      // 排出口（エジェクションポート）の位置
    public Vector2 ejectForceRange = new Vector2(3f, 5f); // 飛ばす力の強さ
    public Vector2 ejectTorqueRange = new Vector2(10f, 20f); // 回転のランダム性

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
    public AudioClip dryFireSound; // 「カチッ」という小さな音

    [Header("----------残弾演出設定----------")]
    [SerializeField] private AudioClip kinKinSound; // キンキンする高い金属音

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
    public Light muzzleFlashLight; // さっき作った Point Light を登録して、ね
    public float flashDuration = 0.05f; // 閃光が光っている時間（ごく短く）
    public float maxIntensity = 15f;  // 最大の眩しさ

    [Header("覗き込み（ADS）設定")]
    public Vector3 adsPosition;    // 覗き込んだ時のローカル座標
    public float adsSpeed = 0.1f;  // 覗き込む速さ（秒）
    public float adsFieldOfView = 40f; // 覗き込んだ時の視野角（少しズームする、よ）
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

        // カメラが設定された（または既にあった）後に、必ず元の視野角を保存するようにします
        if (playerCamera != null)
        {
            defaultFOV = playerCamera.fieldOfView;
        }


        UpdateAmmoDisplay();
    }

    void Update()
    {
        if (Time.timeScale == 0) return; // ← ポーズ中は処理しない

        if(isNPC){return;}

　　　　　// 1. 魂(Mode)に「今、撃つべき？」と問いかける
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

    // このスクリプトのUpdateから呼び出される、よ
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
             // 空撃ち音はピッチを少しだけ高くすると、乾いた感じが出るんだ、よ
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

    // このスクリプトのtryFireから呼び出される、よ
    void fire()
    {
        lastFireTime = Time.time;
        
        if (shotAction == null && isNPC == false) 
        {
            Debug.Log("shotactionが未設定...だ、よ。ままならないね...");
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
        // CameraRecoilは現在はshotAction(normalShot)から呼ばれる形を維持するが、
        // 関数自体はここに用意して一貫性を持たせる、よ
        

        EjectShell();

        if (crosshair != null)
        {
            // 撃つたびにレティクルを 10 ずつ広げる！
            crosshair.AddSpread(10f); 
        }

    }

    // 銃本体の見た目の反動を制御する、よ
    public void ApplyGunRecoil()
    {
        if (recoilAnimation == null || gunModel == null || isNPC) return;

        // 1. 次の反動ベクトル（回転）を計算する
        // 基本のランダムな反動
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

        // 2. アニメーション実行
        recoilAnimation.Play(recoilPivot, finalPos, finalRot, animDuration);
    }

    // カメラ（視点）の跳ね上がりを制御する、よ
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

        // --- 残弾演出の計算 ---
        float ammoRatio = (float)currentAmmo / maxAmmo;
        float dynamicPitch = 1.0f;
        float kinKinVolume = 0f;

        if (ammoRatio < changeStartThreshold)
        {
            float effectProgress = 1.0f - (ammoRatio / changeStartThreshold);
            // 弾が減るほど、dynamicPitch は 1.0 から maxPitchShift へ近づく、よ
            dynamicPitch = Mathf.Lerp(1.0f, maxPitchShift, effectProgress);
            kinKinVolume = effectProgress * maxKinKinVolume;
        }

        // --- 再生実行 ---
        source.clip = clipToPlay;
        
        // 基本のランダム性に、計算した dynamicPitch を掛け合わせるんだ、ね
        source.pitch = (1.0f + Random.Range(-pitchRandomness, pitchRandomness)) * dynamicPitch;
        
        source.Play();

        // 次の演者へ
        currentIndex = (currentIndex + 1) % sources.Length;

        // キンキン音の重層化
        if (kinKinVolume > 0 && kinKinSound != null)
        {
            StartCoroutine(DelayedKinKin(source, kinKinVolume));
        }
    }

    // キンキン音専用の「遅延実行」コルーチン
    private IEnumerator DelayedKinKin(AudioSource targetSource, float volume)
    {
    yield return new WaitForSeconds(kinKinDelay);
    
    // メインの音が鳴っている最中に、後から「カキンッ！」と重ねる
    targetSource.PlayOneShot(kinKinSound, volume);
    }

    public void PlayReloadSound(AudioClip clip)
    {
    if (clip == null || sources == null || sources.Length == 0) return;
    
    sources[0].pitch = 1.0f; // リロードは正確な音で。
    sources[0].PlayOneShot(clip, 1.0f); // 音量は1.0でしっかり聴かせる、よ
    }

    void EjectShell()
    {
        if(pManager.shellPool == null ||ejectPoint == null)
        {
            Debug.Log("薬莢の排出がままならないね...");
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
            // 2. 右斜め上あたりに、少しランダムな力を加える……
            Vector3 force = ejectPoint.right * Random.Range(ejectForceRange.x, ejectForceRange.y) 
                        + ejectPoint.up * Random.Range(1f, 2f);
            
            rb.AddForce(force, ForceMode.Impulse);
       }

        // 4. いつまでも残っていると、メモリが「ぐるぐる」しちゃうから。
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

        // 1. 閃光の「誕生」。一瞬だけ、最大出力で世界を照らす……
        muzzleFlashLight.enabled = true;
        muzzleFlashLight.intensity = maxIntensity;

        // 2. 閃光の「死」。0.05秒後に、光を消し去る残酷な魔法（コルーチン）を放つ。
        StartCoroutine(ExtinguishFlash());
    }

    // 閃光を消し去るための、遅延実行（コルーチン）
    private IEnumerator ExtinguishFlash()
    {
        // ごく短い「命の時間」を待って……
        yield return new WaitForSeconds(flashDuration);
    
        // ライトを黙らせる（無音への回帰）。
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
        }
   }

   // Update() の中で入力を監視して、ね
   void HandleADS()
   {
        if(isNPC){return;}

        if(isReloading)
        {
            // リロードが始まった瞬間に「狙い込み中」だったら、1回だけ解除処理を呼ぶんだ、よ
            if (isAiming)
            {
                isAiming = false;
                crosshair.SetVisible(true);
                PlayADSTween(defaultGunPosition, defaultFOV, defaultGunRotation);
            }
            return;
        }

        // 右クリックを押している間は「覗き込む」
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            crosshair.SetVisible(false);
            PlayADSTween(adsPosition, adsFieldOfView,adsRotation);
        }
    
        // 離せば「腰だめ」に戻る
        if (Input.GetMouseButtonUp(1))
        {
            crosshair.SetVisible(true);
            isAiming = false;
            PlayADSTween(defaultGunPosition, defaultFOV,defaultGunRotation);
        }
    }

    void PlayADSTween(Vector3 targetPos, float targetFOV,Vector3 targetLotate)
    {
        // 銃の位置を「しなやか」に動かす
        adsPivot.DOLocalMove(targetPos, adsSpeed).SetEase(Ease.OutQuad);
        adsPivot.DOLocalRotate(targetLotate, adsSpeed).SetEase(Ease.OutQuad);
    
        // カメラの視野角も変えて、集中力を表現するんだ、ね
        playerCamera.DOFieldOfView(targetFOV, adsSpeed).SetEase(Ease.OutQuad);
    }

    

    // このスクリプトのtryFireから呼び出される、よ
    IEnumerator tryReload()
    {
        if(isReloading) yield break;
        
        isReloading = true;
        // 基本のリロード時間に、ステータスの倍率をちゃんとかけてあげようね。

        if(reloadAnimation && isNPC == false)
        {
            //アニメーションの中で「currentAmmo = maxAmmo」と「isReloding=false」する、よ
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

    //弾が変化した時に呼び出される、ね...
    public void UpdateAmmoDisplay()
    {
        if(isNPC){return;}

        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";

            if (currentAmmo <= maxAmmo * 0.1)
            {
                // 弾切れ……絶望の「赤」
                ammoText.color = Color.red; 
            }
            else if (currentAmmo <= maxAmmo * 0.3f)
            {
                // 残り3割……警告の「黄色」
                ammoText.color = Color.yellow;
            }
            else 
            {
                // まだ余裕がある……平穏な「白」
                ammoText.color = Color.white;
            }
        }
    }
}