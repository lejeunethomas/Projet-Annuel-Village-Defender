using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed = 10f;
    
    public void Setup(Transform target, int damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 dir = target.position - transform.position;
        float distanceFrame = speed * Time.deltaTime;

        if (dir.sqrMagnitude <= distanceFrame * distanceFrame)
        {
            HitTarget();
            return;
        }
        
        transform.Translate(dir.normalized * distanceFrame, Space.World);
        transform.LookAt(target);
    }

    void HitTarget()
    {
        target.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }
}
