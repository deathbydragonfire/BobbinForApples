using System.Collections;
using UnityEngine;

public class SideAmbushPattern : BobbdraAttackPattern
{
    public enum AttackSequence
    {
        TopToBottom,
        BottomToTop,
        Random
    }
    
    [Header("Side Ambush Settings")]
    [SerializeField] private GameObject sideAmbushPrefab;
    [SerializeField] private Transform sideAmbushTopSpawn;
    [SerializeField] private Transform sideAmbushMiddleSpawn;
    [SerializeField] private Transform sideAmbushBottomSpawn;
    [SerializeField] private float disappearDuration = 1.5f;
    [SerializeField] private float delayBetweenHeads = 0.7f;
    [SerializeField] private AttackSequence attackSequence = AttackSequence.TopToBottom;
    [SerializeField] private Vector3 sinkPosition = new Vector3(0f, -8f, 0f);
    [SerializeField] private float attackAnimationDuration = 2.84f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float attackIndicatorDelay = 0.5f;
    
    private const string ATTACK_ANIMATION_NAME = "SideAmbush Attack";
    private const string INDICATOR_ANIMATION_NAME = "Attack Indicator";
    
    private bool patternComplete;
    private Vector3 originalPosition;
    private GameObject[] activeAmbushInstances = new GameObject[3];
    
    public override void Initialize(BobbdraManager bobbdraManager, BobbdraHead[] bobbdraHeads)
    {
        base.Initialize(bobbdraManager, bobbdraHeads);
        
        if (sideAmbushTopSpawn == null)
        {
            GameObject topSpawn = GameObject.Find("SideAmbushTop");
            if (topSpawn != null)
            {
                sideAmbushTopSpawn = topSpawn.transform;
            }
            else
            {
                Debug.LogWarning("SideAmbushPattern: SideAmbushTop not found in scene");
            }
        }
        
        if (sideAmbushMiddleSpawn == null)
        {
            GameObject middleSpawn = GameObject.Find("SideAmbushMiddle");
            if (middleSpawn != null)
            {
                sideAmbushMiddleSpawn = middleSpawn.transform;
            }
            else
            {
                Debug.LogWarning("SideAmbushPattern: SideAmbushMiddle not found in scene");
            }
        }
        
        if (sideAmbushBottomSpawn == null)
        {
            GameObject bottomSpawn = GameObject.Find("SideAmbushBottom");
            if (bottomSpawn != null)
            {
                sideAmbushBottomSpawn = bottomSpawn.transform;
            }
            else
            {
                Debug.LogWarning("SideAmbushPattern: SideAmbushBottom not found in scene");
            }
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
        originalPosition = manager.transform.position;
        
        StartCoroutine(ExecuteSideAmbush());
    }
    
    private IEnumerator ExecuteSideAmbush()
    {
        Debug.Log("Executing Side Ambush Pattern");
        
        manager.SetInvincible(true);
        
        yield return StartCoroutine(DisappearPhase());
        
        yield return StartCoroutine(EmergencePhase());
        
        yield return StartCoroutine(ReturnPhase());
        
        manager.SetInvincible(false);
        
        patternComplete = true;
        isExecuting = false;
    }
    
    private IEnumerator DisappearPhase()
    {
        float elapsed = 0f;
        Vector3 startPosition = manager.transform.position;
        Vector3 targetPosition = startPosition + sinkPosition;
        
        while (elapsed < disappearDuration)
        {
            manager.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / disappearDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        manager.transform.position = targetPosition;
    }
    
    private IEnumerator EmergencePhase()
    {
        if (sideAmbushPrefab == null)
        {
            Debug.LogError("Side Ambush: Prefab not assigned");
            yield break;
        }
        
        if (sideAmbushTopSpawn == null || sideAmbushMiddleSpawn == null || sideAmbushBottomSpawn == null)
        {
            Debug.LogError("Side Ambush: Spawn points not assigned");
            yield break;
        }
        
        yield return StartCoroutine(SpawnSideAmbush(sideAmbushTopSpawn, false, 0));
        yield return new WaitForSeconds(delayBetweenHeads);
        
        yield return StartCoroutine(SpawnSideAmbush(sideAmbushMiddleSpawn, true, 1));
        yield return new WaitForSeconds(delayBetweenHeads);
        
        yield return StartCoroutine(SpawnSideAmbush(sideAmbushBottomSpawn, false, 2));
    }
    
    private IEnumerator SpawnSideAmbush(Transform spawnPoint, bool fromLeft, int instanceIndex)
    {
        GameObject ambushInstance = Instantiate(sideAmbushPrefab);
        activeAmbushInstances[instanceIndex] = ambushInstance;
        
        ambushInstance.transform.position = spawnPoint.position;
        ambushInstance.transform.rotation = spawnPoint.rotation;
        
        if (!fromLeft)
        {
            ambushInstance.transform.localScale = new Vector3(
                -ambushInstance.transform.localScale.x,
                ambushInstance.transform.localScale.y,
                ambushInstance.transform.localScale.z
            );
        }
        
        Animator indicatorAnimator = null;
        Animator attackAnimator = null;
        
        Animator[] animators = ambushInstance.GetComponentsInChildren<Animator>();
        foreach (Animator anim in animators)
        {
            if (anim.gameObject.name == "Side Attack Indicator")
            {
                indicatorAnimator = anim;
            }
            else if (anim.gameObject.name == "The Bobber Puppet")
            {
                attackAnimator = anim;
                attackAnimator.enabled = false;
            }
        }
        
        if (indicatorAnimator != null)
        {
            indicatorAnimator.Rebind();
            indicatorAnimator.Play(INDICATOR_ANIMATION_NAME, 0, 0f);
        }
        else
        {
            Debug.LogWarning("Side Ambush: Attack Indicator animator not found");
        }
        
        yield return new WaitForSeconds(attackIndicatorDelay);
        
        if (attackAnimator != null)
        {
            attackAnimator.enabled = true;
            attackAnimator.Rebind();
            attackAnimator.Play(ATTACK_ANIMATION_NAME, 0, 0f);
        }
        else
        {
            Debug.LogWarning("Side Ambush: Attack animator not found on prefab");
        }
        
        yield return new WaitForSeconds(attackAnimationDuration);
        
        Destroy(ambushInstance);
        activeAmbushInstances[instanceIndex] = null;
    }
    
    private IEnumerator ReturnPhase()
    {
        float elapsed = 0f;
        Vector3 startPosition = manager.transform.position;
        
        while (elapsed < disappearDuration)
        {
            manager.transform.position = Vector3.Lerp(startPosition, originalPosition, elapsed / disappearDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        manager.transform.position = originalPosition;
    }
    
    public override bool IsPatternComplete()
    {
        return patternComplete;
    }
    
    public override void StopPattern()
    {
        base.StopPattern();
        
        if (manager != null)
        {
            manager.transform.position = originalPosition;
            manager.SetInvincible(false);
        }
        
        for (int i = 0; i < activeAmbushInstances.Length; i++)
        {
            if (activeAmbushInstances[i] != null)
            {
                Destroy(activeAmbushInstances[i]);
                activeAmbushInstances[i] = null;
            }
        }
        
        patternComplete = true;
    }
}
