using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{

    public static MinimapManager Instance { get; private set; }
    [Header("Icon Prefabs")]
    public RectTransform minerIconPrefab;
    public RectTransform builderIconPrefab;
    public RectTransform collectorIconPrefab;
    public RectTransform stoneIconPrefab;
    public RectTransform oreIconPrefab;
    public RectTransform woodIconPrefab;


    [Header("References")]
    public Camera minimapCamera;
    public RectTransform minimapRect;
    public Transform iconContainer; // parent v canvasu


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
        District
    }

    private float worldSizeHalf;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Pokud chceš, aby pøetrvával mezi scénami:
        // DontDestroyOnLoad(gameObject);

        worldSizeHalf = minimapCamera.orthographicSize;

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

    // === Pøepoèet pozic ===
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

    // === FILTRACE ===
    public void SetGroupVisible(MinimapIconType type, bool visible)
    {
        foreach (var icon in iconGroups[type])
        {
            if (icon != null)
                icon.uiIcon.gameObject.SetActive(visible);
        }
    }

    public void ShowOnly(MinimapIconType type)
    {
        foreach (var group in iconGroups)
        {
            bool show = (group.Key == type);

            foreach (var icon in group.Value)
                icon.uiIcon.gameObject.SetActive(show);
        }
    }


    public RectTransform SpawnIcon(MinimapIconType type)
    {
        RectTransform prefab = null;

        switch (type)
        {
            case MinimapIconType.Miner: prefab = minerIconPrefab; break;
            case MinimapIconType.Builder: prefab = builderIconPrefab; break;
            case MinimapIconType.Collector: prefab = collectorIconPrefab; break;
            case MinimapIconType.Stone: prefab = stoneIconPrefab; break;
            case MinimapIconType.Ore: prefab = oreIconPrefab; break;
            case MinimapIconType.Wood: prefab = woodIconPrefab; break;
            case MinimapIconType.District: prefab = woodIconPrefab; break;
        }

        RectTransform icon = Instantiate(prefab, iconContainer);
        return icon;
    }

}
