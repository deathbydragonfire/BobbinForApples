using System.Collections.Generic;
using UnityEngine;

public class Area4SonarManager : MonoBehaviour
{
    [Header("Sonar Controller")]
    [SerializeField] private SonarWaveController sonarWaveController;
    
    [Header("Obstacle Setup")]
    [SerializeField] private Transform area4Root;
    [SerializeField] private float fadeDuration = 4f;
    
    [Header("Exclusions")]
    [SerializeField] private List<GameObject> excludedObjects = new List<GameObject>();
    [SerializeField] private List<string> excludedTags = new List<string> { "Player", "MainCamera" };
    
    private List<SonarObstacleIlluminator> illuminators = new List<SonarObstacleIlluminator>();
    private bool isInitialized = false;
    
    private void Awake()
    {
        if (area4Root == null)
        {
            area4Root = transform;
        }
        
        InitializeObstacles();
    }
    
    private void InitializeObstacles()
    {
        if (isInitialized) return;
        
        Renderer[] allRenderers = area4Root.GetComponentsInChildren<Renderer>();
        
        HashSet<GameObject> processedObjects = new HashSet<GameObject>();
        
        foreach (Renderer rend in allRenderers)
        {
            GameObject obj = rend.gameObject;
            
            if (processedObjects.Contains(obj)) continue;
            
            if (IsExcluded(obj)) continue;
            
            GameObject targetObject = obj;
            LODGroup lodGroup = obj.GetComponentInParent<LODGroup>();
            
            if (lodGroup != null)
            {
                targetObject = lodGroup.gameObject;
                
                if (processedObjects.Contains(targetObject)) continue;
            }
            else
            {
                Collider existingCollider = targetObject.GetComponent<Collider>();
                if (existingCollider == null)
                {
                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        MeshCollider meshCollider = targetObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = meshFilter.sharedMesh;
                        meshCollider.convex = false;
                        meshCollider.isTrigger = false;
                    }
                    else
                    {
                        BoxCollider boxCollider = targetObject.AddComponent<BoxCollider>();
                    }
                }
            }
            
            if (targetObject.GetComponent<SonarObstacleIlluminator>() == null)
            {
                SonarObstacleIlluminator illuminator = targetObject.AddComponent<SonarObstacleIlluminator>();
                illuminators.Add(illuminator);
                processedObjects.Add(targetObject);
                Debug.Log($"Added SonarObstacleIlluminator to: {targetObject.name} at position {targetObject.transform.position}");
            }
        }
        
        isInitialized = true;
        
        Debug.Log($"Area 4 Sonar: Initialized {illuminators.Count} obstacles");
    }
    
    private bool IsExcluded(GameObject obj)
    {
        if (excludedObjects.Contains(obj)) return true;
        
        foreach (string tag in excludedTags)
        {
            if (obj.CompareTag(tag)) return true;
        }
        
        if (obj.GetComponent<SonarWaveController>() != null) return true;
        
        return false;
    }
    
    public void EnableSonar()
    {
        if (sonarWaveController != null)
        {
            sonarWaveController.EnableSonar();
            Debug.Log("Area 4 Sonar: Enabled");
        }
        else
        {
            Debug.LogWarning("Area 4 Sonar: SonarWaveController not assigned!");
        }
    }
    
    public void DisableSonar()
    {
        if (sonarWaveController != null)
        {
            sonarWaveController.DisableSonar();
            Debug.Log("Area 4 Sonar: Disabled");
        }
    }
    
    public void PauseSonar()
    {
        if (sonarWaveController != null)
        {
            sonarWaveController.PauseSonar();
            Debug.Log("Area 4 Sonar: Paused");
        }
    }
    
    public void ResumeSonar()
    {
        if (sonarWaveController != null)
        {
            sonarWaveController.ResumeSonar();
            Debug.Log("Area 4 Sonar: Resumed");
        }
    }
    
    public void TriggerInitializationWave()
    {
        if (sonarWaveController != null)
        {
            sonarWaveController.TriggerManualWave();
            Debug.Log("Area 4 Sonar: Initialization wave triggered");
        }
    }
}
