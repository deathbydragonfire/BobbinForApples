using System.Collections;
using UnityEngine;

public class BobbdraHeadShootable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BobbdraManager bobbdraManager;
    
    [Header("Settings")]
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    
    private const float ATTACK_HEAD_DAMAGE = 3f;
    
    private Renderer[] attackHeadRenderers;
    private Material[] originalMaterials;
    private Material[] flashMaterials;
    
    private void Awake()
    {
        Transform puppetRoot = transform.parent;
        if (puppetRoot != null)
        {
            attackHeadRenderers = puppetRoot.GetComponentsInChildren<Renderer>();
        }
        else
        {
            attackHeadRenderers = GetComponentsInChildren<Renderer>();
        }
        
        if (attackHeadRenderers != null && attackHeadRenderers.Length > 0)
        {
            originalMaterials = new Material[attackHeadRenderers.Length];
            flashMaterials = new Material[attackHeadRenderers.Length];
            
            for (int i = 0; i < attackHeadRenderers.Length; i++)
            {
                if (attackHeadRenderers[i] != null)
                {
                    originalMaterials[i] = attackHeadRenderers[i].material;
                    flashMaterials[i] = new Material(originalMaterials[i]);
                    flashMaterials[i].color = hitFlashColor;
                }
            }
            
            Debug.Log($"BobbdraHeadShootable: Found {attackHeadRenderers.Length} renderers to flash");
        }
    }
    
    private void Start()
    {
        if (bobbdraManager == null)
        {
            bobbdraManager = FindFirstObjectByType<BobbdraManager>();
            
            if (bobbdraManager == null)
            {
                Debug.LogWarning($"BobbdraHeadShootable on {gameObject.name} could not find BobbdraManager in scene!");
            }
        }
    }
    
    public void OnProjectileHit(Vector3 hitPosition)
    {
        if (bobbdraManager == null)
        {
            Debug.LogWarning($"BobbdraHeadShootable on {gameObject.name}: Cannot deal damage - BobbdraManager is null!");
            return;
        }
        
        float damage = ATTACK_HEAD_DAMAGE * damageMultiplier;
        bobbdraManager.TakeDamage(damage, hitPosition, bypassInvincibility: true);
        
        StartCoroutine(FlashAttackHead());
        
        Debug.Log($"Attack head {gameObject.name} was shot! Dealt {damage} damage to Bobbdra at position {hitPosition}");
    }
    
    private IEnumerator FlashAttackHead()
    {
        if (attackHeadRenderers == null || attackHeadRenderers.Length == 0)
        {
            yield break;
        }
        
        for (int i = 0; i < attackHeadRenderers.Length; i++)
        {
            if (attackHeadRenderers[i] != null && flashMaterials[i] != null)
            {
                attackHeadRenderers[i].material = flashMaterials[i];
            }
        }
        
        yield return new WaitForSeconds(hitFlashDuration);
        
        for (int i = 0; i < attackHeadRenderers.Length; i++)
        {
            if (attackHeadRenderers[i] != null && originalMaterials[i] != null)
            {
                attackHeadRenderers[i].material = originalMaterials[i];
            }
        }
    }
}
