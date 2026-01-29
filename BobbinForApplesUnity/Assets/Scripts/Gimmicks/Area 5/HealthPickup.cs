using System.Collections;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Healing Amount")]
    [SerializeField] private float healPercentage = 0.333f;
    
    [Header("Spawn Animation")]
    [SerializeField] private float spawnDuration = 0.5f;
    [SerializeField] private float spawnBounciness = 1.5f;
    
    [Header("Idle Animation")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;
    
    private Vector3 startPosition;
    private Vector3 targetScale;
    private float bobOffset;
    private bool isSpawning = true;
    
    private void Start()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        targetScale = transform.localScale;
        
        transform.localScale = Vector3.zero;
        
        StartCoroutine(SpawnAnimation());
    }
    
    private IEnumerator SpawnAnimation()
    {
        float elapsed = 0f;
        
        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnDuration;
            
            float bounce = 1f + (spawnBounciness * Mathf.Sin(t * Mathf.PI));
            float scale = Mathf.Lerp(0f, 1f, t) * bounce;
            
            transform.localScale = targetScale * Mathf.Clamp(scale, 0f, 1.2f);
            
            yield return null;
        }
        
        transform.localScale = targetScale;
        isSpawning = false;
        
        Debug.Log("Health Pickup: Spawn animation complete, ready for collection.");
    }
    
    private void Update()
    {
        if (!isSpawning)
        {
            float bobY = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
            transform.position = startPosition + new Vector3(0f, bobY, 0f);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySound(AudioEventType.HealthPickup, transform.position);
            
            PlayerHealthUI healthUI = FindFirstObjectByType<PlayerHealthUI>();
            if (healthUI != null)
            {
                int healthToRestore = Mathf.CeilToInt(3 * healPercentage);
                
                Debug.Log($"Health Pickup: Player collected! Restoring {healthToRestore} health.");
                healthUI.RestoreHealth(healthToRestore);
            }
            else
            {
                Debug.LogWarning("Health Pickup: PlayerHealthUI not found!");
            }
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayGoldGlowEffect();
            }
            
            Destroy(gameObject);
        }
    }
}
