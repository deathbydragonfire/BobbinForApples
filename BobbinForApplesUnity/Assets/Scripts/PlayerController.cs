using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float strokeForce = 50f;
    [SerializeField] private float strokeTorque = 100f;
    [SerializeField] private float kickForce = 150f;
    [SerializeField] private float pushOffForce = 100f;

    [Header("2D Plane Settings")]
    [SerializeField] private Vector3 planeNormal = Vector3.forward;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string leftArmAnimationName = "LeftArmStroke";
    [SerializeField] private string rightArmAnimationName = "RightArmStroke";
    [SerializeField] private string kickAnimationName = "Kick";
    [SerializeField] private int leftArmLayer = 1;
    [SerializeField] private int rightArmLayer = 2;
    [SerializeField] private int kickLayer = 3;

    [Header("Health System")]
    [SerializeField] private PlayerHealthUI healthUI;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float flashSequenceDuration = 0.3f;
    [SerializeField] private float shakeIntensity = 0.15f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float healFlashDuration = 0.3f;

    [Header("God Mode")]
    [SerializeField] private bool godMode = false;

    private Rigidbody rigidBody;
    private Keyboard keyboard;

    private Vector3 leftSurfaceNormal = Vector3.zero;
    private Vector3 rightSurfaceNormal = Vector3.zero;
    private bool isTouchingLeftSurface = false;
    private bool isTouchingRightSurface = false;
    private bool isInvincible = false;
    
    private Renderer[] playerRenderers;
    private Material[] originalMaterials;
    private Material[] redFlashMaterials;
    private Material[] whiteFlashMaterials;
    private Vector3 originalPosition;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        
        if (rigidBody == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody component!");
        }
        else
        {
            ConfigureRigidbodyFor2DPlane();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("PlayerController: No Animator found. Animations will not play.");
            }
        }
        
        InitializeFlashMaterials();
        
        keyboard = Keyboard.current;
    }
    
    private void InitializeFlashMaterials()
    {
        playerRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[playerRenderers.Length];
        redFlashMaterials = new Material[playerRenderers.Length];
        whiteFlashMaterials = new Material[playerRenderers.Length];
        
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            originalMaterials[i] = playerRenderers[i].material;
            
            redFlashMaterials[i] = new Material(originalMaterials[i]);
            redFlashMaterials[i].color = Color.red;
            
            whiteFlashMaterials[i] = new Material(originalMaterials[i]);
            whiteFlashMaterials[i].color = new Color(10f, 10f, 10f, 1f);
        }
        
        originalPosition = transform.position;
    }

    private void ConfigureRigidbodyFor2DPlane()
    {
        rigidBody.useGravity = false;
        
        if (planeNormal == Vector3.forward)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionZ | 
                                   RigidbodyConstraints.FreezeRotationX | 
                                   RigidbodyConstraints.FreezeRotationY;
        }
        else if (planeNormal == Vector3.up)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY | 
                                   RigidbodyConstraints.FreezeRotationX | 
                                   RigidbodyConstraints.FreezeRotationZ;
        }
        else if (planeNormal == Vector3.right)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionX | 
                                   RigidbodyConstraints.FreezeRotationY | 
                                   RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void Update()
    {
        if (keyboard == null)
        {
            return;
        }

        HandleArmStrokes();
        HandleKick();
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 contactNormal = contact.normal;
            float dotLeft = Vector3.Dot(contactNormal, transform.right);
            float dotRight = Vector3.Dot(contactNormal, -transform.right);

            if (dotLeft > 0.5f)
            {
                isTouchingLeftSurface = true;
                leftSurfaceNormal = contactNormal;
            }

            if (dotRight > 0.5f)
            {
                isTouchingRightSurface = true;
                rightSurfaceNormal = contactNormal;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isTouchingLeftSurface = false;
        isTouchingRightSurface = false;
        leftSurfaceNormal = Vector3.zero;
        rightSurfaceNormal = Vector3.zero;
    }

    private void HandleArmStrokes()
    {
        bool leftArmPressed = keyboard.aKey.wasPressedThisFrame;
        bool rightArmPressed = keyboard.dKey.wasPressedThisFrame;

        if (leftArmPressed && rightArmPressed)
        {
            ApplyForwardStroke();
        }
        else if (leftArmPressed)
        {
            ApplyLeftStroke();
        }
        else if (rightArmPressed)
        {
            ApplyRightStroke();
        }
    }

    private void ApplyLeftStroke()
    {
        Vector3 forwardDirection = transform.up;
        rigidBody.AddForce(forwardDirection * strokeForce, ForceMode.Impulse);
        rigidBody.AddTorque(GetTorqueAxis() * -strokeTorque, ForceMode.Impulse);

        if (isTouchingLeftSurface)
        {
            rigidBody.AddForce(leftSurfaceNormal * pushOffForce, ForceMode.Impulse);
        }

        PlayAnimation(leftArmAnimationName, leftArmLayer);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.PlayerStroke, transform.position);
        }
    }

    private void ApplyRightStroke()
    {
        Vector3 forwardDirection = transform.up;
        rigidBody.AddForce(forwardDirection * strokeForce, ForceMode.Impulse);
        rigidBody.AddTorque(GetTorqueAxis() * strokeTorque, ForceMode.Impulse);

        if (isTouchingRightSurface)
        {
            rigidBody.AddForce(rightSurfaceNormal * pushOffForce, ForceMode.Impulse);
        }

        PlayAnimation(rightArmAnimationName, rightArmLayer);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.PlayerStroke, transform.position);
        }
    }

    private void ApplyForwardStroke()
    {
        Vector3 forwardDirection = transform.up;
        rigidBody.AddForce(forwardDirection * strokeForce * 2f, ForceMode.Impulse);

        PlayAnimation(leftArmAnimationName, leftArmLayer);
        PlayAnimation(rightArmAnimationName, rightArmLayer);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.PlayerStroke, transform.position);
        }
    }

    private void HandleKick()
    {
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            Vector3 forwardDirection = transform.up;
            rigidBody.AddForce(forwardDirection * kickForce, ForceMode.Impulse);

            PlayAnimation(kickAnimationName, kickLayer);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(AudioEventType.PlayerKick, transform.position);
            }
        }
    }

    private void PlayAnimation(string animationName, int layer)
    {
        if (animator != null)
        {
            animator.Play(animationName, layer, 0f);
        }
    }

    private Vector3 GetTorqueAxis()
    {
        return planeNormal.normalized;
    }

    public void TakeDamage()
    {
        if (godMode)
        {
            Debug.Log("God Mode: Damage ignored");
            return;
        }

        if (isInvincible || healthUI == null)
        {
            return;
        }

        Debug.Log("Player taking damage - triggering flash sequence and shake");
        healthUI.TakeDamage();
        StartCoroutine(DamageFlashSequence());
        StartCoroutine(DamageShake());
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioEventType.PlayerDamage, transform.position);
        }
        
        if (healthUI.IsDead())
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
    
    private System.Collections.IEnumerator DamageFlashSequence()
    {
        float flashDuration = flashSequenceDuration / 4f;
        
        SetPlayerMaterials(redFlashMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(whiteFlashMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(redFlashMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(whiteFlashMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(originalMaterials);
    }
    
    private void SetPlayerMaterials(Material[] materials)
    {
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null && materials[i] != null)
            {
                playerRenderers[i].material = materials[i];
            }
        }
    }
    
    private System.Collections.IEnumerator DamageShake()
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            
            transform.position = startPosition + new Vector3(offsetX, offsetY, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = startPosition;
    }

    private void Die()
    {
        if (godMode)
        {
            Debug.Log("God Mode: Death ignored");
            return;
        }

        Debug.Log("Player died!");
        
        if (PowerupInventoryManager.Instance != null)
        {
            PowerupInventoryManager.Instance.ClearAllPowerups();
        }

        if (PlayerDeathManager.Instance != null)
        {
            PlayerDeathManager.Instance.TriggerDeath("GAME OVER");
        }
    }

    public void DieFromBobber()
    {
        if (godMode)
        {
            Debug.Log("God Mode: Death from bobber ignored");
            return;
        }

        Debug.Log("Player caught by bobber!");
        
        if (PowerupInventoryManager.Instance != null)
        {
            PowerupInventoryManager.Instance.ClearAllPowerups();
        }

        if (PlayerDeathManager.Instance != null)
        {
            PlayerDeathManager.Instance.TriggerDeath("GAME OVER");
        }
    }

    public void SetHealthUI(PlayerHealthUI ui)
    {
        healthUI = ui;
    }
    
    public void PlayGoldGlowEffect()
    {
        StartCoroutine(HealFlashSequence());
    }
    
    private System.Collections.IEnumerator HealFlashSequence()
    {
        float flashDuration = healFlashDuration / 6f;
        
        SetPlayerMaterials(whiteFlashMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(originalMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(whiteFlashMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(originalMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(whiteFlashMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(originalMaterials);
        yield return new WaitForSeconds(flashDuration);
        
        SetPlayerMaterials(originalMaterials);
    }
}
