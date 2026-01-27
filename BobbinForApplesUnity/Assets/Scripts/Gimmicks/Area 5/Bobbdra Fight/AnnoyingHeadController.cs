using UnityEngine;
using System.Collections;

public class AnnoyingHeadController : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Transform rotTransform;
    
    [Header("Death Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Collision")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 2f;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHits = 5;
    [SerializeField] private float respawnDelay = 10f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private float hitShakeDuration = 0.1f;
    [SerializeField] private float hitShakeMagnitude = 0.15f;
    
    private Transform playerTransform;
    private bool isDying = false;
    private float lastDamageTime = -999f;
    private Renderer[] renderers;
    private int currentHits = 0;
    private Vector3 spawnPosition;
    private BobbdraManager bobbdraManager;
    private DamageFlash damageFlash;
    private CameraShake cameraShake;
    private Color[][] originalColors;
    
    private const string DEATH_ANIMATION_NAME = "Annoying Head Death";
    
    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        
        StoreOriginalColors();
        
        if (rotTransform == null)
        {
            rotTransform = transform.Find("Rot");
            if (rotTransform == null)
            {
                Debug.LogWarning("AnnoyingHead: Could not find 'Rot' child object. Rotation will be applied to root.");
                rotTransform = transform;
            }
        }
        
        damageFlash = GetComponent<DamageFlash>();
        if (damageFlash == null)
        {
            damageFlash = gameObject.AddComponent<DamageFlash>();
        }
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
            }
        }
    }
    
    private void StoreOriginalColors()
    {
        originalColors = new Color[renderers.Length][];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                Material[] materials = renderers[i].materials;
                originalColors[i] = new Color[materials.Length];
                
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] != null && materials[j].HasProperty("_Color"))
                    {
                        originalColors[i][j] = materials[j].GetColor("_Color");
                    }
                }
            }
        }
    }
    
    public void Initialize(Vector3 initialSpawnPosition, BobbdraManager manager)
    {
        spawnPosition = initialSpawnPosition;
        bobbdraManager = manager;
        currentHits = 0;
        isDying = false;
    }
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("AnnoyingHead: Could not find player with tag 'Player'");
        }
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    
    private void Update()
    {
        if (isDying || playerTransform == null)
        {
            return;
        }
        
        ChasePlayer();
    }
    
    private void ChasePlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.z = 0f;
        
        transform.position += directionToPlayer * chaseSpeed * Time.deltaTime;
        
        if (directionToPlayer != Vector3.zero && rotTransform != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
            rotTransform.rotation = Quaternion.Slerp(rotTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isDying)
        {
            return;
        }
        
        if (other.CompareTag("Player") && Time.time >= lastDamageTime + damageCooldown)
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage();
                lastDamageTime = Time.time;
            }
        }
    }
    
    public void TakeHit()
    {
        if (isDying)
        {
            return;
        }
        
        currentHits++;
        Debug.Log($"Annoying Head hit! {currentHits}/{maxHits}");
        
        if (damageFlash != null)
        {
            damageFlash.Flash();
        }
        
        if (cameraShake != null)
        {
            cameraShake.Shake(hitShakeDuration, hitShakeMagnitude);
        }
        
        StartCoroutine(HitShake());
        
        if (currentHits >= maxHits)
        {
            Die(true);
        }
    }
    
    private IEnumerator HitShake()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < hitShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * hitShakeMagnitude;
            float y = Random.Range(-1f, 1f) * hitShakeMagnitude;
            
            transform.position = originalPosition + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
    }
    
    public void Die(bool shouldRespawn = false)
    {
        if (isDying)
        {
            return;
        }
        
        isDying = true;
        StartCoroutine(DeathSequence(shouldRespawn));
    }
    
    private IEnumerator DeathSequence(bool shouldRespawn)
    {
        Debug.Log("AnnoyingHead: Starting death sequence");
        
        if (animator != null)
        {
            animator.Play(DEATH_ANIMATION_NAME);
            
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            float animationLength = 2.63f;
            
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == DEATH_ANIMATION_NAME)
                {
                    animationLength = clip.length;
                    break;
                }
            }
            
            yield return new WaitForSeconds(animationLength);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        
        yield return StartCoroutine(FadeOut());
        
        if (shouldRespawn)
        {
            if (bobbdraManager != null)
            {
                bobbdraManager.StartCoroutine(RespawnAfterDelay());
            }
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }
    
    private void Respawn()
    {
        if (bobbdraManager != null && !bobbdraManager.IsDead())
        {
            currentHits = 0;
            isDying = false;
            
            transform.position = spawnPosition;
            
            RestoreOriginalColors();
            
            gameObject.SetActive(true);
            Debug.Log($"Annoying Head respawned at {spawnPosition}!");
        }
        else
        {
            Debug.Log("Bobbdra is dead, destroying Annoying Head instead of respawning");
            Destroy(gameObject);
        }
    }
    
    private void RestoreOriginalColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalColors[i] != null)
            {
                Material[] materials = renderers[i].materials;
                
                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] != null && j < originalColors[i].Length)
                    {
                        materials[j].SetColor("_Color", originalColors[i][j]);
                    }
                }
                
                renderers[i].materials = materials;
            }
        }
    }
    
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color[] originalColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
                    Color newColor = originalColors[i];
                    newColor.a = alpha;
                    renderers[i].material.color = newColor;
                }
            }
            
            yield return null;
        }
    }
}
