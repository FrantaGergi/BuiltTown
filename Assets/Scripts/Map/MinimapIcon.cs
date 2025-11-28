using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public MinimapManager.MinimapIconType iconType;

    [HideInInspector]
    public RectTransform uiIcon;

    void Start()
    {
        // spawn UI ikonky
        uiIcon = MinimapManager.Instance.SpawnIcon(iconType);

        // registrace
        MinimapManager.Instance.RegisterIcon(this);
    }

    void OnDestroy()
    {
        if (MinimapManager.Instance != null)
        {
            MinimapManager.Instance.UnregisterIcon(this);

            if (uiIcon != null)
                Destroy(uiIcon.gameObject);
        }
    }
}
