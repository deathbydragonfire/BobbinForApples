using System.Collections;
using UnityEngine;

public class ProjectileBarragePattern : BobbdraAttackPattern
{
    [Header("Projectile Barrage Settings")]
    [SerializeField] private bool useOnlyCenterHead = true;
    [SerializeField] private int projectilesPerVolley = 4;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private bool simultaneousFire = false;
    [SerializeField] private float delayBetweenShots = 0.15f;
    [SerializeField] private bool doubleVolley = false;
    [SerializeField] private float volleyDelay = 0.5f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileScale = 1.5f;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float fireDelayAfterAnimationStart = 2.0f;
    [SerializeField] private bool onlyCenterHeadAnimation = true;
    
    private bool patternComplete;
    private Transform player;
    private BobbdraHead centerHead;
    
    public override void Initialize(BobbdraManager bobbdraManager, BobbdraHead[] bobbdraHeads)
    {
        base.Initialize(bobbdraManager, bobbdraHeads);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        foreach (BobbdraHead head in heads)
        {
            if (head != null && head.Position == BobbdraHead.HeadPosition.Center)
            {
                centerHead = head;
                
                ProjectileBarrageAnimationEvents animEvents = head.GetComponentInChildren<ProjectileBarrageAnimationEvents>();
                if (animEvents != null)
                {
                    animEvents.SetProjectileBarragePattern(this);
                    Debug.Log("ProjectileBarragePattern: Connected to center head animation events");
                }
                break;
            }
        }
        
        if (centerHead == null)
        {
            Debug.LogWarning("ProjectileBarragePattern: Center head not found!");
        }
    }
    
    public override void ExecutePattern()
    {
        if (!AreAllHeadsReady())
        {
            return;
        }
        
        isExecuting = true;
        patternComplete = false;
        
        StartCoroutine(ExecuteProjectileBarrage());
    }
    
