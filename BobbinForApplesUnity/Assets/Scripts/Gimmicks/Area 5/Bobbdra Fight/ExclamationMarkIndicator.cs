using UnityEngine;

public class ExclamationMarkIndicator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float scaleInDuration = 0.2f;
    
    private Vector3 startPosition;
    private Vector3 targetScale;
    private float elapsedTime;
    
    private void Start()
    {
        startPosition = transform.position;
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }
    
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime < scaleInDuration)
        {
            float t = elapsedTime / scaleInDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
        }
        else
        {
            transform.localScale = targetScale;
            float offset = Mathf.Sin((elapsedTime - scaleInDuration) * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * offset;
        }
    }
}
