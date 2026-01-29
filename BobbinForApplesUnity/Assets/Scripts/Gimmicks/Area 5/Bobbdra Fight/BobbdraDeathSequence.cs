using System.Collections;
using UnityEngine;

public class BobbdraDeathSequence : MonoBehaviour
{
    [Header("Death Animation Clips")]
    [SerializeField] private AnimationClip outerDeathsClip;
    [SerializeField] private AnimationClip centerDeathClip;
    
    [Header("Head Animation Players")]
    [SerializeField] private SimpleAnimationPlayer leftHeadPlayer;
    [SerializeField] private SimpleAnimationPlayer rightHeadPlayer;
    [SerializeField] private SimpleAnimationPlayer centerHeadPlayer;
    
    [Header("Head Animators (for disabling idle)")]
    [SerializeField] private BobbdraHeadAnimator leftHeadAnimator;
    [SerializeField] private BobbdraHeadAnimator rightHeadAnimator;
    [SerializeField] private BobbdraHeadAnimator centerHeadAnimator;
    
    [Header("Death Settings")]
    [SerializeField] private float sinkDistance = -25f;
    [SerializeField] private float sinkSpeed = 10f;
    
    [Header("Boss Entry Animation")]
    [SerializeField] private GameObject bossPreviewPrefab;
    [SerializeField] private Transform heartSpawnPoint;
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float bossSpawnDelay = 0.5f;
    [SerializeField] private BossCinematicSequence cinematicSequence;
    
    private Vector3 spawnPosition;
    private bool isDeathSequenceActive;
    private GameObject spawnedBoss;
    
    private void Start()
    {
        spawnPosition = transform.position;
    }
    
    public void PlayDeathSequence()
    {
        if (isDeathSequenceActive)
        {
            return;
        }
        
        isDeathSequenceActive = true;
        StartCoroutine(ExecuteDeathSequence());
    }
    
    private IEnumerator ExecuteDeathSequence()
    {
        Debug.Log("Bobbdra Death Sequence: Starting");
        
        DisableIdleAnimations();
        
        float outerDeathDuration = 0f;
        
        if (outerDeathsClip != null && leftHeadPlayer != null && rightHeadPlayer != null)
        {
            Debug.Log("Playing Outer Deaths animation");
            
            leftHeadPlayer.PlayAndHold(outerDeathsClip);
            rightHeadPlayer.PlayAndHold(outerDeathsClip);
            
            outerDeathDuration = outerDeathsClip.length;
            yield return new WaitForSeconds(outerDeathDuration);
        }
        else
        {
            Debug.LogWarning("Outer Deaths animation or animation players not assigned");
            yield return new WaitForSeconds(1.5f);
        }
        
        if (centerDeathClip != null && centerHeadPlayer != null)
        {
            Debug.Log("Playing Center Death animation");
            
            centerHeadPlayer.PlayAndHold(centerDeathClip);
            
            StartCoroutine(SpawnBossDuringAnimation());
            
            float centerDeathDuration = centerDeathClip.length;
            yield return new WaitForSeconds(centerDeathDuration);
        }
        else
        {
            Debug.LogWarning("Center Death animation or animation player not assigned");
            yield return new WaitForSeconds(2f);
        }
        
        Debug.Log("Starting sink sequence");
        yield return StartCoroutine(SinkBobbdra());
        
        Debug.Log("Bobbdra Death Sequence: Complete");
    }
    
    private void DisableIdleAnimations()
    {
        if (leftHeadAnimator != null)
        {
            leftHeadAnimator.enabled = false;
        }
        
        if (rightHeadAnimator != null)
        {
            rightHeadAnimator.enabled = false;
        }
        
        if (centerHeadAnimator != null)
        {
            centerHeadAnimator.enabled = false;
        }
        
        Debug.Log("Disabled idle animations on all heads");
    }
    
    private IEnumerator SinkBobbdra()
    {
        Vector3 targetPosition = spawnPosition + new Vector3(0, sinkDistance, 0);
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, sinkSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = targetPosition;
    }
    
    private IEnumerator SpawnBossDuringAnimation()
    {
        yield return new WaitForSeconds(bossSpawnDelay);
        
        if (bossPreviewPrefab == null)
        {
            Debug.LogWarning("Boss Preview prefab not assigned in BobbdraDeathSequence");
            yield break;
        }
        
        if (arenaCenter == null)
        {
            GameObject arenaCenterGO = GameObject.Find("ArenaCenter");
            if (arenaCenterGO != null)
            {
                arenaCenter = arenaCenterGO.transform;
                Debug.Log($"Found ArenaCenter GameObject. Local pos: {arenaCenter.localPosition}, World pos: {arenaCenter.position}");
            }
            else
            {
                Debug.LogError("ArenaCenter GameObject not found in scene!");
            }
        }
        
        Vector3 spawnLocation = heartSpawnPoint != null ? heartSpawnPoint.position : transform.position;
        
        Debug.Log($"Spawning preview boss at {spawnLocation}");
        spawnedBoss = Instantiate(bossPreviewPrefab, spawnLocation, Quaternion.identity);
        
        BossEntryAnimation entryAnimation = spawnedBoss.GetComponent<BossEntryAnimation>();
        if (entryAnimation == null)
        {
            entryAnimation = spawnedBoss.AddComponent<BossEntryAnimation>();
        }
        
        if (cinematicSequence == null)
        {
            cinematicSequence = FindFirstObjectByType<BossCinematicSequence>();
            Debug.Log($"Found BossCinematicSequence: {cinematicSequence != null}");
        }
        
        if (cinematicSequence != null && entryAnimation != null)
        {
            typeof(BossEntryAnimation).GetField("cinematicSequence", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(entryAnimation, cinematicSequence);
            Debug.Log("BossCinematicSequence assigned to BossEntryAnimation");
        }
        
        Vector3 arenaCenterPosition = arenaCenter != null ? arenaCenter.position : Vector3.zero;
        Debug.Log($"Arena center Transform is {(arenaCenter != null ? "assigned" : "NULL")}, position: {arenaCenterPosition}");
        entryAnimation.PlayEntryAnimation(arenaCenterPosition);
    }
}
