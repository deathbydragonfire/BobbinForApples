using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ToonMaterialMapping
{
    public GameObject prefab;
    public List<MaterialBackup> originalMaterials = new List<MaterialBackup>();
    public List<MaterialBackup> toonMaterials = new List<MaterialBackup>();
    public bool isConverted;
    
    [Serializable]
    public class MaterialBackup
    {
        public Renderer renderer;
        public Material[] materials;
        public string rendererPath;
        
        public MaterialBackup(Renderer rend, Material[] mats, string path)
        {
            renderer = rend;
            materials = mats != null ? (Material[])mats.Clone() : new Material[0];
            rendererPath = path;
        }
    }
}
