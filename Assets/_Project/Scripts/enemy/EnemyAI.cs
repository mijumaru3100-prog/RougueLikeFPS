using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using DG.Tweening;
public abstract class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Normal, Damaged, Dead }
    [Header("現在の状態")]
    public EnemyState currentState = EnemyState.Normal;
    [Header("共通設定")]
    public Transform target;
    protected NavMeshAgent agent;
    protected Animator anim;
    public float detectionRange = 20f;
    public GameObject body;
    [Header("射撃・スナイパー設定")]
    public GunBase gun;
    public float keepDistance = 10f;
    public LayerMask obstacleLayer;
    [Header("射撃設定")]
    public float fireRate = 0.5f;
    [Header("エイム設定")]
    public float aimHeightOffset = 0f;
    public float myEyeHeight = 1.5f;
    public float shootingAngleThreshold = 20f;
    protected float nextFireTime;
    protected MeshRenderer _renderer;
    protected MaterialPropertyBlock _propBlock;
    protected static readonly int _colorID = Shader.PropertyToID("_Color");
    protected Color _originalColor;
    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");
    protected static readonly int DamageHash = Animator.StringToHash("Damage");
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        anim = GetComponentInChildren<Animator>(); 
        if (gun == null) gun = GetComponentInChildren<GunBase>();
    }
    protected virtual void Update()
    {
        if (target == null || currentState != EnemyState.Normal) return;
        float dist = Vector3.Distance(transform.position, target.position);
        Vector3 aimTargetPos = target.position + Vector3.up * aimHeightOffset;
        bool canSee = CanSeePlayer(aimTargetPos); 
        HandleRotation();
        UpdateMovement(dist, canSee);
        if (anim != null)
        {
            float speedForAnim = 0f;
            if (agent.hasPath && agent.remainingDistance > 0.1f)
            {
                speedForAnim = GetAnimationSpeed(dist);
            }
            float currentParam = anim.GetFloat(SpeedHash);
            float smoothedSpeed = Mathf.Lerp(currentParam, speedForAnim, Time.deltaTime * 5f);
            anim.SetFloat(SpeedHash, smoothedSpeed);
        }
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        if (canSee && dist <= detectionRange && angle < shootingAngleThreshold)
        {
            if (Time.time >= nextFireTime)
            {
                if (gun != null) gun.tryFire(); 
                if (anim != null) anim.SetTrigger(AttackHash);
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    protected abstract void UpdateMovement(float dist, bool canSee);
    protected abstract float GetAnimationSpeed(float currentDist);
    public virtual void OnDamage()
    {
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Damaged;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (anim != null) anim.SetTrigger(DamageHash);
        if (body != null) body.transform.DOShakePosition(0.5f, new Vector3(0.1f, 0.1f, 0.1f), 10, 90f, true);
        StopCoroutine("RecoverFromDamage"); 
        StartCoroutine(RecoverFromDamage(0.5f));
    }
    protected virtual IEnumerator RecoverFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentState != EnemyState.Dead)
        {
            currentState = EnemyState.Normal;
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        }
    }
    protected bool CanSeePlayer(Vector3 targetUpperPos)
    {
        Vector3 eyePos = transform.position + Vector3.up * myEyeHeight;
        Vector3 direction = (targetUpperPos - eyePos).normalized;
        RaycastHit hit;
        int mask = ~LayerMask.GetMask("Ignore Raycast", "invisibleWall", "Enemy"); 
        Debug.DrawRay(eyePos, direction * detectionRange, Color.red);
        if (Physics.Raycast(eyePos, direction, out hit, detectionRange, mask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == target;
        }
        return false;
    }
    protected virtual void HandleRotation()
    {
        if (target == null) return;
        Vector3 aimTarget = target.position + Vector3.up * aimHeightOffset;
        if (gun != null) gun.transform.LookAt(aimTarget);
        bool isFleeing = IsFleeing();
        if (isFleeing || agent.velocity.sqrMagnitude < 0.2f)
        {
            Vector3 targetPos = target.position;
            targetPos.y = transform.position.y;
            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
            }
        }
        else
        {
            Vector3 moveDir = agent.velocity.normalized;
            moveDir.y = 0;
            if (moveDir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
            }
        }
    }
    protected virtual bool IsFleeing()
    {
        return false;
    }
    protected void Flee() 
    {
        Vector3 fleeDir = (transform.position - target.position).normalized;
        Vector3 targetPos = transform.position + fleeDir * 5f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas)) agent.destination = hit.position; 
    }
    protected virtual void LateUpdate()
    {
        if (body != null)
        {
            Vector3 pos = body.transform.localPosition;
            pos.x = 0; pos.z = 0;
            body.transform.localPosition = pos;
        }
    }
    public virtual void OnDie()
    {
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Dead;
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject, 0.1f);
    }
}