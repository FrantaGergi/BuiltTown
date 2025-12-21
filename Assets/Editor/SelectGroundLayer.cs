using UnityEditor;
using UnityEngine;
using System.Linq;

public class SelectGroundLayer
{
    [MenuItem("Tools/Select Ground Layer Objects")]
    static void SelectGround()
    {
        int layer = LayerMask.NameToLayer("Ground");
        if (layer == -1)
        {
            Debug.LogError("Layer 'ground' neexistuje.");
            return;
        }

        var objs = Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None)
            .Where(go => go.layer == layer)
            .ToArray();

        Selection.objects = objs;
        Debug.Log($"Vybráno {objs.Length} objektù s layer 'ground'");
    }
}
