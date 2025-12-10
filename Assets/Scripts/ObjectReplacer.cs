using UnityEngine;
using UnityEditor;

public class ObjectReplacer : EditorWindow
{
    public GameObject prefabToSpawn;

    [MenuItem("Tools/Object Replacer")]
    public static void ShowWindow()
    {
        GetWindow<ObjectReplacer>("Object Replacer");
    }

    void OnGUI()
    {
        prefabToSpawn = (GameObject)EditorGUILayout.ObjectField("New Prefab", prefabToSpawn, typeof(GameObject), false);

        if (GUILayout.Button("Replace Selected"))
        {
            Replace();
        }
    }

    void Replace()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            // ulož transform pùvodního objektu
            Vector3 pos = obj.transform.position;
            Quaternion rot = obj.transform.rotation;
            Vector3 scale = obj.transform.localScale;
            Transform parent = obj.transform.parent;

            // vytvoø nový objekt z prefabu
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn, parent);

            // nastav transform PO vytvoøení
            newObj.transform.position = pos;
            newObj.transform.rotation = rot;
            newObj.transform.localScale = scale;

            // smaž pùvodní objekt
            DestroyImmediate(obj);
        }
    }
}
