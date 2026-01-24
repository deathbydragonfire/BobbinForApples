using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ToonConverterData", menuName = "Tools/Toon Converter Data")]
public class ToonConverterData : ScriptableObject
{
    public List<ToonMaterialMapping> mappings = new List<ToonMaterialMapping>();
    public Shader toonShader;
    
    private const string DATA_PATH = "Assets/Scripts/Editor/ToonConverterData.asset";
    
    public static ToonConverterData GetOrCreate()
    {
        ToonConverterData data = AssetDatabase.LoadAssetAtPath<ToonConverterData>(DATA_PATH);
        
        if (data == null)
        {
            data = CreateInstance<ToonConverterData>();
            string directory = System.IO.Path.GetDirectoryName(DATA_PATH);
            
            if (!AssetDatabase.IsValidFolder(directory))
            {
                string[] folders = directory.Split('/');
                string currentPath = folders[0];
                
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            AssetDatabase.CreateAsset(data, DATA_PATH);
            AssetDatabase.SaveAssets();
        }
        
        return data;
    }
    
    public ToonMaterialMapping FindMapping(GameObject prefab)
    {
        return mappings.Find(m => m.prefab == prefab);
    }
    
    public void AddOrUpdateMapping(ToonMaterialMapping mapping)
    {
        ToonMaterialMapping existing = FindMapping(mapping.prefab);
        
        if (existing != null)
        {
            mappings.Remove(existing);
        }
        
        mappings.Add(mapping);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    
    public void RemoveMapping(GameObject prefab)
    {
        ToonMaterialMapping mapping = FindMapping(prefab);
        if (mapping != null)
        {
            mappings.Remove(mapping);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
