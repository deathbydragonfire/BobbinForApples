using System.Collections;
using UnityEngine;

public class PlayerTargetFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private bool isFollowing = false;
    
    [Header("Timed Behavior")]
    [SerializeField] private float offDuration = 10f;
    [SerializeField] private float onDuration = 5f;
    [SerializeField] private bool startCycleOnStart = true;
    
    private Transform playerTransform;
    private Coroutine followCycleCoroutine;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("PlayerTargetFollow: Player not found! Make sure a GameObject is tagged as 'Player'.");
        }

        if (startCycleOnStart)
        {
            StartFollowCycle();
        }
    }

    void Update()
    {
        if (isFollowing && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * followSpeed * Time.deltaTime;
        }
    }

    public void StartFollowCycle()
    {
        if (followCycleCoroutine != null)
        {
            StopCoroutine(followCycleCoroutine);
        }
        
        followCycleCoroutine = StartCoroutine(FollowCycleRoutine());
    }

    public void StopFollowCycle()
    {
        if (followCycleCoroutine != null)
        {
            StopCoroutine(followCycleCoroutine);
            followCycleCoroutine = null;
        }
        
        isFollowing = false;
    }

    public void StartFollowingAfterDelay(float delaySeconds, float followDurationSeconds)
    {
        StartCoroutine(DelayedFollowRoutine(delaySeconds, followDurationSeconds));
    }

    private IEnumerator FollowCycleRoutine()
    {
        while (true)
        {
            isFollowing = false;
            yield return new WaitForSeconds(offDuration);
            
            isFollowing = true;
            yield return new WaitForSeconds(onDuration);
        }
    }

    private IEnumerator DelayedFollowRoutine(float delaySeconds, float followDurationSeconds)
    {
        isFollowing = false;
        yield return new WaitForSeconds(delaySeconds);
        
        isFollowing = true;
        yield return new WaitForSeconds(followDurationSeconds);
        
        isFollowing = false;
    }
}
