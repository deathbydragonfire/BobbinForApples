using System.Collections;
using UnityEngine;

public class DownAttackPattern : BobbdraAttackPattern
{
    [Header("Down Attack Settings")]
    [SerializeField] private GameObject downAttackPrefab;
    [SerializeField] private Transform downAttackSpawnPoint;
    [SerializeField] private float attackAnimationDuration = 3.84f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject attackIndicator;
    [SerializeField] private int blinkCount = 5;
    [SerializeField] private float blinkDuration = 0.2f;
    [SerializeField] private float attackIndicatorDelay = 1f;
    
    [Header("Disappear/Reappear Settings")]
    [SerializeField] private float disappearDuration = 1.5f;
    [SerializeField] private Vector3 sinkPosition = new Vector3(0f, -20f, 0f);
    
    private const string ATTACK_ANIMATION_NAME = "Bobbdra Down Attack";
    
    private bool patternComplete;
    private GameObject activeAttackInstance;
    private Vector3 originalPosition;
    
    public override void Initialize(BobbdraManager bobbdraManager, BobbdraHead[] bobbdraHeads)
    {
        base.Initialize(bobbdraManager, bobbdraHeads);
        
        if (downAttackSpawnPoint == null)
        {
            GameObject spawnPoint = GameObject.Find("DownAttackPoint");
            if (spawnPoint != null)
            {
                downAttackSpawnPoint = spawnPoint.transform;
            }
            else
            {
                Debug.LogWarning("DownAttackPattern: DownAttackPoint not found in scene");
            }
        }
        
        if (attackIndicator == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "Down Attack Indicator" && obj.scene.IsValid())
                {
                    attackIndicator = obj;
                    break;
                }
            }
            
            if (attackIndicator == null)
            {
                Debug.LogWarning("DownAttackPattern: Down Attack Indicator not found in scene");
            }
        }
        
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(false);
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
        
        StartCoroutine(ExecuteDownAttack());
    }
    
    private IEnumerator ExecuteDownAttack()
    {
        Debug.Log("Executing Down Attack Pattern");
        
        originalPosition = manager.transform.position;
        manager.SetInvincible(true);
        
        yield return StartCoroutine(DisappearPhase());
        
        yield return StartCoroutine(ShowIndicator());
        
        yield return StartCoroutine(SpawnDownAttack());
        
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
    
    private IEnumerator ShowIndicator()
    {
        if (attackIndicator == null)
        {
            Debug.LogWarning("Down Attack: Indicator not assigned");
            yield break;
        }
        
        attackIndicator.SetActive(true);
        
        CanvasGroup canvasGroup = attackIndicator.GetComponentInChildren<CanvasGroup>();
        if (canvasGroup == null)
        {
            Canvas canvas = attackIndicator.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (canvasGroup != null)
        {
            for (int i = 0; i < blinkCount; i++)
            {
                canvasGroup.alpha = 0f;
                yield return new WaitForSeconds(blinkDuration);
                canvasGroup.alpha = 1f;
                yield return new WaitForSeconds(blinkDuration);
            }
        }
        else
        {
            Debug.LogWarning("Down Attack: CanvasGroup not found for blinking effect");
            yield return new WaitForSeconds(blinkDuration * blinkCount * 2);
        }
        
        yield return new WaitForSeconds(attackIndicatorDelay);
        
        attackIndicator.SetActive(false);
    }
    
    private IEnumerator SpawnDownAttack()
    {
        if (downAttackPrefab == null)
        {
            Debug.LogError("Down Attack: Prefab not assigned");
            yield break;
        }
        
        if (downAttackSpawnPoint == null)
        {
            Debug.LogError("Down Attack: Spawn point not assigned");
            yield break;
        }
        
        GameObject attackInstance = Instantiate(downAttackPrefab);
        activeAttackInstance = attackInstance;
        
        attackInstance.transform.position = downAttackSpawnPoint.position;
        attackInstance.transform.rotation = downAttackSpawnPoint.rotation;
        
        BubbleParticleEffect bubbleEffect = attackInstance.GetComponentInChildren<BubbleParticleEffect>();
        if (bubbleEffect != null)
        {
            bubbleEffect.PlayBubbleEffect();
        }
        
        Animator attackAnimator = attackInstance.GetComponentInChildren<Animator>();
        
        if (attackAnimator != null)
        {
            attackAnimator.Rebind();
            attackAnimator.Play(ATTACK_ANIMATION_NAME, 0, 0f);
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(AudioEventType.BossDownAttack, downAttackSpawnPoint.position);
                AudioManager.Instance.PlaySound(AudioEventType.BossDownAttackSecondLayer, downAttackSpawnPoint.position);
                AudioManager.Instance.PlaySound(AudioEventType.BossDownAttackThirdLayer, downAttackSpawnPoint.position);
            }
        }
        else
        {
            Debug.LogWarning("Down Attack: Animator not found on prefab");
        }
        
        yield return new WaitForSeconds(attackAnimationDuration);
        
        Destroy(attackInstance);
        activeAttackInstance = null;
    }
    
    public override bool IsPatternComplete()
    {
        return patternComplete;
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
    
    public override void StopPattern()
    {
        base.StopPattern();
        
        if (manager != null)
        {
            manager.transform.position = originalPosition;
            manager.SetInvincible(false);
        }
        
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(false);
        }
        
        if (activeAttackInstance != null)
        {
            Destroy(activeAttackInstance);
            activeAttackInstance = null;
        }
        
        patternComplete = true;
    }
}
