using UnityEngine;

[RequireComponent(typeof(LODGroup))]
public class LODColliderManager : MonoBehaviour
{
    private LODGroup lodGroup;
    private MeshCollider[] lodColliders;
    private int currentLODIndex = -1;
    
    private void Awake()
    {
        lodGroup = GetComponent<LODGroup>();
        CacheColliders();
    }
    
    private void CacheColliders()
    {
        LOD[] lods = lodGroup.GetLODs();
        lodColliders = new MeshCollider[lods.Length];
        
        for (int i = 0; i < lods.Length; i++)
        {
            if (lods[i].renderers.Length > 0)
            {
                GameObject lodObject = lods[i].renderers[0].gameObject;
                
                if (lodObject == gameObject)
                {
                    Debug.LogWarning($"LODColliderManager on {gameObject.name}: LOD renderer is on parent GameObject. Skipping.");
                    continue;
                }
                
                MeshCollider meshCollider = lodObject.GetComponent<MeshCollider>();
                
                if (meshCollider == null)
                {
                    Debug.LogWarning($"LODColliderManager on {gameObject.name}: LOD{i} child '{lodObject.name}' is missing a MeshCollider. Please add colliders using Tools > Add Colliders to All LOD Children");
                }
                
                lodColliders[i] = meshCollider;
            }
        }
    }
    
    private void Update()
    {
        UpdateActiveCollider();
    }
    
    private void UpdateActiveCollider()
    {
        int activeLOD = GetActiveLODIndex();
        
        if (activeLOD != currentLODIndex)
        {
            if (currentLODIndex >= 0 && currentLODIndex < lodColliders.Length && lodColliders[currentLODIndex] != null)
            {
                lodColliders[currentLODIndex].enabled = false;
            }
            
            if (activeLOD >= 0 && activeLOD < lodColliders.Length && lodColliders[activeLOD] != null)
            {
                lodColliders[activeLOD].enabled = true;
            }
            
            currentLODIndex = activeLOD;
        }
    }
    
    private int GetActiveLODIndex()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return 0;
        }
        
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 objectPosition = transform.TransformPoint(lodGroup.localReferencePoint);
        float distance = Vector3.Distance(cameraPosition, objectPosition);
        float relativeDistance = distance / QualitySettings.lodBias / lodGroup.size;
        
        LOD[] lods = lodGroup.GetLODs();
        for (int i = 0; i < lods.Length; i++)
        {
            if (relativeDistance < lods[i].screenRelativeTransitionHeight)
            {
                return i;
            }
        }
        
        return lods.Length - 1;
    }
}
