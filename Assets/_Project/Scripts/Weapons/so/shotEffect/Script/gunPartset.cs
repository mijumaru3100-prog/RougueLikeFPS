using UnityEngine;
    [System.Serializable]
    public class GunPartSet
    {
        public Transform partTransform;  
        public GunPartAction actionData; 
        [HideInInspector] public Vector3 defaultPos;
        [HideInInspector] public Vector3 defaultRot;
        public void Init()
        {
            if (partTransform == null) return;
            defaultPos = partTransform.localPosition;
            defaultRot = partTransform.localEulerAngles;
        }
    }