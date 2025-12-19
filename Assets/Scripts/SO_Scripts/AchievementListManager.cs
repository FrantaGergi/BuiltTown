using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AchievementListManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform flowPanel; // Flow/Content panel (Layout Group)
    [SerializeField] private GameObject entryPrefab;   // prefab obsahující AchievementListEntry
    [SerializeField] private AchievementIconDatabase iconDatabase;
    [SerializeField] private TextMeshProUGUI titleText;

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    private void Start()
    {
        if (flowPanel == null) Debug.LogWarning("AchievementListManager: flowPanel není pøiøazen.");
        if (entryPrefab == null) Debug.LogWarning("AchievementListManager: entryPrefab není pøiøazen.");
        RefreshList();
        AchievementsStatus();
    }

    private void OnEnable()
    {
        AchievementService.OnAchievementUnlocked += OnAchievementUnlocked;
    }

    private void OnDisable()
    {
        AchievementService.OnAchievementUnlocked -= OnAchievementUnlocked;
    }

    private void OnAchievementUnlocked(AchievementService.Achievement ach)
    {
        // Pøidáme novì odemèený achievement (mùžeme rovnou pøidat jeden záznam)
        AddEntry(ach);
        AchievementsStatus();
    }

    private void AchievementsStatus()
    {
        titleText.text = $"Achievements ({AchievementService.GetUnlockedCount()} / 18)";
    }

    // Vyèistí a naplní podle aktuálních odemèených achievementù
    [ContextMenu("Refresh Achievement List")]
    public void RefreshList()
    {
        ClearEntries();

        if (flowPanel == null || entryPrefab == null) return;

        foreach (var a in AchievementService.achievements)
        {
            if (a.unlocked)
                AddEntry(a);
        }
    }

    private void AddEntry(AchievementService.Achievement a)
    {
        if (flowPanel == null || entryPrefab == null) return;

        var go = Instantiate(entryPrefab, flowPanel, false);
        go.name = $"{a.id}_Entry";
        spawnedEntries.Add(go);

        var entry = go.GetComponent<AchievementListEntry>();
        if (entry != null)
            entry.Setup(a, iconDatabase);
        else
            Debug.LogWarning("AchievementListManager: prefab nemá komponentu AchievementListEntry.");
    }

    private void ClearEntries()
    {
        for (int i = spawnedEntries.Count - 1; i >= 0; i--)
        {
            var go = spawnedEntries[i];
            if (go != null) Destroy(go);
        }
        spawnedEntries.Clear();
    }

    // Utility pro test v editoru
#if UNITY_EDITOR
    [ContextMenu("Clear And Rebuild")]
    private void EditorRebuild() => RefreshList();
#endif
}