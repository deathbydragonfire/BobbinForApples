using UnityEngine;
using System.Collections;

public class PowerupPickup : MonoBehaviour
{
    [Header("Powerup Configuration")]
    [SerializeField] private PowerupType powerupType = PowerupType.None;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Spawn Animation")]
    [SerializeField] private float spawnDuration = 0.5f;
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Idle Animation")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotationSpeed = 50f;
    
    [Header("Collision")]
    [SerializeField] private float colliderDepth = 5f;
    
    private Vector3 originalScale;
    private Vector3 startPosition;
    private bool isCollected = false;
    private BoxCollider boxCollider;
    
    public PowerupType PowerupType => powerupType;
    
    private void Start()
    {
        originalScale = transform.localScale;
        startPosition = transform.position;
        
        StartCoroutine(SpawnAnimation());
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.PowerupSpawn, transform.position);
        }
    }
    
    private IEnumerator SpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        
        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnDuration;
            float curveValue = spawnCurve.Evaluate(t);
            
            transform.localScale = originalScale * curveValue;
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        StartCoroutine(IdleAnimation());
    }
    
    private IEnumerator IdleAnimation()
    {
        while (!isCollected)
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * bobOffset;
            
            if (enableRotation)
            {
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
            
            yield return null;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
        {
            return;
        }
        
        if (other.CompareTag(playerTag))
        {
            CollectPowerup(other);
        }
    }
    
    private void CollectPowerup(Collider playerCollider)
    {
        isCollected = true;
        
        if (PowerupInventoryManager.Instance != null)
        {
            PowerupInventoryManager.Instance.AddPowerup(powerupType, GetComponent<SpriteRenderer>());
        }
        
        if (PowerupNotificationUI.Instance != null)
        {
            PowerupNotificationUI.Instance.ShowNotification(powerupType);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.PowerupPickup, transform.position);
        }
        
        PlayerController player = playerCollider.GetComponent<PlayerController>();
        if (player != null)
        {
            player.PlayGoldGlowEffect();
        }
        
        Destroy(gameObject);
    }
}
