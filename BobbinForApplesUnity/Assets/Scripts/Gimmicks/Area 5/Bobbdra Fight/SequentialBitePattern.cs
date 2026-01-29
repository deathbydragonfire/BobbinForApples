using System.Collections;
using UnityEngine;

public class SequentialBitePattern : BobbdraAttackPattern
{
    public enum SequenceType
    {
        LeftToRight,
        RightToLeft,
        CenterOutward
    }
    
    [Header("Sequential Bite Settings")]
    [SerializeField] private float delayBetweenHeads = 0.5f;
    [SerializeField] private bool randomizeSequence = true;
    [SerializeField] private float delayBetweenIndicators = 0.5f;
    [SerializeField] private float delayBeforeAttacks = 0.5f;
    
    private SequenceType sequenceType = SequenceType.LeftToRight;
    private bool patternComplete;
    
    public override void ExecutePattern()
    {
        if (!AreAllHeadsReady())
        {
            return;
        }
        
        isExecuting = true;
        patternComplete = false;
        
        if (randomizeSequence)
        {
            RandomizeSequenceType();
        }
        
        StartCoroutine(ExecuteSequentialBite());
    }
    
    private void RandomizeSequenceType()
    {
        int randomIndex = Random.Range(0, 3);
        sequenceType = (SequenceType)randomIndex;
        Debug.Log($"Randomized sequence type to: {sequenceType}");
    }
    
    private IEnumerator ExecuteSequentialBite()
    {
        Debug.Log($"Executing Sequential Bite Pattern: {sequenceType}");
        
        BobbdraHead[] sequence = GetHeadSequence();
        
        Debug.Log("Phase 1: Showing all indicators in sequence");
        foreach (BobbdraHead head in sequence)
        {
            if (head != null)
            {
                head.ShowAttackIndicator();
                yield return new WaitForSeconds(delayBetweenIndicators);
            }
        }
        
        yield return new WaitForSeconds(delayBeforeAttacks);
        
        Debug.Log("Phase 2: Executing attacks in sequence");
        foreach (BobbdraHead head in sequence)
        {
            if (head != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(AudioEventType.BossBite, head.transform.position);
                }
                
                Vector3 biteTarget = head.transform.position + Vector3.up * 6f;
                head.PerformBiteAttack(biteTarget, skipIdleControl: true);
                yield return new WaitForSeconds(delayBetweenHeads);
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        patternComplete = true;
        isExecuting = false;
    }
    
    private BobbdraHead[] GetHeadSequence()
    {
        switch (sequenceType)
        {
            case SequenceType.LeftToRight:
                return new BobbdraHead[] { heads[0], heads[1], heads[2] };
            
            case SequenceType.RightToLeft:
                return new BobbdraHead[] { heads[2], heads[1], heads[0] };
            
            case SequenceType.CenterOutward:
                return new BobbdraHead[] { heads[1], heads[0], heads[2] };
            
            default:
                return heads;
        }
    }
    
    public override bool IsPatternComplete()
    {
        return patternComplete;
    }
    
    public void SetSequenceType(SequenceType type)
    {
        sequenceType = type;
        randomizeSequence = false;
    }
}
