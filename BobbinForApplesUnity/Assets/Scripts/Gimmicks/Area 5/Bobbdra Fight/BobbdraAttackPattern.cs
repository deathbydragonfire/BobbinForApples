using UnityEngine;

public abstract class BobbdraAttackPattern : MonoBehaviour
{
    [Header("Pattern Settings")]
    [SerializeField] protected float baseCooldownDuration = 5f;
    
    protected BobbdraManager manager;
    protected BobbdraHead[] heads;
    protected bool isExecuting;
    protected Transform arenaTransform;
    protected BoxCollider arenaCollider;
    
    public virtual void Initialize(BobbdraManager bobbdraManager, BobbdraHead[] bobbdraHeads)
    {
        manager = bobbdraManager;
        heads = bobbdraHeads;
        
        GameObject arena = GameObject.Find("Arena");
        if (arena != null)
        {
            arenaTransform = arena.transform;
            arenaCollider = arena.GetComponent<BoxCollider>();
            Debug.Log($"BobbdraAttackPattern: Found Arena at {arenaTransform.position}");
        }
        else
        {
            Debug.LogWarning("BobbdraAttackPattern: Arena not found!");
        }
    }
    
    public abstract void ExecutePattern();
    
    public abstract bool IsPatternComplete();
    
    public virtual void StopPattern()
    {
        if (isExecuting)
        {
            StopAllCoroutines();
            isExecuting = false;
            Debug.Log($"{GetType().Name}: Pattern stopped");
        }
    }
    
    public virtual float GetCooldownDuration()
    {
        return baseCooldownDuration;
    }
    
    protected bool AreAllHeadsReady()
    {
        foreach (BobbdraHead head in heads)
        {
            if (head != null && head.IsAttacking)
            {
                return false;
            }
        }
        return true;
    }
}
