using UnityEngine;

    
    [System.Serializable]
    public class GunPartSet
    {
        public Transform partTransform;  // 動かす実体
        public GunPartAction actionData; // どの動きをさせるか

        [HideInInspector] public Vector3 defaultPos;
        [HideInInspector] public Vector3 defaultRot;

        public void Init()
        {
            if (partTransform == null) return;
            defaultPos = partTransform.localPosition;
            defaultRot = partTransform.localEulerAngles;
        }
    }