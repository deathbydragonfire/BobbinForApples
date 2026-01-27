using UnityEngine;

public class ProjectileBarrageAnimationEvents : MonoBehaviour
{
    [SerializeField] private BobbdraHead bobbdraHead;
    [SerializeField] private ProjectileBarragePattern projectileBarragePattern;
    
    private void Awake()
    {
        if (bobbdraHead == null)
        {
            bobbdraHead = GetComponentInParent<BobbdraHead>();
        }
    }
    
    public void SetProjectileBarragePattern(ProjectileBarragePattern pattern)
    {
        projectileBarragePattern = pattern;
    }
    
    public void OnFireProjectile()
    {
        Debug.Log($"[ANIMATION EVENT] OnFireProjectile() called at time {Time.time}");
        Debug.Log("Note: Animation events are no longer used. Projectiles fire based on delay after animation starts.");
        
        if (bobbdraHead != null)
        {
            bobbdraHead.TriggerProjectileGlow();
        }
        else
        {
            Debug.LogWarning("ProjectileBarrageAnimationEvents: BobbdraHead is null!");
        }
    }
}
