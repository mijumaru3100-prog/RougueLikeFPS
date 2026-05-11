using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/recoilAnimation/SimpleRecoilAnimation")]
public  class simpleRecoilAnimation : recoilAnimation 
{
public override void Play(Transform target, Vector3 posAmount, Vector3 rotAmount, float duration)
{
    Sequence s = DOTween.Sequence();
    float flip =1f;
    if (Random.value > 0.5f)
    {
        flip =-1f;
    }
    Vector3 flippedPos = new Vector3(posAmount.x * flip, posAmount.y, posAmount.z);
    Vector3 flippedRot = new Vector3(rotAmount.x, rotAmount.y * flip, rotAmount.z * flip);
    s.Join(target.DOLocalMove(flippedPos, duration).SetEase(Ease.OutQuad));
    s.Join(target.DOLocalRotate(flippedRot, duration).SetEase(Ease.OutQuad));
    s.Append(target.DOLocalMove(Vector3.zero, duration * 2f));
    s.Join(target.DOLocalRotate(Vector3.zero, duration * 2f));
}
}