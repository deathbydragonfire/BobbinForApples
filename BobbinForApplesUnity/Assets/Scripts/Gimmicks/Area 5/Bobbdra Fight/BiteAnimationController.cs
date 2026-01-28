using System.Collections;
using UnityEngine;

public class BiteAnimationController : MonoBehaviour
{
    [Header("Animator Reference")]
    [SerializeField] private Animator headAnimator;
    [SerializeField] private string headLayerName = "Head";
    
    [Header("Animation States")]
    [SerializeField] private string closedIdleStateName = "Bobber_Mouth_Closed_Idle";
    
    [Header("Prediction Settings")]
    [SerializeField] private float predictionTime = 2f;
    
    private int headLayerIndex;
    private float scheduledAttackTime = -1f;
    private bool isWaitingForClosedIdle;
    private bool isFrozenOnClosedIdle;
    private float originalAnimatorSpeed = 1f;
    
    private void Awake()
    {
        if (headAnimator == null)
        {
            headAnimator = GetComponent<Animator>();
        }
        
        if (headAnimator != null)
        {
            headLayerIndex = headAnimator.GetLayerIndex(headLayerName);
            originalAnimatorSpeed = headAnimator.speed;
            
            Debug.Log($"BiteAnimationController ({gameObject.name}): Initialized - Layer '{headLayerName}' index = {headLayerIndex}, Speed = {originalAnimatorSpeed}");
            
            if (headLayerIndex == -1)
            {
                Debug.LogError($"BiteAnimationController ({gameObject.name}): Layer '{headLayerName}' not found in animator!");
            }
        }
        else
        {
            Debug.LogError($"BiteAnimationController: Animator not found on {gameObject.name}");
        }
    }
    
    private void Update()
    {
        if (headAnimator == null || headLayerIndex == -1) return;
        
        if (isWaitingForClosedIdle)
        {
            CheckIfReachedClosedIdle();
        }
    }
    
    public void ScheduleAttack(float delay = 0f)
    {
        float timeUntilAttack = predictionTime + delay;
        scheduledAttackTime = Time.time + timeUntilAttack;
        
        AnimatorStateInfo currentState = headAnimator.GetCurrentAnimatorStateInfo(headLayerIndex);
        float currentNormalizedTime = currentState.normalizedTime % 1f;
        float remainingTimeInLoop = (1f - currentNormalizedTime) * currentState.length;
        
        if (remainingTimeInLoop <= timeUntilAttack)
        {
            isWaitingForClosedIdle = true;
            isFrozenOnClosedIdle = false;
            Debug.Log($"BiteAnimationController: Attack scheduled in {timeUntilAttack}s. Will complete loop ({remainingTimeInLoop:F2}s remaining) and freeze on closed idle.");
        }
        else
        {
            Debug.Log($"BiteAnimationController: Attack scheduled in {timeUntilAttack}s, but loop won't complete in time ({remainingTimeInLoop:F2}s remaining). Animation will continue normally.");
        }
    }
    
    private void CheckIfReachedClosedIdle()
    {
        AnimatorStateInfo currentState = headAnimator.GetCurrentAnimatorStateInfo(headLayerIndex);
        
        if (currentState.IsName(closedIdleStateName))
        {
            float normalizedTime = currentState.normalizedTime % 1f;
            
            if (normalizedTime < 0.1f && !isFrozenOnClosedIdle)
            {
                headAnimator.speed = 0f;
                isFrozenOnClosedIdle = true;
                isWaitingForClosedIdle = false;
                Debug.Log($"BiteAnimationController: Frozen on closed idle at normalized time {normalizedTime:F3}");
            }
        }
    }
    
    public void ReleaseFreeze()
    {
        if (isFrozenOnClosedIdle)
        {
            headAnimator.speed = originalAnimatorSpeed;
            isFrozenOnClosedIdle = false;
            scheduledAttackTime = -1f;
            Debug.Log("BiteAnimationController: Released freeze, resuming normal animation");
        }
    }
    
    public void ResetToClosedIdleStart()
    {
        if (headAnimator == null || headLayerIndex == -1)
        {
            Debug.LogWarning("BiteAnimationController: Cannot reset - animator or layer not found");
            return;
        }
        
        headAnimator.speed = originalAnimatorSpeed;
        isWaitingForClosedIdle = false;
        isFrozenOnClosedIdle = false;
        scheduledAttackTime = -1f;
        
        headAnimator.Play(closedIdleStateName, headLayerIndex, 0f);
        headAnimator.Update(0f);
        
        Debug.Log($"BiteAnimationController ({gameObject.name}): Reset to closed idle start (normalized time 0)");
    }
    
    public void ForceResetImmediate()
    {
        if (headAnimator == null || headLayerIndex == -1)
        {
            Debug.LogWarning("BiteAnimationController: Cannot force reset - animator or layer not found");
            return;
        }
        
        headAnimator.speed = originalAnimatorSpeed;
        isWaitingForClosedIdle = false;
        isFrozenOnClosedIdle = false;
        scheduledAttackTime = -1f;
        
        headAnimator.Play(closedIdleStateName, headLayerIndex, 0f);
        headAnimator.Update(0f);
        headAnimator.Update(0f);
        
        Debug.Log($"BiteAnimationController ({gameObject.name}): Force reset immediate to closed idle");
    }
    
    public void ForceResetDelayed()
    {
        StartCoroutine(ForceResetCoroutine());
    }
    
    private IEnumerator ForceResetCoroutine()
    {
        if (headAnimator == null || headLayerIndex == -1)
        {
            yield break;
        }
        
        headAnimator.speed = originalAnimatorSpeed;
        isWaitingForClosedIdle = false;
        isFrozenOnClosedIdle = false;
        scheduledAttackTime = -1f;
        
        yield return null;
        
        headAnimator.Play(closedIdleStateName, headLayerIndex, 0f);
        headAnimator.Update(0f);
        headAnimator.Update(0f);
        
        Debug.Log($"BiteAnimationController ({gameObject.name}): Force reset delayed to closed idle");
    }
    
    public void CancelScheduledAttack()
    {
        if (isWaitingForClosedIdle || isFrozenOnClosedIdle)
        {
            headAnimator.speed = originalAnimatorSpeed;
            isWaitingForClosedIdle = false;
            isFrozenOnClosedIdle = false;
            scheduledAttackTime = -1f;
            Debug.Log("BiteAnimationController: Canceled scheduled attack");
        }
    }
    
    public bool IsFrozen => isFrozenOnClosedIdle;
    public bool IsWaitingToFreeze => isWaitingForClosedIdle;
}
