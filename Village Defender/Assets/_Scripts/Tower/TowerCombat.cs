using UnityEngine;
using UnityEngine.UI;

public class TowerCombat : MonoBehaviour
{
    public TowerData data;

	[Header("UI")]
    public Image healthBarFill;
    
    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float projectileSpeed = 15f;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string attackParameter = "Attack";

    private float _fireCountdown = 0f;
    private Transform _targetEnemy;
	private int _currentHealth;
    private int _maxHealth;
    private int _degatsFinaux;
    private int _attackParameterHash;
    private bool _hasAttackParameter;

    void Awake()
    {
        SetupAnimator();
    }

    void Start()
    {
        if (TargetManager.Instance != null)
        {
            TargetManager.Instance.toursActives.Add(this.transform);
        }

		if(data != null)
		{
            int degatsDeBase = data.damage;
            int niveau = GetTowerLevel();
            int bonusParNiveau = data.bonusLv;
            _degatsFinaux = degatsDeBase + (niveau * bonusParNiveau);
            _maxHealth = data.maxHealth + (niveau * bonusParNiveau);
            _currentHealth = _maxHealth;
			UpdateHealthBar();
		}
    }

    void Update()
    {
        if (data == null) return;

        UpdateTarget();

        if (_targetEnemy == null) return;

        if (_fireCountdown <= 0f)
        {
            Shoot();
            PlayAttackAnimation();
            _fireCountdown = 1f / data.fireRate;
        }

        _fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        if (TargetManager.Instance == null || TargetManager.Instance.ennemisActifs.Count == 0)
        {
            _targetEnemy = null;
            return;
        }
        
        float shortestDistance = Mathf.Infinity;
        EnemyMovement nearestEnemy = null;

        foreach (EnemyMovement enemy in TargetManager.Instance.ennemisActifs)
        {
            if (enemy == null) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                if (enemy.data.Type == data.targetType || data.targetType == 0)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;   
                }
            }
        }

        if (nearestEnemy != null && shortestDistance <= data.range)
        {
            _targetEnemy = nearestEnemy.transform;
        }
        else
        {
            _targetEnemy = null;
        }
    }

    void Shoot()
    {
        EnemyMovement e = _targetEnemy.GetComponent<EnemyMovement>();
        
        if (e != null)
        {
            if (projectilePrefab != null && projectileSpawn != null && e.transform != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
                
                Projectile projectileScript = projectile.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    projectileScript.Setup(e.transform, _degatsFinaux ,projectileSpeed);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.range);
        }
    }

	public void TakeDamage(int damage)
	{
		_currentHealth -= damage;
		UpdateHealthBar();
		if (_currentHealth <= 0)
		{
			Destroy(gameObject);
		}
	}
	
	private void UpdateHealthBar()
	{
		if (healthBarFill != null && data != null && _maxHealth > 0)
        {
            healthBarFill.fillAmount = Mathf.Clamp01((float)_currentHealth / _maxHealth);
        }
	}

    private int GetTowerLevel()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.buildingInventory == null ||
            data == null)
        {
            return 0;
        }

        return GameManager.Instance.buildingInventory.GetTowerLevel(data.name);
    }

    private void SetupAnimator()
    {
        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>(true);

        if (characterAnimator == null || characterAnimator.runtimeAnimatorController == null)
            return;

        _attackParameterHash = Animator.StringToHash(attackParameter);

        foreach (AnimatorControllerParameter parameter in characterAnimator.parameters)
        {
            if (parameter.nameHash == _attackParameterHash &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                _hasAttackParameter = true;
                break;
            }
        }
    }

    private void PlayAttackAnimation()
    {
        if (characterAnimator == null ||
            characterAnimator.runtimeAnimatorController == null ||
            !_hasAttackParameter ||
            !characterAnimator.isActiveAndEnabled ||
            !characterAnimator.gameObject.activeInHierarchy)
        {
            return;
        }

        characterAnimator.ResetTrigger(_attackParameterHash);
        characterAnimator.SetTrigger(_attackParameterHash);
    }
    
    void OnDestroy()
    {
        if (TargetManager.Instance != null)
        {
            TargetManager.Instance.toursActives.Remove(this.transform);
        }
    }
}
