using UnityEngine;

public class TowerCombat : MonoBehaviour
{
    public float range = 5f;
    public int damage = 10;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    public Transform targetEnemy;

    void Update()
    {
        UpdateTarget();

        if (targetEnemy == null) return;

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        // Trouve tous les ennemis (taggués "Enemy")
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
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
        // Simplification : pas de projectile physique, juste des dégâts directs
        EnemyMovement e = targetEnemy.GetComponent<EnemyMovement>();
        if (e != null)
        {
            e.TakeDamage(damage);
            Debug.Log("Pew Pew!"); // Pour vérifier que ça tire
        }
    }
    
    // Pour voir la portée dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}