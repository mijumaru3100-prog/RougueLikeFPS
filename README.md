
# RougueLikeFPS

# ゲーム概要

「Enter The Gungeon」「Slay the Spire」から着想を得て制作したFPSゲームです。ノード型のダンジョンをステータス上昇やさまざまな銃とパッシブアイテムを組み合わせたビルドで攻略していきます。特に銃やパッシブの拡張性の高さと操作の手触りに力を入れて制作しています。

## 力を入れた点
## ①銃を撃つ楽しさ
ゲームをプレイして一番楽しい時は自分の操作の報酬が返ってきた時です。そのため、銃を撃った時の演出や弾が命中したときの報酬を華やかに、たくさん用意したいと考えています。現在実装している要素とこれから実装する予定の要素を羅列していきます。
[GunBase.cs](Assets/_Project/Scripts/Weapons/Gunbase.cs)
### 現在実装している要素
- 銃とカメラの二段階の反動
- ポイントライトとパーティクルを用いたマズルフラッシュ
- 銃パーツを動作・回転させる機能（複数組み合わせ可能）
- 数秒間の合計ダメージ表示
- ヒット時の敵アニメーションとフラッシュ
### 実装予定の機能
- ヒットサウンド、ヘッドショットサウンド
<img width="600" height="336" alt="ezgif-1e229d45935121c6" src="https://github.com/user-attachments/assets/3f9c4116-3f32-4a9d-bf56-65bb9fda84c5" />

## ②さまざまな効果のパッシブアイテム
「Enter The Gungeon」「Slay the Spire」の大きな特徴として個性的な効果を持つパッシブアイテムが挙げられます。このゲームでもそのようなアイテムを再現するためにイベントフック式でパッシブアイテムを呼び出す仕組みを作りました。以下のように特定の行動をしたとき登録されたパッシブを一括で呼び出します。[GunBase.cs](Assets/_Project/Scripts/Weapons/Gunbase.cs)
```csharp
    void Fire()
    {
        lastFireTime = Time.time;

        currentAmmo--;
        shotAction.shot(this);
        foreach (var passive in pManager.activePassives)
        {
            passive.OnShot(pManager);
        }
}
```
パッシブ側は以下のようなフックを持っています。[PassiveEffect.cs](Assets/_Project/Scripts/passive/PassiveEffect.cs)
```csharp
public virtual void OnShot(PlayerManager manager) { }
public virtual void OnReloadComplete(PlayerManager manager) { }
public virtual void OnKillEnemy(PlayerManager manager) { }
public virtual void OnTakeDamage(PlayerManager manager, float damage) { }
他にもたくさんあります
```
## ③動的なバフ変動とScriptableObjectによるバフ管理
このゲームはパッシブ効果やステータスの購入によって武器の性能が大きく変わります。特にFireRateやDamageなどリアルタイムの影響が大きい場合は以下のような計算方法を行っています。[GunBase.cs](Assets/_Project/Scripts/Weapons/Gunbase.cs)
```csharp
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
```
②と似た原理で管理しています。
リアルタイム変化の影響が少ない数値は以下のようになっています。[GunBase.cs](Assets/_Project/Scripts/Weapons/Gunbase.cs)
```csharp
public int maxAmmo => Mathf.RoundToInt((baseMaxAmmo + stats.bonusMaxAmmo) * stats.maxAmmoMultiple);
```






