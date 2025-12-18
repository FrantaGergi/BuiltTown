using System.Collections.Generic;
using UnityEngine;

public class MinimapIconManager : MonoBehaviour
{

    public static MinimapIconManager Instance { get; private set; }
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

    // ty nejsou ve stejne funcionalite jako ostatni ikony
    public GameObject districts;
    private List<LineRenderer> districtLines = new List<LineRenderer>();

    public bool DistrictsVisible => districtsVisible;

    private bool districtsVisible = true;

    public enum MinimapIconType
    {
        Miner,
        Builder,
        Collector,
        Stone,
        Ore,
        Wood,
        District,
        None
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

        if (minimapCamera == null)
            Debug.LogWarning("MinimapIconManager: minimapCamera není pøiøazen. Ikony nebudou správnì poèítány.");

        worldSizeHalf = minimapCamera != null ? minimapCamera.orthographicSize : 1f;

        // inicializace skupin
        foreach (MinimapIconType type in System.Enum.GetValues(typeof(MinimapIconType)))
        {
            iconGroups[type] = new List<MinimapIcon>();
        }

        // districtLines bude doplnìn pozdìji, pokud districts není nastavené nyní
        if (districts != null)
            districtLines.AddRange(districts.GetComponentsInChildren<LineRenderer>());
    }

    private void Start()
    {
        SetAllGroups(true);
    }

    void Update()
    {
        UpdateIcons();
    }

    // === Registrace ikony ===
    public void RegisterIcon(MinimapIcon icon)
    {
        if (icon == null) return;

        Debug.Log($"MinimapIconManager: registering icon {icon.name} type {icon.iconType}");

        if (icon.iconType == MinimapIconType.District)
        {
            // districts mùže být nastaveno jako objekt s MinimapIcon (scénní objekt)
            if (districts == null)
            {
                districts = icon.gameObject;
                districtLines.Clear();
                districtLines.AddRange(districts.GetComponentsInChildren<LineRenderer>());
            }
            SetDistrict(true);
            return;
        }
        else if (icon.iconType == MinimapIconType.None)
        {
            Debug.LogError("Cannot register icon of type None.");
            return;
        }

        if (!iconGroups.ContainsKey(icon.iconType))
            iconGroups[icon.iconType] = new List<MinimapIcon>();

        iconGroups[icon.iconType].Add(icon);
    }

    // === Odstranìní ikony ===
    public void UnregisterIcon(MinimapIcon icon)
    {
        if (icon == null) return;

        if (iconGroups.ContainsKey(icon.iconType))
            iconGroups[icon.iconType].Remove(icon);
    }

    // === Pøepoèet pozic ===
    private void UpdateIcons()
    {
        if (minimapCamera == null || minimapRect == null) return;

        float uiWidth = minimapRect.rect.width;
        float uiHeight = minimapRect.rect.height;

        foreach (var group in iconGroups)
        {
            foreach (var icon in group.Value)
            {
                if (icon == null) continue;
                if (icon.uiIcon == null) continue;

                Vector3 worldPos = icon.transform.position;
                Vector3 camPos = minimapCamera.transform.position;

                Vector3 relative = worldPos - camPos;

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
        if (!iconGroups.ContainsKey(type)) return;

        foreach (var icon in iconGroups[type])
        {
            if (icon != null && icon.uiIcon != null)
                icon.uiIcon.gameObject.SetActive(visible);
        }
        if (type == MinimapIconType.District)
            SetDistrict(visible);

        Debug.Log($"SetGroupVisible: {type} to {visible}");
    }

    public void ShowOnly(MinimapIconType type)
    {
        foreach (var group in iconGroups)
        {
            bool show = (group.Key == type);

            foreach (var icon in group.Value)
                if (icon != null && icon.uiIcon != null)
                    icon.uiIcon.gameObject.SetActive(show);
        }
        SetDistrict(type == MinimapIconType.District);
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
            case MinimapIconType.District: prefab = woodIconPrefab; break; // pokud nemáš district prefab, uprav zde
            default: prefab = null; break;
        }

        if (prefab == null)
        {
            Debug.LogError($"MinimapIconManager: prefab for {type} is not assigned.");
            return null;
        }

        // parent fallback: iconContainer (preferováno) -> minimapRect -> žádný (root)
        Transform parent = iconContainer != null ? iconContainer : (Transform)minimapRect;

        var inst = Instantiate(prefab, parent, false);
        if (inst == null)
        {
            Debug.LogError("MinimapIconManager: Instantiate returned null.");
            return null;
        }

        // Zajistíme korektní transformaci UI prvku
        inst.localScale = Vector3.one;
        inst.anchoredPosition = Vector2.zero;
        inst.gameObject.SetActive(true);

        return inst;
    }

    public void SetAllGroups(bool show)
    {
        foreach (var group in iconGroups)
        {
            foreach (var icon in group.Value)
            {
                if (icon != null && icon.uiIcon != null)
                    icon.uiIcon.gameObject.SetActive(show);
            }
        }
        SetDistrict(show);
    }


    private void SetDistrict(bool show)
    {
        if (districts != null)
        {
            districtsVisible = show;
            districtLines.ForEach(d =>
            {
               d.enabled = show;
            });
        }
    }

}
