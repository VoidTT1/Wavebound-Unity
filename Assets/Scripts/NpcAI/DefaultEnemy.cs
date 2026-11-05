using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Версия которая НЕ используется в игре
// По сути отличается наличием патруля

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class DefaultEnemy : MonoBehaviour
{
    const float arrivalStopDist = 0.1f;
    const float eps = 0.05f;

    public enum EnemyState
    {
        Chase,
        Patrol,
        Attack,
        Cooldown,
        ExternalAction
    }

    public EnemyState currentState = EnemyState.Patrol;


    private Transform player;

    [Header("Detection")]
    [SerializeField] float detectionRange = 25f;
    [SerializeField] LayerMask losMask = 0; // 0 = no LOS check
    [SerializeField] float chaseDuration = 3f;

    [Header("chase Cooldown")]
    [SerializeField] float ChaseCooldown = 0.5f;
    private float chaseCooldownTimer = 0f;

    [Header("Movement")]
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField, Min(0f)] private float chaseResumeDistance = 3.0f;
    [SerializeField] float rotateSpeed = 8f;

    [Header("NavMesh placement")]
    [SerializeField] float sampleRadius = 3f;

    [Header("Surface Patrol (NavMesh)")]
    [SerializeField] bool enableSurfacePatrol = true;
    [SerializeField] float patrolRadius = 8f;
    [SerializeField] float patrolPointWait = 1.5f;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 2.5f;
    public float chaseSpeed = 6f;

    [Header("Attack Timing")]
    [SerializeField] private float attackDamageDelay = 0.65f;
    [SerializeField] private float attackRecoveryTime = 1f;


    [Header("Animation")]
    public Animator _animator;
    [SerializeField] private string _walkBoolName = "Walk";
    [SerializeField] private string _runBoolName = "Run";
    public bool isUseFade;
    [SerializeField] private string _attackTriggerName = "Attack";

    [SerializeField] private bool useAgentPathOnlyWhenChasing = true;
    private Coroutine attackRoutine;

    private OverlapAttack overlapAttack;



    Rigidbody rb;
    NavMeshAgent agent;

    Vector3 originalPos;
    Vector3 lastSeenPos;

    float chaseTimer = 0f;

    Coroutine patrolCoroutine;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        overlapAttack = GetComponent<OverlapAttack>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        //GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        //if (player != null)
        //    player = playerObj.transform;


        originalPos = transform.position;
        lastSeenPos = originalPos;


        if (agent != null)
        {
            agent.speed = Mathf.Max(0.1f, patrolSpeed);
            agent.stoppingDistance = 0f;
            agent.autoRepath = true;
            agent.updateRotation = true;
            agent.updatePosition = true;
            if (agent.acceleration < 1f) agent.acceleration = 20f;
            if (agent.angularSpeed < 1f) agent.angularSpeed = 120f;


            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, sampleRadius, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
                else
                {
                    Debug.LogWarning("DefaultEnemy: navMesh íå íàéäåí");
                    agent = null;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    return;
                }
            }


        }
        else
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        EnterState(currentState);
    }

    void Update()
    {

        if (player == null) return;
        HandleDetection();

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;

            case EnemyState.Attack:
                UpdateAttack();
                break;

            case EnemyState.Cooldown:
                UpdateCooldown();
                break;

            case EnemyState.ExternalAction:
                break;
        }
        UpdateEnemyAnimation();
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Attack || currentState == EnemyState.Cooldown || currentState == EnemyState.ExternalAction)
        {
            return;
        }

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && agent.updatePosition)
            return;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.updatePosition)
        {
            Vector3 target = agent.steeringTarget;
            Vector3 dir = target - rb.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();
                Vector3 next = rb.position + dir * Mathf.Max(0.01f, chaseSpeed) * Time.fixedDeltaTime;
                rb.MovePosition(next);


                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
            }

            agent.nextPosition = rb.position;
            return;
        }
    }





    //   ==============================================----------------------------------------------------------------===============================================
    //                                                                ||||Update.EnemyState Logic||||
    //   ==============================================----------------------------------------------------------------===============================================

    public void HandleDetection()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        bool visible = IsPlayerVisible();

        if (dist <= detectionRange && visible)
        {
            chaseTimer = chaseDuration;
            lastSeenPos = player.position;

            if (currentState == EnemyState.Patrol)
                SetState(EnemyState.Chase);

            return;
        }

        if (currentState == EnemyState.Chase)
        {
            chaseTimer -= Time.deltaTime;

            if (chaseTimer <= 0f)
                SetState(EnemyState.Patrol);
        }
    }
    private void SetState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        ExitState(currentState);

        currentState = newState;

        EnterState(newState);
    }
    private void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patrol:
                agent.updatePosition = true;
                rb.isKinematic = true;

                agent.stoppingDistance = 0f;
                agent.speed = Mathf.Max(0.1f, patrolSpeed);
                StartSurfacePatrol();
                break;

            case EnemyState.Chase:
                if (useAgentPathOnlyWhenChasing)
                {
                    agent.updatePosition = false;
                    rb.isKinematic = false;

                    agent.nextPosition = rb.position;
                }
                if (isUseFade)
                    _animator.CrossFade("Run", 0.1f);

                StopSurfacePatrol();
                break;

            case EnemyState.Attack:
                StopMovement();
                if (attackRoutine == null)
                    attackRoutine = StartCoroutine(AttackRoutine());
                break;

            case EnemyState.ExternalAction:
                StopSurfacePatrol();
                StopMovement();

                if (attackRoutine != null)
                {
                    StopCoroutine(attackRoutine);
                    attackRoutine = null;
                }

                break;
            case EnemyState.Cooldown:
                chaseCooldownTimer = ChaseCooldown;
                break;
        }
    }
    private void ExitState(EnemyState state)
    {
    }
    public bool BeginExternalAction()
    {
        if (currentState == EnemyState.ExternalAction)
            return false;

        SetState(EnemyState.ExternalAction);
        return true;
    }

    private void UpdateChase()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= stoppingDistance + eps)
        {
            SetState(EnemyState.Attack);
            return;
        }


        agent.speed = Mathf.Max(0.1f, chaseSpeed);
        agent.isStopped = false;
        //if (useAgentPathOnlyWhenChasing)
        //{
        //    agent.updatePosition = false;
        //    rb.isKinematic = false;
        //}

        if (dist > chaseResumeDistance)
            agent.SetDestination(player.position);
    }
    private void UpdateCooldown()
    {
        chaseCooldownTimer -= Time.deltaTime;

        if (chaseCooldownTimer <= 0f)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= stoppingDistance + eps)
                SetState(EnemyState.Attack);
            else
                SetState(EnemyState.Chase);
        }
    }

    private void UpdatePatrol() { }
    private void UpdateAttack(){}







