using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/Action/shotGun")]
public class shotGunAction : shotAction
{ 
    public int palletCount = 8; // intの方がループには使いやすいです
    public float spreadAngle = 10.0f; // 拡散範囲（度数）

    public override void shot(GunBase baseGun)
    {
        Camera cam = baseGun.playerCamera;
        Vector3 startPos = baseGun.muzzlePoint != null 
                           ? baseGun.muzzlePoint.position 
                           : cam.transform.position;

        int mask = ~LayerMask.GetMask("Ignore Raycast", "invisibleWall");

        // --- ここからショットガン特有のループ処理 ---
        for (int i = 0; i < palletCount; i++)
        {
            // 1. レティクル中央（Viewport 0.5, 0.5）を取得
            Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));

            // 2. 弾をバラけさせる（カメラの前方向ベクトルを基準にランダム回転）
            // spreadAngleを半分にして、正負に散らす
            Quaternion spread = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            );
            
            // レイの方向を拡散させる
            Vector3 fireDirection = spread * ray.direction;

            Vector3 endPos;
            
            // 3. 拡散した方向へRaycast
            if (Physics.Raycast(startPos, fireDirection, out RaycastHit hit, 100f, mask, QueryTriggerInteraction.Collide))
            {
                endPos = hit.point;
                EnemyHP targetHealth = hit.collider.GetComponentInParent<EnemyHP>();

                if (targetHealth != null)
                {
                    // 弾丸1発ごとのダメージ処理
                    targetHealth.TakeDamage(baseGun.damage, hit.collider, baseGun.pManager); 
                    baseGun.pManager.crosshair.OnHit();
                }
            }
            else
            {
                endPos = startPos + (fireDirection * 100f);
                // ミス時のパッシブ発動（弾丸ごとではなく発射ごとにしたい場合はループの外へ）
                foreach(var p in baseGun.pManager.activePassives)
                {
                    p.OnMiss(baseGun.pManager);
                }
            }

            // 4. トレーサー（弾道）の生成
            if (baseGun.pManager.tracerPool != null)
            {
                GameObject t = baseGun.pManager.tracerPool.Get();
                LineRenderer lr = t.GetComponent<LineRenderer>();
                
                if (lr != null)
                {
                    lr.alignment = LineAlignment.View;
                    lr.startWidth = 0.08f; // ショットガンは少し細めが綺麗です
                    lr.endWidth = 0.08f; 
                    
                    lr.SetPosition(0, startPos);
                    lr.SetPosition(1, endPos);

                    DOTween.To(() => lr.startWidth, x => lr.startWidth = x, 0f, 0.05f);
                    DOTween.To(() => lr.endWidth, x => lr.endWidth = x, 0f, 0.08f)
                           .OnComplete(() => baseGun.pManager.tracerPool.ReturnToPool(t));
                }
            }
        }

        // カメラ反動は1回の射撃で1回だけ
        if (baseGun != null)
        {
             baseGun.ApplyCameraRecoil();
        }
    }
}