using UnityEngine;
using UnityEngine.UI;

public class TowerCombat : MonoBehaviour
{
    public TowerData data;

	[Header("UI")]
    public Image healthBarFill;

    private float fireCountdown = 0f;
    private Transform targetEnemy;
	private int currentHealth;

    void Start()
    {
        if (TargetManager.Instance != null)
        {
            TargetManager.Instance.toursActives.Add(this.transform);
        }

		if(data != null)
		{
			currentHealth = data.maxHealth;
			UpdateHealthBar();
		}
    }

    void Update()
    {
        if (data == null) return;

        UpdateTarget();

        if (targetEnemy == null) return;

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / data.fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        if (TargetManager.Instance == null || TargetManager.Instance.ennemisActifs.Count == 0)
        {
            targetEnemy = null;
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
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= data.range)
        {
            targetEnemy = nearestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    void Shoot()
    {
        EnemyMovement e = targetEnemy.GetComponent<EnemyMovement>();
        if (e != null)
        {
            e.TakeDamage(data.damage);
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
		currentHealth -= damage;
		UpdateHealthBar();
		if (currentHealth <= 0)
		{
			Destroy(gameObject);
		}
	}
	
	private void UpdateHealthBar()
	{
		if (healthBarFill != null && data != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / data.maxHealth;
        }
	}
    
    void OnDestroy()
    {
        if (TargetManager.Instance != null)
        {
            TargetManager.Instance.toursActives.Remove(this.transform);
        }
    }
}