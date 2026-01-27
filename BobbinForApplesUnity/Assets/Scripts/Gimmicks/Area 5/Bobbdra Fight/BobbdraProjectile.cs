using UnityEngine;

public class BobbdraProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;
    
    private Vector3 direction;
    private float speed;
    private float aliveTime;
    
    public void Initialize(Vector3 projectileDirection, float projectileSpeed)
    {
        direction = projectileDirection.normalized;
        speed = projectileSpeed;
        aliveTime = 0f;
    }
    
    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log($"Bobbdra projectile hit player! (-{damage} apple)");
                player.TakeDamage();
            }
            
            Destroy(gameObject);
        }
    }
}
