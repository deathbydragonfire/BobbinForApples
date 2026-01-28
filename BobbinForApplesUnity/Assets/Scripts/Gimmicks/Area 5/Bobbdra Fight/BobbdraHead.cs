using System.Collections;
using UnityEngine;

public class BobbdraHead : MonoBehaviour
{
    public enum HeadPosition
    {
        Left,
        Center,
        Right
    }
    
    [Header("Head Configuration")]
    [SerializeField] private HeadPosition position;
    
    [Header("Animation")]
    [SerializeField] private SimpleAnimationPlayer animationPlayer;
    [SerializeField] private AnimationClip lungeClip;
    [SerializeField] private AnimationClip biteClip;
    [SerializeField] private AnimationClip idleClip;
    [SerializeField] private BobbdraHeadAnimator headAnimator;
    
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    
    [Header("Attack Indicator")]
    [SerializeField] private GameObject biteAttackIndicator;
    
    [Header("Projectile Barrage Animation")]
    [SerializeField] private AnimationClip projectileBarrageClip;
    [SerializeField] private ProjectileSpawnGlow projectileSpawnGlow;
    
    [Header("Bite Animation Controller")]
    [SerializeField] private BiteAnimationController biteAnimationController;
    
    private BobbdraManager manager;
    private Vector3 restingPosition;
    private bool isAttacking;
    
    public HeadPosition Position => position;
    public bool IsAttacking => isAttacking;
    
    private void Start()
    {
        if (biteAttackIndicator == null)
        {
            biteAttackIndicator = FindBiteAttackIndicator();
        }
        
        if (biteAttackIndicator != null)
        {
            biteAttackIndicator.SetActive(false);
        }
    }
    
    private GameObject FindBiteAttackIndicator()
    {
        Transform indicator = transform.Find("Bite Attack Indicator");
        if (indicator != null) return indicator.gameObject;
        
        indicator = transform.Find("Bite Attack Indicator (2)");
        if (indicator != null) return indicator.gameObject;
        
        Transform puppet = transform.Find("The Bobber Puppet");
        if (puppet != null)
        {
            indicator = puppet.Find("Bite Attack Indicator");
            if (indicator != null) return indicator.gameObject;
        }
        
        return null;
    }
    
    public void Initialize(BobbdraManager bobbdraManager)
    {
        manager = bobbdraManager;
        restingPosition = transform.localPosition;
        
        if (animationPlayer == null)
        {
            animationPlayer = GetComponent<SimpleAnimationPlayer>();
        }
        
        if (headAnimator == null)
        {
            headAnimator = GetComponent<BobbdraHeadAnimator>();
        }
        
        if (biteAnimationController == null)
        {
            biteAnimationController = GetComponent<BiteAnimationController>();
        }
        
        Debug.Log($"BobbdraHead ({position}) initialized at position {restingPosition}");
        Debug.Log($"  - AnimationPlayer: {(animationPlayer != null ? "Found" : "NULL")}");
        Debug.Log($"  - LungeClip: {(lungeClip != null ? lungeClip.name : "NULL")}");
        Debug.Log($"  - BiteClip: {(biteClip != null ? biteClip.name : "NULL")}");
        Debug.Log($"  - IdleClip: {(idleClip != null ? idleClip.name : "NULL")}");
        
        if (biteAttackIndicator == null)
        {
            biteAttackIndicator = FindBiteAttackIndicator();
        }
        
        if (biteAttackIndicator != null)
        {
            biteAttackIndicator.SetActive(false);
            Debug.Log($"  - BiteAttackIndicator: Found at {GetRelativePath(biteAttackIndicator.transform)}");
        }
        else
        {
            Debug.LogWarning($"  - BiteAttackIndicator: NOT FOUND");
        }
    }
    
    private string GetRelativePath(Transform target)
    {
        if (target == transform) return "self";
        if (target.parent == transform) return target.name;
        return target.parent.name + "/" + target.name;
    }
    
    public void PerformBiteAttack(Vector3 targetPosition, bool skipIdleControl = false)
    {
        if (isAttacking)
        {
            return;
        }
        
        StartCoroutine(BiteAttackSequence(targetPosition, skipIdleControl));
    }
    
    public void PrepareForAttack(float delay = 0f, bool skipIdleControl = false)
    {
        if (!skipIdleControl && biteAnimationController != null)
        {
            biteAnimationController.ScheduleAttack(delay);
        }
    }
    
