using UnityEngine;

public class BobberNeckLookAt : MonoBehaviour
{
    [Header("Look At Settings")]
    [SerializeField] private Transform target;
    
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    
    private Vector3 initialOffsetFromTarget;
    
    private void Start()
    {
        if (target == null)
        {
            target = transform.Find("Circle");
            
            if (target == null)
            {
                target = GameObject.Find("Circle")?.transform;
            }
            
            if (target == null)
            {
                Debug.LogWarning($"BobberNeckLookAt on {gameObject.name}: No target assigned and couldn't find Circle.");
                enabled = false;
                return;
            }
        }
        
        initialOffsetFromTarget = transform.position - target.position;
        
        Debug.Log($"Stored offset from target: {initialOffsetFromTarget}");
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position + initialOffsetFromTarget;
        
        float newX = targetPosition.x;
        
        float newY = Mathf.Lerp(transform.position.y, targetPosition.y, Time.deltaTime * chaseSpeed);
        float newZ = Mathf.Lerp(transform.position.z, targetPosition.z, Time.deltaTime * chaseSpeed);
        
        transform.position = new Vector3(newX, newY, newZ);
    }
}
