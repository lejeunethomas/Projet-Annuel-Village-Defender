using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerCharacterController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform homePoint;
    [SerializeField] private Camera gameplayCamera;

    [Header("Déplacement")]
    [SerializeField] private LayerMask movementGroundMask;
    [SerializeField] private float raycastDistance = 500f;
    [SerializeField] private float navMeshSampleDistance = 2f;
    [SerializeField] private float movingSpeedThreshold = 0.1f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackHitDelay = 0.35f;
    [SerializeField] private float attackDuration = 0.9f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Animator")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string attackParameter = "Attack";

    private bool isAttacking;
    private EnemyMovement currentTarget;
    private Coroutine attackCoroutine;
    private GameManager.GamePhase previousPhase;
    private bool phaseInitialized;

    private void Awake()
    {
        if (!EnsureSingleControllerInstance())
            return;

        ClampAttackTimings();
        ResolveReferences();
        TryInitializePhase();
    }

    private void Start()
    {
        if (!enabled)
            return;

        ClampAttackTimings();
        ResolveReferences();
        TryInitializePhase();
    }

    private void OnValidate()
    {
        raycastDistance = Mathf.Max(0f, raycastDistance);
        navMeshSampleDistance = Mathf.Max(0f, navMeshSampleDistance);
        movingSpeedThreshold = Mathf.Max(0f, movingSpeedThreshold);
        attackRange = Mathf.Max(0f, attackRange);
        attackHitDelay = Mathf.Max(0f, attackHitDelay);
        attackDuration = Mathf.Max(attackHitDelay, attackDuration);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
    }

    private void Update()
    {
        if (!enabled)
            return;

        UpdatePhaseState();

        if (IsWavePhase())
        {
            HandleMovementInput();

            if (!isAttacking && attackCoroutine == null)
                TryStartAttackOnClosestEnemy();
        }

        UpdateAnimatorSpeed();
    }

    private bool EnsureSingleControllerInstance()
    {
        PlayerCharacterController[] controllers = GetComponents<PlayerCharacterController>();
        if (controllers == null || controllers.Length <= 1)
            return true;

        if (controllers[0] == this)
        {
            Debug.LogError("PlayerCharacterController : plusieurs PlayerCharacterController sont présents sur ce GameObject. Supprimez les doublons dans Unity pour éviter les attaques multiples.");
            return true;
        }

        enabled = false;
        Debug.LogError("PlayerCharacterController : doublon désactivé automatiquement. Gardez un seul PlayerCharacterController sur PlayerCharacter.");
        return false;
    }

    private void ClampAttackTimings()
    {
        attackHitDelay = Mathf.Max(0f, attackHitDelay);
        attackDuration = Mathf.Max(attackHitDelay, attackDuration);
    }

    private void ResolveReferences()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        if (agent == null)
            Debug.LogError("PlayerCharacterController : aucun NavMeshAgent n'est assigné ou présent sur PlayerCharacter.");

        if (animator == null)
            Debug.LogError("PlayerCharacterController : aucun Animator n'est assigné ou présent dans les enfants de PlayerCharacter.");

        if (homePoint == null)
            Debug.LogError("PlayerCharacterController : aucun PlayerHomePoint n'est assigné.");
    }

    private void TryInitializePhase()
    {
        if (phaseInitialized || GameManager.Instance == null)
            return;

        previousPhase = GameManager.Instance.CurrentPhase;
        phaseInitialized = true;
        HandlePhaseChanged(previousPhase);
    }

    private void UpdatePhaseState()
    {
        if (GameManager.Instance == null)
            return;

        if (!phaseInitialized)
        {
            TryInitializePhase();
            return;
        }

        GameManager.GamePhase currentPhase = GameManager.Instance.CurrentPhase;
        if (currentPhase == previousPhase)
            return;

        previousPhase = currentPhase;
        HandlePhaseChanged(currentPhase);
    }

    private void HandlePhaseChanged(GameManager.GamePhase newPhase)
    {
        switch (newPhase)
        {
            case GameManager.GamePhase.Village:
            case GameManager.GamePhase.Preparation:
                MoveImmediatelyToHome();
                break;

            case GameManager.GamePhase.Wave:
                break;

            case GameManager.GamePhase.MainMenu:
            case GameManager.GamePhase.Intro:
                StopCurrentAttack();
                StopAgentAtCurrentPosition();
                SetAnimatorSpeed(0f);
                break;

            case GameManager.GamePhase.EndScreen:
                StopCurrentAttack();
                StopAgentAtCurrentPosition();
                SetAnimatorSpeed(0f);
                break;
        }
    }

    private void MoveImmediatelyToHome()
    {
        StopCurrentAttack();

        if (animator != null)
            animator.ResetTrigger(attackParameter);

        StopAndClearAgentPath();

        if (homePoint != null)
        {
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(homePoint.position, out navMeshHit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                bool warped = false;

                if (agent != null && agent.enabled)
                    warped = agent.Warp(navMeshHit.position);

                if (!warped)
                    transform.position = navMeshHit.position;
            }
            else
            {
                transform.position = homePoint.position;
            }

            transform.rotation = homePoint.rotation;
        }

        SetAnimatorSpeed(0f);
    }

    private void HandleMovementInput()
    {
        if (isAttacking || agent == null || !agent.enabled || !agent.isOnNavMesh || gameplayCamera == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = gameplayCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, raycastDistance, movementGroundMask))
            return;

        NavMeshHit navMeshHit;
        if (!NavMesh.SamplePosition(hit.point, out navMeshHit, navMeshSampleDistance, NavMesh.AllAreas))
            return;

        agent.SetDestination(navMeshHit.position);
        agent.isStopped = false;
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator == null)
            return;

        if (isAttacking || agent == null || !agent.enabled)
        {
            SetAnimatorSpeed(0f);
            return;
        }

        float speed = agent.velocity.magnitude;
        if (speed < movingSpeedThreshold)
            speed = 0f;

        SetAnimatorSpeed(speed);
    }

    private void TryStartAttackOnClosestEnemy()
    {
        if (!IsWavePhase() || isAttacking || attackCoroutine != null)
            return;

        EnemyMovement nearestEnemy = FindClosestEnemyInRange();
        if (nearestEnemy == null)
            return;

        StartAttack(nearestEnemy);
    }

    private void StartAttack(EnemyMovement target)
    {
        if (!IsWavePhase() || target == null || target.IsDead || isAttacking || attackCoroutine != null)
            return;

        isAttacking = true;
        currentTarget = target;

        attackCoroutine = StartCoroutine(AttackEnemy(target));
    }

    private EnemyMovement FindClosestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        EnemyMovement closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemyObject in enemies)
        {
            if (enemyObject == null)
                continue;

            EnemyMovement enemy = enemyObject.GetComponent<EnemyMovement>();
            if (enemy == null || enemy.IsDead)
                continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > attackRange || distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestEnemy = enemy;
        }

        return closestEnemy;
    }

    private IEnumerator AttackEnemy(EnemyMovement target)
    {
        if (target == null || target.IsDead)
        {
            currentTarget = null;
            isAttacking = false;
            attackCoroutine = null;
            yield break;
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        SetAnimatorSpeed(0f);
        RotateTowardsTarget(target);

        if (animator != null)
            animator.SetTrigger(attackParameter);

        yield return WaitAttackTime(attackHitDelay, target);

        if (IsWavePhase() && target != null && !target.IsDead)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance <= attackRange)
                target.KillInstantly();
        }

        float remainingAttackDuration = Mathf.Max(0f, attackDuration - attackHitDelay);
        yield return WaitAttackTime(remainingAttackDuration, target);

        currentTarget = null;
        isAttacking = false;
        attackCoroutine = null;
        SetAnimatorSpeed(0f);

        if (!IsWavePhase())
            yield break;

        ResumeAgentIfItStillHasAPath();
    }

    private IEnumerator WaitAttackTime(float duration, EnemyMovement target)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (target != null)
                RotateTowardsTarget(target);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void RotateTowardsTarget(EnemyMovement target)
    {
        if (target == null)
            return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void StopCurrentAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        isAttacking = false;
        currentTarget = null;

        if (animator != null)
            animator.ResetTrigger(attackParameter);

        SetAnimatorSpeed(0f);
    }

    private void StopAgentAtCurrentPosition()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void StopAndClearAgentPath()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    private void ResumeAgentIfItStillHasAPath()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (agent.hasPath || agent.pathPending)
            agent.isStopped = false;
    }

    private bool IsWavePhase()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.CurrentPhase == GameManager.GamePhase.Wave;
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat(speedParameter, speed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
