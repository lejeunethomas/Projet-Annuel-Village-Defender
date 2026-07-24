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

		if(_target != null && _agent.hasPath && !_agent.pathPending)
		{
			if (_agent.remainingDistance <= data.attackRange)
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
            PlayAttackAnimation();
            GameManager.Instance.DamageBase(1);
            _attackTimer = data.attackRate;
        }
        else if (_target.CompareTag("Tower"))
        {
            TowerCombat tower = _target.GetComponent<TowerCombat>();
            if (tower != null)
            {
                PlayAttackAnimation();
                tower.TakeDamage(data.attackDamage);
                _attackTimer = data.attackRate;
            }
        }
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
