using UnityEngine;
using UnityEditor;

public class AddLODColliders : EditorWindow
{
    [MenuItem("Tools/Add Colliders to All LOD Children")]
    public static void AddCollidersToLODs()
    {
        LODGroup[] lodGroups = FindObjectsByType<LODGroup>(FindObjectsSortMode.None);
        int addedCount = 0;
        
        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();
            
            for (int i = 0; i < lods.Length; i++)
            {
                if (lods[i].renderers.Length > 0)
                {
                    GameObject lodObject = lods[i].renderers[0].gameObject;
                    
                    if (lodObject == lodGroup.gameObject)
                    {
                        continue;
                    }
                    
                    MeshCollider meshCollider = lodObject.GetComponent<MeshCollider>();
                    
                    if (meshCollider == null)
                    {
                        MeshFilter meshFilter = lodObject.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            meshCollider = lodObject.AddComponent<MeshCollider>();
                            meshCollider.sharedMesh = meshFilter.sharedMesh;
                            meshCollider.convex = false;
                            meshCollider.enabled = i == 0;
                            addedCount++;
                            EditorUtility.SetDirty(lodObject);
                        }
                    }
                    else
                    {
                        meshCollider.enabled = i == 0;
                        EditorUtility.SetDirty(lodObject);
                    }
                }
            }
        }
        
        Debug.Log($"Added {addedCount} MeshColliders to LOD children. Only LOD0 colliders are enabled.");
        EditorUtility.DisplayDialog("Complete", $"Added {addedCount} MeshColliders to LOD children.\nOnly LOD0 colliders are now enabled.", "OK");
    }
}
