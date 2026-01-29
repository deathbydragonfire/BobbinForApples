using System.Collections;
using UnityEngine;

public class BossEntryAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float shootOutSpeed = 8f;
    [SerializeField] private float shootOutDistance = 8f;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Rise Settings")]
    [SerializeField] private float riseToArenaSpeed = 2f;
    [SerializeField] private Vector3 arenaCenter = Vector3.zero;
    
    [Header("Cinematic Settings")]
    [SerializeField] private BossCinematicSequence cinematicSequence;
    [SerializeField] private bool triggerCinematicAfterRise = true;
    
    private Vector3 targetScale;
    private bool entryAnimationComplete = false;
    
    private void Awake()
    {
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }
    
    public void PlayEntryAnimation()
    {
        StartCoroutine(EntrySequence());
    }
    
    public void PlayEntryAnimation(Vector3 customArenaCenter)
    {
        arenaCenter = customArenaCenter;
        Debug.Log($"BossEntryAnimation: PlayEntryAnimation called with arena center {arenaCenter}, cinematicSequence: {cinematicSequence != null}, triggerCinematic: {triggerCinematicAfterRise}");
        StartCoroutine(EntrySequence());
    }
    
    private IEnumerator EntrySequence()
    {
        Debug.Log($"BossEntryAnimation: EntrySequence started");
        Vector3 startPosition = transform.position;
        
        yield return StartCoroutine(ScaleUp());
        
        yield return StartCoroutine(ShootOut(startPosition));
        
        yield return StartCoroutine(RiseToArenaCenter());
        
        entryAnimationComplete = true;
        Debug.Log($"Boss entry animation complete, triggerCinematic: {triggerCinematicAfterRise}, cinematicSequence: {cinematicSequence != null}");
        
        if (triggerCinematicAfterRise && cinematicSequence != null)
        {
            BossArenaManager arenaManager = FindFirstObjectByType<BossArenaManager>();
            Debug.Log($"Triggering cinematic sequence, arenaManager: {arenaManager != null}");
            cinematicSequence.PlayCinematicSequence(gameObject, arenaManager);
        }
        else
        {
            if (!triggerCinematicAfterRise)
                Debug.LogWarning("triggerCinematicAfterRise is false");
            if (cinematicSequence == null)
                Debug.LogWarning("cinematicSequence is null!");
        }
    }
    
    private IEnumerator ScaleUp()
    {
        Debug.Log("BossEntryAnimation: ScaleUp started");
        float elapsedTime = 0f;
        
        while (elapsedTime < scaleUpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scaleUpDuration;
            float curveValue = scaleCurve.Evaluate(t);
            transform.localScale = targetScale * curveValue;
            yield return null;
        }
        Debug.Log("BossEntryAnimation: ScaleUp complete");
        
        transform.localScale = targetScale;
    }
    
    private IEnumerator ShootOut(Vector3 startPosition)
    {
        Debug.Log("BossEntryAnimation: ShootOut started");
        Vector3 targetPosition = startPosition + Vector3.up * shootOutDistance;
        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = distance / shootOutSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
            yield return null;
        }
        
        transform.position = targetPosition;
        Debug.Log("BossEntryAnimation: ShootOut complete");
    }
    
    private IEnumerator RiseToArenaCenter()
    {
        Debug.Log($"BossEntryAnimation: RiseToArenaCenter started. Boss at {transform.position}, target: {arenaCenter}, distance: {Vector3.Distance(transform.position, arenaCenter)}");
        Vector3 startPosition = transform.position;
        
        float elapsedTime = 0f;
        float maxDuration = 30f;
        
        while (Vector3.Distance(transform.position, arenaCenter) > 0.1f)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > maxDuration)
            {
                Debug.LogError($"RiseToArenaCenter timeout! Boss stuck at {transform.position}, target: {arenaCenter}");
                break;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, arenaCenter, riseToArenaSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = arenaCenter;
        Debug.Log($"BossEntryAnimation: RiseToArenaCenter complete. Final position: {transform.position}");
    }
    
    public bool IsEntryAnimationComplete()
    {
        return entryAnimationComplete;
    }
}