    private IEnumerator BiteAttackSequence(Vector3 targetPosition, bool skipIdleControl = false)
    {
        isAttacking = true;
        
        HideAttackIndicator();
        
        if (!skipIdleControl && biteAnimationController != null)
        {
            biteAnimationController.ReleaseFreeze();
        }
        
        Debug.Log($"[{position}] BiteAttackSequence started");
        
        if (headAnimator != null)
        {
            headAnimator.SetIdleEnabled(false);
        }
        
        float lungeDuration = 0.5f;
        
        if (animationPlayer != null && lungeClip != null)
        {
            Debug.Log($"[{position}] Playing lunge clip: {lungeClip.name}");
            animationPlayer.Play(lungeClip);
            lungeDuration = animationPlayer.GetClipLength(lungeClip);
        }
        else
        {
            Debug.LogWarning($"[{position}] Cannot play lunge - Player: {animationPlayer != null}, Clip: {lungeClip != null}");
        }
        
        yield return new WaitForSeconds(lungeDuration);
        
        if (animationPlayer != null && biteClip != null)
        {
            Debug.Log($"[{position}] Playing bite clip: {biteClip.name}");
            animationPlayer.Play(biteClip);
            yield return new WaitForSeconds(animationPlayer.GetClipLength(biteClip));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        ReturnToRestingPosition();
        
        yield return new WaitForSeconds(0.1f);
        
        if (!skipIdleControl && biteAnimationController != null)
        {
            biteAnimationController.ForceResetImmediate();
        }
        
        isAttacking = false;
    }
    
    public void PlayProjectileBarrageAnimation()
    {
        if (biteAnimationController != null)
        {
            biteAnimationController.ReleaseFreeze();
        }
        
        if (animationPlayer != null && projectileBarrageClip != null)
        {
            if (headAnimator != null)
            {
                headAnimator.SetIdleEnabled(false);
            }
            
            animationPlayer.Play(projectileBarrageClip);
            Debug.Log($"BobbdraHead ({position}): Playing projectile barrage animation - {projectileBarrageClip.name}");
        }
        else
        {
            Debug.LogWarning($"BobbdraHead ({position}): Cannot play projectile barrage - AnimationPlayer: {animationPlayer != null}, Clip: {projectileBarrageClip != null}");
            
            if (projectileBarrageClip == null)
            {
                Debug.LogWarning($"BobbdraHead ({position}): Projectile Barrage Clip is not assigned in the inspector!");
            }
        }
    }
    
    public void OnProjectileBarrageComplete()
    {
        if (headAnimator != null)
        {
            headAnimator.SetIdleEnabled(true);
        }
        
        if (biteAnimationController != null)
        {
            biteAnimationController.ForceResetImmediate();
        }
        
        Debug.Log($"BobbdraHead ({position}): Projectile barrage complete, reset to closed idle");
    }
    
    public void TriggerProjectileGlow()
    {
        if (projectileSpawnGlow != null)
        {
            projectileSpawnGlow.TriggerFlash();
            Debug.Log($"BobbdraHead ({position}): Triggered projectile glow flash");
        }
        else
        {
            Debug.LogWarning($"BobbdraHead ({position}): Cannot trigger glow - ProjectileSpawnGlow component not assigned!");
        }
    }
    
    public AnimationClip GetProjectileBarrageClip()
    {
        return projectileBarrageClip;
    }
    
    public void FireProjectile(Vector3 direction, float speed = 8f)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"BobbdraHead ({position}): Projectile prefab not assigned");
            return;
        }
        
        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        BobbdraProjectile projectileScript = projectile.GetComponent<BobbdraProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(direction.normalized, speed);
        }
        
        Debug.Log($"BobbdraHead ({position}): Fired projectile in direction {direction}");
    }
    
    public void ReturnToRestingPosition()
    {
        StartCoroutine(ReturnToRestCoroutine());
    }
    
    private IEnumerator ReturnToRestCoroutine()
    {
        float returnDuration = 0.4f;
        
        if (animationPlayer != null && idleClip != null)
        {
            animationPlayer.Play(idleClip);
        }
        
        yield return new WaitForSeconds(returnDuration);
        
        if (headAnimator != null)
        {
            headAnimator.SetIdleEnabled(true);
        }
        
        yield return new WaitForSeconds(0.1f);
        
        if (biteAnimationController != null)
        {
            biteAnimationController.ForceResetImmediate();
        }
        
        transform.localPosition = restingPosition;
    }
    
    public void PlayIdleAnimation()
    {
        if (animationPlayer != null && idleClip != null && !isAttacking)
        {
            animationPlayer.Play(idleClip);
        }
    }
    
    public void PlayRoarAnimation(AnimationClip roarClip)
    {
        if (biteAnimationController != null)
        {
            biteAnimationController.ReleaseFreeze();
        }
        
        if (animationPlayer != null && roarClip != null)
        {
            Debug.Log($"BobbdraHead ({position}): Playing roar animation");
            
            if (headAnimator != null)
            {
                headAnimator.SetIdleEnabled(false);
            }
            
            animationPlayer.Play(roarClip);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(AudioEventType.BossRoar, transform.position);
                Debug.Log($"BobbdraHead ({position}): Triggered roar sound");
            }
        }
        else
        {
            Debug.LogWarning($"BobbdraHead ({position}): Cannot play roar - AnimationPlayer: {animationPlayer != null}, RoarClip: {roarClip != null}");
        }
    }
    
    public void OnRoarComplete()
    {
        if (headAnimator != null)
        {
            headAnimator.SetIdleEnabled(true);
        }
        
        if (biteAnimationController != null)
        {
            biteAnimationController.ForceResetImmediate();
        }
        
        Debug.Log($"BobbdraHead ({position}): Roar complete, reset to closed idle");
    }
    
    public void StopAllAttacks()
    {
        StopAllCoroutines();
        isAttacking = false;
        transform.localPosition = restingPosition;
        
        if (headAnimator != null)
        {
            headAnimator.SetIdleEnabled(false);
        }
        
        if (biteAnimationController != null)
        {
            biteAnimationController.CancelScheduledAttack();
        }
        
        HideAttackIndicator();
        
        Debug.Log($"BobbdraHead ({position}): All attacks stopped");
    }
    
    public void ShowAttackIndicator()
    {
        if (biteAttackIndicator != null)
        {
            biteAttackIndicator.SetActive(true);
            Debug.Log($"BobbdraHead ({position}): Attack indicator shown");
        }
        else
        {
            Debug.LogWarning($"BobbdraHead ({position}): No attack indicator assigned");
        }
    }
    
    public void HideAttackIndicator()
    {
        if (biteAttackIndicator != null)
        {
            biteAttackIndicator.SetActive(false);
            Debug.Log($"BobbdraHead ({position}): Attack indicator hidden");
        }
    }
}
