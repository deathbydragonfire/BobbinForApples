using System.Collections;
using UnityEngine;

public class BobbdraHeadAnimator : MonoBehaviour
{
    [Header("Bone References")]
    [SerializeField] private Transform bone1;
    [SerializeField] private Transform bone2;
    [SerializeField] private Transform bone3;
    [SerializeField] private Transform bone4;
    
    [Header("Stretch Settings")]
    [SerializeField] private float maxStretchDistance = 6f;
    [SerializeField] private float stretchSpeed = 8f;
    
    [Header("Idle Animation")]
    [SerializeField] private float idleSwayAmount = 0.1f;
    [SerializeField] private float idleSwaySpeed = 1f;
    [SerializeField] private float idleBreathAmount = 0.05f;
    [SerializeField] private float idleBreathSpeed = 2f;
    
    private Transform[] bones;
    private Vector3[] originalBoneScales;
    private Vector3[] originalBonePositions;
    private Quaternion[] originalBoneRotations;
    private Vector3 originalParentPosition;
    private float currentStretchAmount;
    private bool isStretching;
    private float idleTimeOffset;
    
    private void Awake()
    {
        bones = new Transform[] { bone1, bone2, bone3, bone4 };
        
        originalBoneScales = new Vector3[bones.Length];
        originalBonePositions = new Vector3[bones.Length];
        originalBoneRotations = new Quaternion[bones.Length];
        
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                originalBoneScales[i] = bones[i].localScale;
                originalBonePositions[i] = bones[i].localPosition;
                originalBoneRotations[i] = bones[i].localRotation;
            }
        }
        
        if (transform.parent != null)
        {
            originalParentPosition = transform.parent.localPosition;
        }
        
        idleTimeOffset = UnityEngine.Random.Range(0f, 100f);
    }
    
    private void Update()
    {
        if (!isStretching)
        {
            ApplyIdleAnimation();
        }
        else
        {
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[BobbdraHeadAnimator] {gameObject.name}: isStretching=true, skipping idle");
            }
        }
    }
    
    private void ApplyIdleAnimation()
    {
        float time = Time.time + idleTimeOffset;
        
        float swayX = Mathf.Sin(time * idleSwaySpeed) * idleSwayAmount;
        float breathY = Mathf.Sin(time * idleBreathSpeed) * idleBreathAmount;
        
        if (bone1 != null)
        {
            bone1.localRotation = originalBoneRotations[0] * Quaternion.Euler(0, 0, swayX * 15f);
        }
        
        if (bone2 != null)
        {
            bone2.localRotation = originalBoneRotations[1] * Quaternion.Euler(0, 0, swayX * -10f);
        }
        
        if (transform.parent != null)
        {
            Vector3 parentPos = originalParentPosition;
            parentPos.y += breathY;
            transform.parent.localPosition = parentPos;
        }
    }
    
    public void StretchVertical(float targetDistance, float duration)
    {
        StartCoroutine(StretchVerticalCoroutine(targetDistance, duration));
    }
    
    public void LungeToBite(Vector3 targetOffset, float duration)
    {
        StartCoroutine(LungeCoroutine(targetOffset, duration));
    }
    
    private IEnumerator LungeCoroutine(Vector3 targetOffset, float duration)
    {
        isStretching = true;
        float elapsed = 0f;
        
        Vector3 startOffset = Vector3.zero;
        if (bone4 != null)
        {
            startOffset = bone4.localPosition - originalBonePositions[3];
        }
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 currentOffset = Vector3.Lerp(startOffset, targetOffset, t);
            
            if (bone4 != null)
            {
                bone4.localPosition = originalBonePositions[3] + currentOffset;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (bone4 != null)
        {
            bone4.localPosition = originalBonePositions[3] + targetOffset;
        }
        
        isStretching = false;
    }
    
    private IEnumerator StretchVerticalCoroutine(float targetDistance, float duration)
    {
        isStretching = true;
        
        float elapsed = 0f;
        float startStretch = currentStretchAmount;
        
        while (elapsed < duration)
        {
            currentStretchAmount = Mathf.Lerp(startStretch, targetDistance, elapsed / duration);
            ApplyVerticalStretch(currentStretchAmount);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentStretchAmount = targetDistance;
        ApplyVerticalStretch(currentStretchAmount);
        
        isStretching = false;
    }
    
    private void ApplyVerticalStretch(float stretchDistance)
    {
        float stretchPerBone = stretchDistance / bones.Length;
        
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                float scaleMultiplier = 1f + (stretchPerBone / 2f);
                bones[i].localScale = new Vector3(
                    originalBoneScales[i].x,
                    originalBoneScales[i].y * scaleMultiplier,
                    originalBoneScales[i].z
                );
            }
        }
    }
    
    public void ResetToRestPosition(float duration)
    {
        StartCoroutine(ResetToRestCoroutine(duration));
    }
    
    private IEnumerator ResetToRestCoroutine(float duration)
    {
        isStretching = true;
        
        float elapsed = 0f;
        float startStretch = currentStretchAmount;
        Vector3 startBone4Pos = bone4 != null ? bone4.localPosition : Vector3.zero;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            currentStretchAmount = Mathf.Lerp(startStretch, 0f, t);
            ApplyVerticalStretch(currentStretchAmount);
            
            if (bone4 != null)
            {
                bone4.localPosition = Vector3.Lerp(startBone4Pos, originalBonePositions[3], t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        currentStretchAmount = 0f;
        
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                bones[i].localScale = originalBoneScales[i];
                bones[i].localPosition = originalBonePositions[i];
                bones[i].localRotation = originalBoneRotations[i];
            }
        }
        
        isStretching = false;
    }
    
    public void SetIdleEnabled(bool enabled)
    {
        if (!enabled)
        {
            isStretching = true;
            
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null)
                {
                    bones[i].localRotation = originalBoneRotations[i];
                }
            }
            
            if (transform.parent != null)
            {
                transform.parent.localPosition = originalParentPosition;
            }
        }
        else
        {
            isStretching = false;
        }
    }
}
