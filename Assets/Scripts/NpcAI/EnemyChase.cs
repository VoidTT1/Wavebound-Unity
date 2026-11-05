using System.Collections;
using UnityEngine;
using UnityEngine.AI;


// Версия которая используется в игре

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyChase : MonoBehaviour
{
    const float eps = 0.05f;

    public enum EnemyState
    {
        Chase,
        Attack,
        Cooldown,
        ExternalAction
    }

    private EnemyState currentState = EnemyState.Chase;
    public EnemyState CurrentState => currentState;
    public bool IsExternalAction => currentState == EnemyState.ExternalAction;


    private Transform player;
    public Transform Player => player;

    [Header("chase Cooldown")]
    [SerializeField] float ChaseCooldown = 0.5f;
    private float chaseCooldownTimer = 0f;

    [Header("Movement")]
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField, Min(0f)] private float chaseResumeDistance = 3.0f;
    [SerializeField] float rotateSpeed = 8f;

    [Header("NavMesh placement")]
    [SerializeField] float sampleRadius = 3f;

    [Header("Speed")]
    public float _speed = 6f;

    [Header("Attack Timing")]
    [SerializeField] private float attackDamageDelay = 0.65f;
    [SerializeField] private float attackRecoveryTime = 1f;


    [Header("Animation")]
    public Animator _animator;
    [SerializeField] private string _walkBoolName = "Walk";
    [SerializeField] private string _runBoolName = "Run";
    [SerializeField] private string _attackTriggerName = "Attack";

    [SerializeField] private bool useAgentPathOnlyWhenChasing = true;
    [SerializeField] private bool useCrossFade_Run = true;
    private Coroutine attackRoutine;

    private OverlapAttack overlapAttack;

    Rigidbody rb;
    NavMeshAgent agent;

    //float chaseTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        overlapAttack = GetComponent<OverlapAttack>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        //GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        //if (player != null)
        //    player = playerObj.transform;





        if (agent != null)
        {
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
                    Debug.LogWarning("EnemyChase: NavMeshSurface íå íàéäåí");
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
        SetState(EnemyState.Chase);
    }

    void Update()
    {

        if (player == null) return;

        switch (currentState)
        {
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
                Vector3 next = rb.position + dir * Mathf.Max(0.01f, _speed) * Time.fixedDeltaTime;
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
            case EnemyState.Attack:
                StopMovement();
                if (attackRoutine == null)
                    attackRoutine = StartCoroutine(AttackRoutine());
                break;

            case EnemyState.ExternalAction:
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

            case EnemyState.Chase:
                if (useAgentPathOnlyWhenChasing)
                {
                    agent.updatePosition = false;
                    //rb.isKinematic = false;

                    agent.nextPosition = rb.position;
                }
                if (useCrossFade_Run != false)
                    _animator.CrossFade("Run", 0.1f);

                

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


    public bool EndExternalAction()
    {
        if (currentState == EnemyState.Chase)
            return false;

        SetState(EnemyState.Chase);
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

        agent.speed = Mathf.Max(0.1f, _speed);
        agent.isStopped = false;

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
    private void UpdateAttack() { }



    //   ==============================================----------------------------------------------------------------===============================================
    //                                                                ||||EnemyChase Logic||||
    //   ==============================================----------------------------------------------------------------===============================================


    public void SetTarget(Transform target)
    {
        player = target;
    }
    private IEnumerator AttackRoutine()
    {
        _animator?.SetTrigger(_attackTriggerName);

        yield return new WaitForSeconds(attackDamageDelay);

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
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
                case EnemyState.Chase:
                    run = !agent.isStopped;
                    break;
            }
        }

        _animator.SetBool(_walkBoolName, walk);
        _animator.SetBool(_runBoolName, run);
    }
}
