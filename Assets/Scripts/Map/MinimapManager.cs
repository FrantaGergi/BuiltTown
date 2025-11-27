using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    [Header("References")]
    public Camera minimapCamera;
    public RectTransform minimapRect;

    // Dictionary: typ -> list ikon
    private Dictionary<MinimapIconType, List<MinimapIcon>> iconGroups =
        new Dictionary<MinimapIconType, List<MinimapIcon>>();



    public enum MinimapIconType
    {
        Miner,
        Builder,
        Collector,
        Stone,
        Ore,
        Wood,
        Building
    }

    private float worldSizeHalf;

    void Start()
    {
        worldSizeHalf = minimapCamera.orthographicSize;

        // Inicializuj dictionary pro všechny typy
        foreach (MinimapIconType type in System.Enum.GetValues(typeof(MinimapIconType)))
        {
            iconGroups[type] = new List<MinimapIcon>();
        }
    }

    void Update()
    {
        UpdateIcons();
    }

    // === Registrace ikony ===
    public void RegisterIcon(MinimapIcon icon)
    {
        iconGroups[icon.iconType].Add(icon);
    }

    // === Odstranìní ikony ===
    public void UnregisterIcon(MinimapIcon icon)
    {
        iconGroups[icon.iconType].Remove(icon);
    }

    // === Pøepoèet pozic všech ikon ===
    private void UpdateIcons()
    {
        float uiWidth = minimapRect.rect.width;
        float uiHeight = minimapRect.rect.height;

        foreach (var group in iconGroups)
        {
            foreach (var icon in group.Value)
            {
                if (icon == null) continue;

                Vector3 relative = icon.transform.position - minimapCamera.transform.position;

                float normX = relative.x / worldSizeHalf;
                float normY = relative.z / worldSizeHalf;

                float uiX = normX * (uiWidth / 2f);
                float uiY = normY * (uiHeight / 2f);

                icon.uiIcon.anchoredPosition = new Vector2(uiX, uiY);
            }
        }
    }

    // === FILTRACE / ZAPÍNÁNÍ / VYPÍNÁNÍ ===
    public void SetGroupVisible(MinimapIconType type, bool visible)
    {
        foreach (var icon in iconGroups[type])
        {
            if (icon != null)
                icon.uiIcon.gameObject.SetActive(visible);
        }
    }

    // Vypnout vše kromì...
    public void ShowOnly(MinimapIconType type)
    {
        foreach (var group in iconGroups)
        {
            bool show = (group.Key == type);

            foreach (var icon in group.Value)
                icon.uiIcon.gameObject.SetActive(show);
        }
    }
}
