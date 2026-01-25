using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime = 3f;
    private float aliveTime;
    private ProjectilePool parentPool;
    
    public void Initialize(Vector3 projectileDirection, float projectileSpeed, ProjectilePool pool)
    {
        direction = projectileDirection.normalized;
        speed = projectileSpeed;
        aliveTime = 0f;
        parentPool = pool;
    }
    
    private void Update()
    {
        Vector3 movement = direction * speed * Time.deltaTime;
        transform.position += movement;
        
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
        {
            ReturnToPool();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile hit: {other.gameObject.name} (Tag: {other.tag}) at position {other.transform.position}, Projectile at {transform.position}");
        
        if (other.CompareTag("Boss"))
        {
            BossController boss = other.GetComponentInParent<BossController>();
            if (boss == null)
            {
                boss = other.GetComponent<BossController>();
            }
            
            if (boss != null)
            {
                Debug.Log("Boss hit! Dealing damage.");
                boss.TakeDamage(10f);
            }
            else
            {
                Debug.LogWarning("Boss tag detected but BossController not found!");
            }
            
            ReturnToPool();
        }
        else if (other.gameObject.layer != LayerMask.NameToLayer("Default"))
        {
            ReturnToPool();
        }
    }
    
    private void ReturnToPool()
    {
        if (parentPool != null)
        {
            parentPool.ReturnObject(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
