using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform targetBase;
    public int health = 30;
    public int goldReward = 10;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (targetBase != null)
            agent.SetDestination(targetBase.position);
    }

    void Update()
    {
        // Si l'ennemi est arrivé à la base
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GameManager.Instance.DamageBase(1);
            Destroy(gameObject); // L'ennemi disparaît
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GameManager.Instance.AddGold(goldReward);
            Destroy(gameObject);
        }
    }
}