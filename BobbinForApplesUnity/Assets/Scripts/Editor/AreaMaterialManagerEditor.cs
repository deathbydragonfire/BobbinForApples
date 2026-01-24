using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaMaterialManager))]
public class AreaMaterialManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AreaMaterialManager manager = (AreaMaterialManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Material Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply Material to All Cubes", GUILayout.Height(30)))
        {
            Undo.RecordObject(manager.gameObject, "Apply Material to Area");
            manager.ApplyMaterialToAllCubes();
        }

        if (GUILayout.Button("Reset All Materials", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Reset Materials", 
                "Are you sure you want to reset all materials in this area?", 
                "Yes", "Cancel"))
            {
                Undo.RecordObject(manager.gameObject, "Reset Materials in Area");
                manager.ResetAllMaterials();
            }
        }
    }

    [MenuItem("GameObject/Area Tools/Apply Material to Area", false, 0)]
    private static void ApplyMaterialToAreaContextMenu()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select an Area GameObject first.", "OK");
            return;
        }

        AreaMaterialManager manager = selected.GetComponent<AreaMaterialManager>();
        if (manager == null)
        {
            EditorUtility.DisplayDialog("Missing Component", 
                "Selected GameObject doesn't have an AreaMaterialManager component.", "OK");
            return;
        }

        manager.ApplyMaterialToAllCubes();
    }
}
