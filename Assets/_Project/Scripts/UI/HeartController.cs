using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    // 自分が持っている「中身の画像」をインスペクターで紐付ける用
    public Image fillImage;

    // 外（PlayerHP）から「0.0〜1.0」の数字をもらって、見た目を変える関数
    public void SetHeart(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
        }
    }
}