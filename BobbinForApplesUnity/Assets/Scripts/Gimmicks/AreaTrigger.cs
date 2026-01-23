using UnityEngine;
using UnityEngine.Events;

public class AreaTrigger : MonoBehaviour
{
    [Header("Area Settings")]
    public string areaName = "Area";
    
    [Header("Events")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    
    [Header("Debug")]
    public bool showDebugLogs = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugLogs)
            {
                Debug.Log($"Player entered {areaName}");
            }
            
            onPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugLogs)
            {
                Debug.Log($"Player exited {areaName}");
            }
            
            onPlayerExit?.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
        }
    }
}