    private IEnumerator ExecuteProjectileBarrage()
    {
        Debug.Log($"Executing Projectile Barrage Pattern (useAnimation: {useAnimation}, useOnlyCenterHead: {useOnlyCenterHead}, projectilesPerVolley: {projectilesPerVolley})");
        
        float animationLength = 0f;
        
        if (useOnlyCenterHead && centerHead != null)
        {
            centerHead.PrepareForAttack();
        }
        else if (!useOnlyCenterHead)
        {
            foreach (BobbdraHead head in heads)
            {
                if (head != null)
                {
                    head.PrepareForAttack();
                }
            }
        }
        
        if (useAnimation)
        {
            if (useOnlyCenterHead)
            {
                if (centerHead != null)
                {
                    AnimationClip barrageClip = centerHead.GetProjectileBarrageClip();
                    if (barrageClip != null)
                    {
                        animationLength = barrageClip.length;
                    }
                    
                    centerHead.PlayProjectileBarrageAnimation();
                    Debug.Log($"ProjectileBarragePattern: Animation started (length: {animationLength}s). Waiting {fireDelayAfterAnimationStart}s before firing projectiles and glow.");
                    
                    yield return new WaitForSeconds(fireDelayAfterAnimationStart);
                    
                    centerHead.TriggerProjectileGlow();
                    FireVolley();
                }
                else
                {
                    Debug.LogWarning("ProjectileBarragePattern: Center head is null, cannot fire!");
                }
            }
            else
            {
                foreach (BobbdraHead head in heads)
                {
                    if (head != null)
                    {
                        head.PlayProjectileBarrageAnimation();
                    }
                }
                
                Debug.Log($"ProjectileBarragePattern: Animations started. Waiting {fireDelayAfterAnimationStart}s before firing.");
                yield return new WaitForSeconds(fireDelayAfterAnimationStart);
                
                FireVolley();
            }
        }
        else
        {
            FireVolley();
        }
        
        if (doubleVolley)
        {
            yield return new WaitForSeconds(volleyDelay);
            Debug.Log("ProjectileBarragePattern: Firing second volley");
            
            if (useAnimation && centerHead != null)
            {
                centerHead.TriggerProjectileGlow();
            }
            
            FireVolley();
        }
        
        if (useAnimation && animationLength > 0f)
        {
            float timeElapsed = fireDelayAfterAnimationStart + (doubleVolley ? volleyDelay : 0f);
            float timeRemaining = animationLength - timeElapsed;
            
            if (timeRemaining > 0f)
            {
                Debug.Log($"ProjectileBarragePattern: Waiting {timeRemaining}s for animation to complete");
                yield return new WaitForSeconds(timeRemaining);
            }
        }
        
        if (useOnlyCenterHead && centerHead != null)
        {
            centerHead.OnProjectileBarrageComplete();
        }
        else if (!useOnlyCenterHead)
        {
            foreach (BobbdraHead head in heads)
            {
                if (head != null)
                {
                    head.OnProjectileBarrageComplete();
                }
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        patternComplete = true;
        isExecuting = false;
        Debug.Log("Projectile Barrage Pattern Complete");
    }
    
    private void FireVolley()
    {
        if (simultaneousFire)
        {
            FireAllHeads();
        }
        else
        {
            StartCoroutine(FireHeadsSequentially());
        }
    }
    
    private void FireAllHeads()
    {
        if (useOnlyCenterHead)
        {
            if (centerHead != null)
            {
                FireProjectilesFromHead(centerHead);
                Debug.Log($"ProjectileBarragePattern: Fired {projectilesPerVolley} projectiles from center head");
            }
        }
        else
        {
            foreach (BobbdraHead head in heads)
            {
                if (head != null)
                {
                    FireProjectilesFromHead(head);
                }
            }
        }
    }
    
    private void FireProjectilesFromHead(BobbdraHead head)
    {
        if (simultaneousFire)
        {
            if (projectilesPerVolley == 1)
            {
                Vector3 direction = GetDirectionToPlayer(head);
                head.FireProjectile(direction, projectileSpeed, projectileScale);
            }
            else
            {
                Vector3 baseDirection = GetDirectionToPlayer(head);
                
                float startAngle = -spreadAngle / 2f;
                float angleIncrement = spreadAngle / (projectilesPerVolley - 1);
                
                for (int i = 0; i < projectilesPerVolley; i++)
                {
                    float angle = startAngle + (angleIncrement * i);
                    Vector3 rotatedDirection = Quaternion.Euler(0, 0, angle) * baseDirection;
                    head.FireProjectile(rotatedDirection, projectileSpeed, projectileScale);
                }
            }
        }
        else
        {
            StartCoroutine(FireProjectilesSequentially(head));
        }
    }
    
    private IEnumerator FireProjectilesSequentially(BobbdraHead head)
    {
        for (int i = 0; i < projectilesPerVolley; i++)
        {
            Vector3 direction = GetDirectionToPlayer(head);
            head.FireProjectile(direction, projectileSpeed, projectileScale);
            
            if (i < projectilesPerVolley - 1)
            {
                yield return new WaitForSeconds(delayBetweenShots);
            }
        }
    }
    
    private IEnumerator FireHeadsSequentially()
    {
        if (useOnlyCenterHead)
        {
            if (centerHead != null)
            {
                FireProjectilesFromHead(centerHead);
            }
        }
        else
        {
            foreach (BobbdraHead head in heads)
            {
                if (head != null)
                {
                    FireProjectilesFromHead(head);
                    yield return new WaitForSeconds(delayBetweenShots);
                }
            }
        }
    }
    
    private Vector3 GetDirectionToPlayer(BobbdraHead head)
    {
        if (player != null)
        {
            return (player.position - head.transform.position).normalized;
        }
        
        return Vector3.up;
    }
    
    public override bool IsPatternComplete()
    {
        return patternComplete;
    }
}
