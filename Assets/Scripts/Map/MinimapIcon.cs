using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public MinimapIconManager.MinimapIconType iconType;

    [HideInInspector]
    public RectTransform uiIcon;

    void Start()
    {
        // spawn UI ikonky
        uiIcon = MinimapIconManager.Instance.SpawnIcon(iconType);

        // registrace
        MinimapIconManager.Instance.RegisterIcon(this);
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
