using UnityEngine;
using System.Collections.Generic;

public class DepthBasedColliderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private float waterPlaneZPosition = 3f;
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private bool debugMode = false;
    
    [Header("Exclusions")]
    [SerializeField] private string[] excludedParentNames = { "Arena", "Bobbdra", "Boss" };
    
    private List<ColliderInfo> trackedColliders = new List<ColliderInfo>();
    private float lastUpdateTime;
    
    private class ColliderInfo
    {
        public Collider collider;
        public Bounds originalBounds;
        public Transform parent;
        
        public ColliderInfo(Collider col)
        {
            collider = col;
            
            if (col is MeshCollider meshCol && meshCol.sharedMesh != null)
            {
                originalBounds = meshCol.sharedMesh.bounds;
            }
            else if (col is BoxCollider boxCol)
            {
                originalBounds = new Bounds(boxCol.center, boxCol.size);
            }
            
            parent = col.transform;
        }
        
        public Vector3 GetWorldMinZ()
        {
            Vector3 worldMin = parent.TransformPoint(originalBounds.min);
            return worldMin;
        }
        
        public Vector3 GetWorldMaxZ()
        {
            Vector3 worldMax = parent.TransformPoint(originalBounds.max);
            return worldMax;
        }
    }
    
    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("DepthBasedColliderController: Player not found!");
                return;
            }
        }
        
        FindAndTrackColliders();
    }
    
    private void FindAndTrackColliders()
    {
        Collider[] allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        
        foreach (Collider col in allColliders)
        {
            if (ShouldTrackCollider(col))
            {
                trackedColliders.Add(new ColliderInfo(col));
                
                if (debugMode)
                {
                    Debug.Log($"Tracking collider: {GetFullPath(col.transform)}");
                }
            }
        }
        
        Debug.Log($"DepthBasedColliderController: Tracking {trackedColliders.Count} colliders");
    }
    
    private bool ShouldTrackCollider(Collider col)
    {
        if (col == null || col.isTrigger)
        {
            return false;
        }
        
        Transform current = col.transform;
        while (current != null)
        {
            foreach (string excludedName in excludedParentNames)
            {
                if (current.name.Contains(excludedName))
                {
                    return false;
                }
            }
            
            if (current.name == "Bobber")
            {
                return false;
            }
            
            if (current.name == "Player")
            {
                return false;
            }
            
            current = current.parent;
        }
        
        return true;
    }
    
    private void Update()
    {
        if (player == null || Time.time - lastUpdateTime < updateInterval)
        {
            return;
        }
        
        lastUpdateTime = Time.time;
        UpdateColliderStates();
    }
    
    private void UpdateColliderStates()
    {
        foreach (ColliderInfo info in trackedColliders)
        {
            if (info.collider == null)
            {
                continue;
            }
            
            Vector3 worldMin = info.GetWorldMinZ();
            Vector3 worldMax = info.GetWorldMaxZ();
            
            bool hasPartBelowWater = worldMin.z <= waterPlaneZPosition;
            
            bool shouldBeEnabled = hasPartBelowWater;
            
            if (info.collider.enabled != shouldBeEnabled)
            {
                info.collider.enabled = shouldBeEnabled;
                
                if (debugMode)
                {
                    Debug.Log($"{info.collider.name}: {(shouldBeEnabled ? "ENABLED" : "DISABLED")} (min.z={worldMin.z:F2}, max.z={worldMax.z:F2}, waterZ={waterPlaneZPosition})");
                }
            }
        }
    }
    
    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        Transform current = transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
    
    public void SetWaterPlaneZPosition(float zPos)
    {
        waterPlaneZPosition = zPos;
    }
}
