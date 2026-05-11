using UnityEngine;
using UnityEngine.UI;
public class HeartController : MonoBehaviour
{
    public Image fillImage;
    public void SetHeart(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
        }
    }
}