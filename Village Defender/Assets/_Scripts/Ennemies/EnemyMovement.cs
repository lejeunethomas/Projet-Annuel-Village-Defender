using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data;

    [SerializeField, Range(0.1f, 2f)]
    private float moveSpeedMultiplier = 0.85f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField] private float movementAnimationThreshold = 0.1f;
    [SerializeField, Min(0f)] private float attackDistanceTolerance = 0.15f;

    private NavMeshAgent _agent;
    private int _currentHealth;
	private Transform _target;
	private float _attackTimer;
    private bool isDead = false;
    private int _speedParameterHash;
    private int _attackParameterHash;
    private bool _hasSpeedParameter;
    private bool _hasAttackParameter;

    public bool IsDead
    {
        get { return isDead; }
    }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetupAnimator();
    }

    void Start()
    {
        if (data != null && _agent != null)
        {
            _agent.speed = data.moveSpeed * moveSpeedMultiplier;
			_agent.stoppingDistance = data.attackRange;
            _currentHealth = data.maxHealth;
        }

        if(TargetManager.Instance != null)
		{
			TargetManager.Instance.ennemisActifs.Add(this);
		}
    }
    
    void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentPhase != GameManager.GamePhase.Wave ||
            isDead ||
            data == null ||
            _agent == null)
        {
            SetAnimatorSpeed(0f);
            return;
        }

		if(_target != null && !_agent.pathPending)
		{
			if (IsTargetInAttackRange())
        	{
                if (!_agent.isStopped)
                    _agent.isStopped = true;

                SetAnimatorSpeed(0f);

				Vector3 direction = (_target.position - transform.position).normalized;
				if(direction != Vector3.zero)
				{
					Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
					transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
				}

                TryAttackTarget();
        	}
            else
            {
                if (_agent.isStopped)
                    _agent.isStopped = false;

                if (!_agent.hasPath)
                    _agent.SetDestination(_target.position);

                UpdateMovementAnimation();
            }
		} 
        else
        {
            UpdateMovementAnimation();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentPhase != GameManager.GamePhase.Wave)
        {
            return;
        }

        _currentHealth -= damageAmount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void KillInstantly()
    {
        if (isDead)
            return;

        Die();
    }
    
    public void RecevoirNouvelleCible(Transform cible)
    {
        if (_agent != null && cible != null)
        {
            if (_target != cible && _agent.isStopped)
                _agent.isStopped = false;

			_target = cible;
            _agent.SetDestination(cible.position);
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (data != null && GameManager.Instance != null)
            GameManager.Instance.AddGold(data.goldReward);

        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterEnemy();

        Destroy(gameObject);
    }

    public void PlayAttackAnimation()
    {
        SetAnimatorSpeed(0f);

        if (animator == null || !_hasAttackParameter)
            return;

        animator.ResetTrigger(_attackParameterHash);
        animator.SetTrigger(_attackParameterHash);
    }

    private void TryAttackTarget()
    {
        if (_target == null ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentPhase != GameManager.GamePhase.Wave ||
            isDead)
        {
            return;
        }

        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
            return;
        }

        if (_target.CompareTag("Base"))
        {
            GameManager.Instance.DamageBase(1);
            PlayAttackAnimation();
            _attackTimer = data.attackRate;
        }
        else
        {
            TowerCombat tower = _target.GetComponent<TowerCombat>();
            if (tower == null)
                tower = _target.GetComponentInParent<TowerCombat>();
            if (tower == null)
                tower = _target.GetComponentInChildren<TowerCombat>();

            if (tower != null)
            {
                tower.TakeDamage(data.attackDamage);
                PlayAttackAnimation();
                _attackTimer = data.attackRate;
            }
        }
    }

    private bool IsTargetInAttackRange()
    {
        if (_target == null || data == null)
            return false;

        Vector3 enemyPosition = transform.position;
        Vector3 closestTargetPoint = _target.position;

        Collider targetCollider = _target.GetComponent<Collider>();
        if (targetCollider == null)
            targetCollider = _target.GetComponentInChildren<Collider>();

        if (targetCollider != null && targetCollider.enabled)
            closestTargetPoint = targetCollider.ClosestPoint(enemyPosition);

        closestTargetPoint.y = enemyPosition.y;

        float effectiveAttackRange = Mathf.Max(0f, data.attackRange) + attackDistanceTolerance;
        if (_agent != null)
            effectiveAttackRange += _agent.radius;

        return Vector3.Distance(enemyPosition, closestTargetPoint) <= effectiveAttackRange;
    }

    private void UpdateMovementAnimation()
    {
        if (_agent == null)
        {
            SetAnimatorSpeed(0f);
            return;
        }

        float speed = _agent.velocity.magnitude;
        SetAnimatorSpeed(speed);
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (animator == null || !_hasSpeedParameter)
            return;

        if (speed < movementAnimationThreshold)
            speed = 0f;

        animator.SetFloat(_speedParameterHash, speed);
    }

    private void SetupAnimator()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            Debug.LogWarning("EnemyMovement : aucun Animator trouvé dans les enfants de " + gameObject.name + ".");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("EnemyMovement : aucun Animator Controller assigné sur " + animator.gameObject.name + " pour " + gameObject.name + ".");
            return;
        }

        _speedParameterHash = Animator.StringToHash(speedParameter);
        _attackParameterHash = Animator.StringToHash(attackParameter);

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == _speedParameterHash &&
                parameter.type == AnimatorControllerParameterType.Float)
            {
                _hasSpeedParameter = true;
            }

            if (parameter.nameHash == _attackParameterHash &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                _hasAttackParameter = true;
            }
        }

        if (!_hasSpeedParameter)
            Debug.LogWarning("EnemyMovement : paramètre Animator Float '" + speedParameter + "' introuvable sur " + animator.gameObject.name + " pour " + gameObject.name + ".");

        if (!_hasAttackParameter)
            Debug.LogWarning("EnemyMovement : paramètre Animator Trigger '" + attackParameter + "' introuvable sur " + animator.gameObject.name + " pour " + gameObject.name + ".");
    }

    void OnDestroy() 
    {
        if(TargetManager.Instance != null)
            TargetManager.Instance.ennemisActifs.Remove(this);
    }
}
