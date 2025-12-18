using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public MinimapIconManager.MinimapIconType iconType;

    [HideInInspector]
    public RectTransform uiIcon;

    void Start()
    {
        uiIcon = MinimapIconManager.Instance.SpawnIcon(iconType);

        if (uiIcon != null)
        {
            // pojmenujeme instanci podle objektu, aby byla v Hierarchy snadno dohledatelná
            uiIcon.name = $"{gameObject.name}_Minimap_{iconType}";
            // zajistíme, že nebude skrytá v Hierarchy
            uiIcon.gameObject.hideFlags = HideFlags.None;
        }

        MinimapIconManager.Instance?.RegisterIcon(this);
    }

    void OnDestroy()
    {
        if (MinimapIconManager.Instance != null)
        {
            MinimapIconManager.Instance.UnregisterIcon(this);

            if (uiIcon != null)
                Destroy(uiIcon.gameObject);
        }
    }
}