//   ==============================================----------------------------------------------------------------===============================================
//                                                                ||||DefaultEnemy Logic||||
//   ==============================================----------------------------------------------------------------===============================================


    void StartSurfacePatrol()
    {
        if (!enableSurfacePatrol || agent == null || !agent.isOnNavMesh) return;
        if (patrolCoroutine != null) return;
        patrolCoroutine = StartCoroutine(SurfacePatrolLoop());
    }

    public void StopSurfacePatrol()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    IEnumerator SurfacePatrolLoop()
    {

        while (currentState == EnemyState.Patrol)
        {
            Vector3 sample = originalPos + Random.insideUnitSphere * patrolRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, patrolRadius, NavMesh.AllAreas))
            {
                Vector3 dest = hit.position;
                agent.isStopped = false;


                agent.speed = Mathf.Max(0.1f, patrolSpeed);

                agent.SetDestination(dest);


                float waitTimeout = 12f;
                float t = 0f;
                while (currentState == EnemyState.Patrol && (agent.pathPending || agent.remainingDistance > arrivalStopDist) && t < waitTimeout)
                {
                    t += Time.deltaTime;
                    yield return null;
                }


                if (currentState != EnemyState.Patrol) break;


                agent.isStopped = true;
                float wait = patrolPointWait;
                while (currentState == EnemyState.Patrol && wait > 0f)
                {
                    wait -= Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                float retryWait = 0.5f;
                yield return new WaitForSeconds(retryWait);
            }
        }

        patrolCoroutine = null;
    }

    bool IsPlayerVisible()
    {
        if (player == null) return false;
        if (losMask == 0) return true;

        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 target = player.position + Vector3.up * 1f;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;
        if (dist < 0.001f) return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, losMask))
            return hit.transform == player;

        return true;
    }
    private IEnumerator AttackRoutine()
    {
        _animator?.SetTrigger(_attackTriggerName);

        yield return new WaitForSeconds(attackDamageDelay);

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position); //Vector3.Distance - âû÷èñëÿåò äèñòàíöèþ ìåæäó 2-ìÿ òî÷êàìè
            if (dist <= stoppingDistance + 0.5f)
            {
                if (overlapAttack == null)
                    overlapAttack = GetComponentInChildren<OverlapAttack>();

                if (overlapAttack == null) 
                    Debug.LogError("DefaulEnemy: îñòóòâóåò OverLapAttack íà îáúåêòå"); 
                overlapAttack.TryPerformAttack();
            }
        }

        yield return new WaitForSeconds(attackRecoveryTime);



        attackRoutine = null;
        SetState(EnemyState.Cooldown);
    }
    public void StopMovement()
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (rb != null)
            rb.velocity = Vector3.zero;
    }
    private void UpdateEnemyAnimation()
    {
        if (_animator == null) return;

        bool run = false;
        bool walk = false;

        if (agent != null && agent.isOnNavMesh)
        {
            switch (currentState)
            {
                case EnemyState.Patrol:
                    walk = !agent.isStopped;
                    break;

                case EnemyState.Chase:
                    run = !agent.isStopped;
                    break;
            }
        }

        _animator.SetBool(_walkBoolName, walk);
        _animator.SetBool(_runBoolName, run);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(originalPos, 0.12f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastSeenPos, 0.12f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(originalPos, patrolRadius);
    }
#endif
}
