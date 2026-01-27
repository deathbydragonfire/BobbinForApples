using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimationPlayer : MonoBehaviour
{
    [SerializeField] private AnimationClip currentClip;
    [SerializeField] private GameObject animationRoot;
    
    private float currentTime;
    private float previousTime;
    private bool isPlaying;
    private Coroutine playCoroutine;
    private bool shouldSampleThisFrame;
    private float sampleTime;
    private HashSet<int> triggeredEvents = new HashSet<int>();
    
    private void Awake()
    {
        if (animationRoot == null)
        {
            animationRoot = gameObject;
        }
    }
    
    private void LateUpdate()
    {
        if (shouldSampleThisFrame && currentClip != null && animationRoot != null)
        {
            currentClip.SampleAnimation(animationRoot, sampleTime);
            CheckAndTriggerAnimationEvents();
        }
    }
    
    private void CheckAndTriggerAnimationEvents()
    {
        if (currentClip == null || currentClip.events == null || currentClip.events.Length == 0)
            return;
            
        foreach (AnimationEvent animEvent in currentClip.events)
        {
            float eventTime = animEvent.time;
            int eventHash = animEvent.GetHashCode();
            
            if (previousTime < eventTime && currentTime >= eventTime)
            {
                if (!triggeredEvents.Contains(eventHash))
                {
                    triggeredEvents.Add(eventHash);
                    TriggerAnimationEvent(animEvent);
                }
            }
        }
        
        previousTime = currentTime;
    }
    
    private void TriggerAnimationEvent(AnimationEvent animEvent)
    {
        string functionName = animEvent.functionName;
        
        if (string.IsNullOrEmpty(functionName))
            return;
            
        Debug.Log($"[SimpleAnimationPlayer] Triggering animation event: {functionName} at time {currentTime:F3}s");
        
        animationRoot.SendMessage(functionName, animEvent, SendMessageOptions.DontRequireReceiver);
    }
    
    public void Play(AnimationClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SimpleAnimationPlayer.Play() called with null clip");
            return;
        }
        
        if (clip.empty)
        {
            Debug.LogWarning($"SimpleAnimationPlayer: Clip '{clip.name}' is EMPTY (no keyframes)! Cannot play.");
            return;
        }
        
        Debug.Log($"[SimpleAnimationPlayer] Playing clip: {clip.name} (length: {clip.length}s, empty: {clip.empty}, events: {clip.events.Length})");
        Debug.Log($"[SimpleAnimationPlayer] Animation root: {animationRoot.name}, full path: {GetFullPath(animationRoot.transform)}");
        
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }
        
        currentClip = clip;
        triggeredEvents.Clear();
        previousTime = 0f;
        playCoroutine = StartCoroutine(PlayClipCoroutine());
    }
    
    public void Stop()
    {
        isPlaying = false;
        shouldSampleThisFrame = false;
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
    }
    
    public void PlayAndHold(AnimationClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SimpleAnimationPlayer.PlayAndHold() called with null clip");
            return;
        }
        
        if (clip.empty)
        {
            Debug.LogWarning($"SimpleAnimationPlayer: Clip '{clip.name}' is EMPTY (no keyframes)! Cannot play.");
            return;
        }
        
        Debug.Log($"[SimpleAnimationPlayer] Playing and holding clip: {clip.name} (length: {clip.length}s, events: {clip.events.Length})");
        
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }
        
        currentClip = clip;
        triggeredEvents.Clear();
        previousTime = 0f;
        playCoroutine = StartCoroutine(PlayAndHoldClipCoroutine());
    }
    
    private IEnumerator PlayAndHoldClipCoroutine()
    {
        if (currentClip == null)
        {
            Debug.LogWarning("PlayAndHoldClipCoroutine: currentClip is null");
            yield break;
        }
        
        isPlaying = true;
        currentTime = 0f;
        float clipLength = currentClip.length;
        
        while (currentTime < clipLength && isPlaying)
        {
            sampleTime = currentTime;
            shouldSampleThisFrame = true;
            currentTime += Time.deltaTime;
            yield return null;
        }
        
        if (isPlaying)
        {
            sampleTime = clipLength;
            shouldSampleThisFrame = true;
        }
        
        Debug.Log($"[SimpleAnimationPlayer] Finished playback of {currentClip.name}, holding final frame");
        
        isPlaying = false;
    }
    
    private IEnumerator PlayClipCoroutine()
    {
        if (currentClip == null)
        {
            Debug.LogWarning("PlayClipCoroutine: currentClip is null");
            yield break;
        }
        
        isPlaying = true;
        currentTime = 0f;
        float clipLength = currentClip.length;
        
        int frameCount = 0;
        
        Transform bone4 = animationRoot.transform.Find("bone_1/bone_2/bone_3/bone_4");
        Vector3 startPos = bone4 != null ? bone4.localPosition : Vector3.zero;
        
        Debug.Log($"[SimpleAnimationPlayer] Starting animation. bone_4 start pos: {startPos}");
        
        while (currentTime < clipLength && isPlaying)
        {
            sampleTime = currentTime;
            shouldSampleThisFrame = true;
            
            if (bone4 != null && frameCount % 20 == 0)
            {
                Debug.Log($"[SimpleAnimationPlayer] Frame {frameCount}: bone_4 pos = {bone4.localPosition} (delta: {bone4.localPosition - startPos})");
            }
            
            currentTime += Time.deltaTime;
            frameCount++;
            yield return null;
        }
        
        if (isPlaying)
        {
            sampleTime = clipLength;
            shouldSampleThisFrame = true;
            yield return null;
        }
        
        if (bone4 != null)
        {
            Debug.Log($"[SimpleAnimationPlayer] Final bone_4 pos: {bone4.localPosition} (delta: {bone4.localPosition - startPos})");
        }
        
        Debug.Log($"[SimpleAnimationPlayer] Finished playback of {currentClip.name} ({frameCount} frames sampled)");
        
        shouldSampleThisFrame = false;
        isPlaying = false;
    }
    
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    public float GetClipLength(AnimationClip clip)
    {
        return clip != null ? clip.length : 0f;
    }
    
    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
