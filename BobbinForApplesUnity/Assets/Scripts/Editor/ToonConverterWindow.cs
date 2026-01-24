using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class ToonConverterWindow : EditorWindow
{
    private GameObject targetObject;
    private GameObject targetPrefab;
    private ToonConverterData converterData;
    private Vector2 scrollPosition;
    private Shader toonShader;
    
    private Color headerColor = new Color(0.2f, 0.6f, 0.9f);
    private Color buttonConvertColor = new Color(0.4f, 0.8f, 0.4f);
    private Color buttonRevertColor = new Color(0.9f, 0.5f, 0.3f);
    
    [MenuItem("Tools/Daniel's Toon Converter")]
    public static void ShowWindow()
    {
        ToonConverterWindow window = GetWindow<ToonConverterWindow>("Toon Converter");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnEnable()
    {
        converterData = ToonConverterData.GetOrCreate();
        toonShader = converterData.toonShader;
    }
    
    private void OnGUI()
    {
        DrawHeader();
        
        EditorGUILayout.Space(10);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawShaderSelection();
        EditorGUILayout.Space(15);
        
        DrawPrefabConverter();
        EditorGUILayout.Space(15);
        
        DrawConvertedPrefabsList();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        
        Rect headerRect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(headerRect, headerColor);
        
        GUI.Label(headerRect, "Daniel's Toon Converter", headerStyle);
        
        GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10,
            normal = { textColor = new Color(1f, 1f, 1f, 0.7f) }
        };
        
        Rect subtitleRect = new Rect(headerRect.x, headerRect.y + 35, headerRect.width, 20);
        GUI.Label(subtitleRect, "Convert Prefabs to Toon Shaders", subtitleStyle);
    }
    
    private void DrawShaderSelection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Toon Shader Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUI.BeginChangeCheck();
        toonShader = (Shader)EditorGUILayout.ObjectField("Toon Shader", toonShader, typeof(Shader), false);
        
        if (EditorGUI.EndChangeCheck())
        {
            converterData.toonShader = toonShader;
            EditorUtility.SetDirty(converterData);
            AssetDatabase.SaveAssets();
        }
        
        if (toonShader == null)
        {
            EditorGUILayout.HelpBox("Please assign a toon shader (e.g., FlatKit/Stylized Surface)", MessageType.Warning);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPrefabConverter()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Prefab Conversion", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUI.BeginChangeCheck();
        targetObject = (GameObject)EditorGUILayout.ObjectField(
            "Target GameObject/Prefab", 
            targetObject, 
            typeof(GameObject), 
            true
        );
        
        if (EditorGUI.EndChangeCheck())
        {
            if (targetObject != null)
            {
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(targetObject);
                
                if (prefabAsset != null)
                {
                    targetPrefab = prefabAsset;
                }
                else if (PrefabUtility.IsPartOfPrefabAsset(targetObject))
                {
                    targetPrefab = targetObject;
                }
                else
                {
                    targetPrefab = null;
                }
            }
            else
            {
                targetPrefab = null;
            }
        }
        
        if (targetObject != null && targetPrefab == null)
        {
            EditorGUILayout.HelpBox("This GameObject is not connected to a prefab. Please use a prefab or a prefab instance from the scene.", MessageType.Warning);
        }
        
        if (targetPrefab != null && targetPrefab != targetObject)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Prefab Source:", EditorStyles.miniLabel, GUILayout.Width(85));
            EditorGUILayout.ObjectField(targetPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(10);
        
        GUI.enabled = targetPrefab != null && toonShader != null;
        
        ToonMaterialMapping existingMapping = targetPrefab != null ? converterData.FindMapping(targetPrefab) : null;
        bool isConverted = existingMapping != null && existingMapping.isConverted;
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = buttonConvertColor;
        if (GUILayout.Button(isConverted ? "Re-Convert to Toon" : "Convert to Toon", GUILayout.Height(35)))
        {
            ConvertPrefabToToon(targetPrefab);
        }
        
        GUI.enabled = isConverted;
        GUI.backgroundColor = buttonRevertColor;
        if (GUILayout.Button("Revert to Original", GUILayout.Height(35)))
        {
            RevertPrefabToOriginal(targetPrefab);
        }
        
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        if (targetPrefab != null && toonShader == null)
        {
            EditorGUILayout.HelpBox("Please assign a toon shader first", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawConvertedPrefabsList()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Converted Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        if (converterData.mappings.Count == 0)
        {
            EditorGUILayout.HelpBox("No prefabs converted yet", MessageType.Info);
        }
        else
        {
            for (int i = converterData.mappings.Count - 1; i >= 0; i--)
            {
                ToonMaterialMapping mapping = converterData.mappings[i];
                
                if (mapping.prefab == null)
                {
                    converterData.mappings.RemoveAt(i);
                    continue;
                }
                
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                EditorGUILayout.ObjectField(mapping.prefab, typeof(GameObject), false, GUILayout.Width(200));
                
                string status = mapping.isConverted ? "✓ Toon" : "Original";
                Color statusColor = mapping.isConverted ? Color.green : Color.gray;
                
                GUIStyle statusStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = statusColor },
                    fontStyle = FontStyle.Bold
                };
                
                EditorGUILayout.LabelField(status, statusStyle, GUILayout.Width(80));
                
                GUILayout.FlexibleSpace();
                
                if (mapping.isConverted)
                {
                    GUI.backgroundColor = buttonRevertColor;
                    if (GUILayout.Button("Revert", GUILayout.Width(80)))
                    {
                        RevertPrefabToOriginal(mapping.prefab);
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUI.backgroundColor = buttonConvertColor;
                    if (GUILayout.Button("Convert", GUILayout.Width(80)))
                    {
                        ConvertPrefabToToon(mapping.prefab);
                    }
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void ConvertPrefabToToon(GameObject prefab)
    {
        if (prefab == null || toonShader == null)
        {
            Debug.LogError("Prefab or Toon Shader is null");
            return;
        }
        
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
        
        ToonMaterialMapping mapping = converterData.FindMapping(prefab);
        
        if (mapping == null)
        {
            mapping = new ToonMaterialMapping { prefab = prefab };
        }
        
        mapping.originalMaterials.Clear();
        mapping.toonMaterials.Clear();
        
        Renderer[] renderers = prefabInstance.GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer renderer in renderers)
        {
            string path = GetGameObjectPath(renderer.transform, prefabInstance.transform);
            Material[] originalMaterials = renderer.sharedMaterials;
            
            mapping.originalMaterials.Add(new ToonMaterialMapping.MaterialBackup(renderer, originalMaterials, path));
            
            Material[] newToonMaterials = new Material[originalMaterials.Length];
            
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i] != null)
                {
                    Material originalMat = originalMaterials[i];
                    Material toonMat = new Material(toonShader)
                    {
                        name = originalMat.name + "_Toon"
                    };
                    
                    Debug.Log($"Converting material: {originalMat.name}");
                    Debug.Log($"Original shader: {originalMat.shader.name}");
                    Debug.Log($"Target shader: {toonShader.name}");
                    
                    CopyMaterialProperties(originalMat, toonMat);
                    
                    newToonMaterials[i] = toonMat;
                    
                    string materialPath = System.IO.Path.GetDirectoryName(prefabPath) + "/" + toonMat.name + ".mat";
                    materialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath);
                    
                    AssetDatabase.CreateAsset(toonMat, materialPath);
                }
            }
            
            renderer.sharedMaterials = newToonMaterials;
            mapping.toonMaterials.Add(new ToonMaterialMapping.MaterialBackup(renderer, newToonMaterials, path));
        }
        
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabInstance);
        
        mapping.isConverted = true;
        converterData.AddOrUpdateMapping(mapping);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Converted {prefab.name} to toon shader");
    }
    
    private void CopyMaterialProperties(Material source, Material target)
    {
        Texture mainTexture = null;
        Color mainColor = Color.white;
        
        string[] texturePropertyNames = { "_BaseColorMap", "_BaseMap", "_MainTex", "_ColorMap", "_Albedo", "_DiffuseTex" };
        foreach (string propName in texturePropertyNames)
        {
            if (source.HasProperty(propName))
            {
                Texture tex = source.GetTexture(propName);
                if (tex != null)
                {
                    mainTexture = tex;
                    Debug.Log($"Found texture in source property: {propName} = {tex.name}");
                    break;
                }
            }
        }
        
        string[] colorPropertyNames = { "_BaseColor", "_Color", "_MainColor", "_Albedo" };
        foreach (string propName in colorPropertyNames)
        {
            if (source.HasProperty(propName))
            {
                Color col = source.GetColor(propName);
                mainColor = col;
                Debug.Log($"Found color in source property: {propName} = {col}");
                break;
            }
        }
        
        if (mainTexture != null)
        {
            bool textureSet = false;
            foreach (string propName in texturePropertyNames)
            {
                if (target.HasProperty(propName))
                {
                    target.SetTexture(propName, mainTexture);
                    Debug.Log($"Set texture to target property: {propName}");
                    textureSet = true;
                    
                    string sourceSTName = null;
                    foreach (string stProp in texturePropertyNames)
                    {
                        if (source.HasProperty(stProp + "_ST"))
                        {
                            sourceSTName = stProp + "_ST";
                            break;
                        }
                    }
                    
                    if (sourceSTName != null)
                    {
                        Vector4 scaleOffset = source.GetVector(sourceSTName);
                        if (target.HasProperty(propName + "_ST"))
                        {
                            target.SetVector(propName + "_ST", scaleOffset);
                            Debug.Log($"Copied texture tiling/offset: {scaleOffset}");
                        }
                    }
                    break;
                }
            }
            
            if (!textureSet)
            {
                Debug.LogWarning($"Could not find matching texture property in target shader for texture: {mainTexture.name}");
            }
        }
        else
        {
            Debug.LogWarning("No texture found in source material!");
        }
        
        bool colorSet = false;
        foreach (string propName in colorPropertyNames)
        {
            if (target.HasProperty(propName))
            {
                target.SetColor(propName, mainColor);
                Debug.Log($"Set color to target property: {propName}");
                colorSet = true;
                break;
            }
        }
        
        if (!colorSet)
        {
            Debug.LogWarning("Could not find matching color property in target shader");
        }
        
        if (source.HasProperty("_BumpMap") && target.HasProperty("_BumpMap"))
        {
            Texture normalMap = source.GetTexture("_BumpMap");
            if (normalMap != null)
            {
                target.SetTexture("_BumpMap", normalMap);
                Debug.Log($"Copied normal map: {normalMap.name}");
            }
        }
        
        if (source.HasProperty("_BaseNormalMap") && target.HasProperty("_BumpMap"))
        {
            Texture normalMap = source.GetTexture("_BaseNormalMap");
            if (normalMap != null)
            {
                target.SetTexture("_BumpMap", normalMap);
                Debug.Log($"Copied normal map from _BaseNormalMap: {normalMap.name}");
            }
        }
        
        if (source.HasProperty("_MetallicGlossMap") && target.HasProperty("_MetallicGlossMap"))
        {
            target.SetTexture("_MetallicGlossMap", source.GetTexture("_MetallicGlossMap"));
        }
        
        if (source.HasProperty("_Metallic") && target.HasProperty("_Metallic"))
        {
            target.SetFloat("_Metallic", source.GetFloat("_Metallic"));
        }
        
        if (source.HasProperty("_Smoothness") && target.HasProperty("_Smoothness"))
        {
            target.SetFloat("_Smoothness", source.GetFloat("_Smoothness"));
        }
        
        if (source.HasProperty("_Glossiness") && target.HasProperty("_Glossiness"))
        {
            target.SetFloat("_Glossiness", source.GetFloat("_Glossiness"));
        }
    }
    
    private void RevertPrefabToOriginal(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null");
            return;
        }
        
        ToonMaterialMapping mapping = converterData.FindMapping(prefab);
        
        if (mapping == null || mapping.originalMaterials.Count == 0)
        {
            Debug.LogWarning($"No backup found for {prefab.name}");
            return;
        }
        
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
        
        foreach (ToonMaterialMapping.MaterialBackup backup in mapping.originalMaterials)
        {
            Transform rendererTransform = FindChildByPath(prefabInstance.transform, backup.rendererPath);
            
            if (rendererTransform != null)
            {
                Renderer renderer = rendererTransform.GetComponent<Renderer>();
                
                if (renderer != null)
                {
                    renderer.sharedMaterials = backup.materials;
                }
            }
        }
        
        foreach (ToonMaterialMapping.MaterialBackup toonBackup in mapping.toonMaterials)
        {
            foreach (Material mat in toonBackup.materials)
            {
                if (mat != null)
                {
                    string matPath = AssetDatabase.GetAssetPath(mat);
                    if (!string.IsNullOrEmpty(matPath))
                    {
                        AssetDatabase.DeleteAsset(matPath);
                    }
                }
            }
        }
        
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabInstance);
        
        mapping.isConverted = false;
        converterData.AddOrUpdateMapping(mapping);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Reverted {prefab.name} to original materials");
    }
    
    private string GetGameObjectPath(Transform transform, Transform root)
    {
        if (transform == root)
            return "";
        
        string path = transform.name;
        Transform current = transform.parent;
        
        while (current != null && current != root)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
    
    private Transform FindChildByPath(Transform root, string path)
    {
        if (string.IsNullOrEmpty(path))
            return root;
        
        string[] parts = path.Split('/');
        Transform current = root;
        
        foreach (string part in parts)
        {
            bool found = false;
            foreach (Transform child in current)
            {
                if (child.name == part)
                {
                    current = child;
                    found = true;
                    break;
                }
            }
            
            if (!found)
                return null;
        }
        
        return current;
    }
}
