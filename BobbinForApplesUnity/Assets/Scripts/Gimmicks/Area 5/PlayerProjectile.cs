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
            BossController bossController = other.GetComponentInParent<BossController>();
            if (bossController == null)
            {
                bossController = other.GetComponent<BossController>();
            }
            
            if (bossController != null)
            {
                Debug.Log("BossController hit! Dealing damage.");
                bossController.TakeDamage(5f);
                ReturnToPool();
                return;
            }
            
            BobbdraManager bobbdraManager = other.GetComponentInParent<BobbdraManager>();
            if (bobbdraManager == null)
            {
                bobbdraManager = other.GetComponent<BobbdraManager>();
            }
            
            if (bobbdraManager != null)
            {
                Debug.Log("BobbdraManager hit! Dealing damage.");
                bobbdraManager.TakeDamage(3f, transform.position);
                ReturnToPool();
                return;
            }
            
            BobbdraHeadShootable shootableHead = other.GetComponentInParent<BobbdraHeadShootable>();
            if (shootableHead == null)
            {
                shootableHead = other.GetComponent<BobbdraHeadShootable>();
            }
            
            if (shootableHead != null)
            {
                Debug.Log("Shootable attack head hit! Dealing damage via BobbdraHeadShootable.");
                shootableHead.OnProjectileHit(transform.position);
                ReturnToPool();
                return;
            }
            
            Debug.LogWarning("Boss tag detected but no boss component found!");
            ReturnToPool();
        }
        else
        {
            AnnoyingHeadController annoyingHead = other.GetComponentInParent<AnnoyingHeadController>();
            if (annoyingHead == null)
            {
                annoyingHead = other.GetComponent<AnnoyingHeadController>();
            }
            
            if (annoyingHead != null)
            {
                Debug.Log("Annoying Head hit!");
                annoyingHead.TakeHit();
                ReturnToPool();
                return;
            }
            
            if (other.gameObject.layer != LayerMask.NameToLayer("Default"))
            {
                ReturnToPool();
            }
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
