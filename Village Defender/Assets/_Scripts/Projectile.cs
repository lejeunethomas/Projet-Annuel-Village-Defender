using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform _transform;
    private int _damage;
    private float _speed = 10f;
    
    public void Setup(Transform target, int damage, float speed)
    {
        this._transform = target;
        this._damage = damage;
        this._speed = speed;
    }

    void Update()
    {
        if (_transform == null)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 dir = _transform.position - transform.position;
        float distanceFrame = _speed * Time.deltaTime;

        if (dir.sqrMagnitude <= distanceFrame * distanceFrame)
        {
            HitTarget();
            return;
        }
        
        transform.Translate(dir.normalized * distanceFrame, Space.World);
        transform.LookAt(_transform);
    }

    void HitTarget()
    {
        _transform.gameObject.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }
}
