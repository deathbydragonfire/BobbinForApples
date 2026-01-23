using UnityEngine;

public class AreaMaterialManager : MonoBehaviour
{
    [Header("Material Settings")]
    public Material materialToApply;
    
    [Header("Filter Options")]
    public bool onlyCubeMeshes = true;
    public bool includeInactive = false;

    public void ApplyMaterialToAllCubes()
    {
        if (materialToApply == null)
        {
            Debug.LogWarning("No material assigned to apply!");
            return;
        }

        int count = 0;
        MeshRenderer[] renderers = includeInactive 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponentsInChildren<MeshRenderer>(false);

        foreach (MeshRenderer renderer in renderers)
        {
            if (onlyCubeMeshes)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    if (meshFilter.sharedMesh.name.Contains("Cube"))
                    {
                        renderer.material = materialToApply;
                        count++;
                    }
                }
            }
            else
            {
                renderer.material = materialToApply;
                count++;
            }
        }

        Debug.Log($"Applied material '{materialToApply.name}' to {count} objects in '{gameObject.name}'");
    }

    public void ResetAllMaterials()
    {
        int count = 0;
        MeshRenderer[] renderers = includeInactive 
            ? GetComponentsInChildren<MeshRenderer>(true) 
            : GetComponentsInChildren<MeshRenderer>(false);

        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            count++;
        }

        Debug.Log($"Reset materials on {count} objects in '{gameObject.name}'");
    }
}
