using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimationClipInspector : MonoBehaviour
{
    [SerializeField] private AnimationClip clipToInspect;
    [SerializeField] private GameObject targetRoot;
    
    [ContextMenu("Inspect Animation Clip")]
    public void InspectClip()
    {
#if UNITY_EDITOR
        if (clipToInspect == null)
        {
            Debug.LogError("No clip assigned!");
            return;
        }
        
        if (targetRoot == null)
        {
            targetRoot = gameObject;
        }
        
        Debug.Log($"=== Inspecting Clip: {clipToInspect.name} ===");
        Debug.Log($"Target Root: {targetRoot.name}");
        Debug.Log($"Empty: {clipToInspect.empty}");
        Debug.Log($"Length: {clipToInspect.length}");
        
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clipToInspect);
        
        if (bindings.Length == 0)
        {
            Debug.LogWarning("No curve bindings found! This clip has no animation data.");
            return;
        }
        
        Debug.Log($"Found {bindings.Length} curve bindings:");
        
        foreach (var binding in bindings)
        {
            string fullPath = string.IsNullOrEmpty(binding.path) ? "[Root]" : binding.path;
            Debug.Log($"  Path: '{fullPath}' | Property: {binding.propertyName} | Type: {binding.type.Name}");
            
            Transform target = string.IsNullOrEmpty(binding.path) ? 
                targetRoot.transform : 
                targetRoot.transform.Find(binding.path);
            
            if (target == null)
            {
                Debug.LogWarning($"    ⚠️ PATH NOT FOUND in hierarchy!");
            }
            else
            {
                Debug.Log($"    ✓ Found: {target.name}");
            }
        }
#else
        Debug.LogWarning("AnimationClipInspector only works in the Unity Editor");
#endif
    }
}
