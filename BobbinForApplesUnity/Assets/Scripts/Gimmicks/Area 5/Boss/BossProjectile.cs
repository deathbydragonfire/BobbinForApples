using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 15f;
    
    private Vector3 direction;
    private float speed;
    private float lifetime = 8f;
    private float aliveTime;
    private ProjectilePool parentPool;
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("BossProjectile requires a Rigidbody component!");
        }
    }
    
    public void Initialize(Vector3 projectileDirection, float projectileSpeed, ProjectilePool pool)
    {
        direction = projectileDirection.normalized;
        speed = projectileSpeed;
        aliveTime = 0f;
        parentPool = pool;
    }
    
    private void FixedUpdate()
    {
        if (rb != null)
        {
            Vector3 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
        
        aliveTime += Time.fixedDeltaTime;
        if (aliveTime >= lifetime)
        {
            ReturnToPool();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("Boss projectile hit player! (-1 apple)");
                player.TakeDamage();
            }
            
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
