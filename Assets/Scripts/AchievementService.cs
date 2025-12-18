// AchievementService.cs - Hlavní správce achievementů

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AchievementService
{
    [Serializable]
    public class Stats
    {
        public int treesCut;
        public int stoneMined;
        public int oreMined;
        public int districtsPurchased;
        public int buildingsBuilt;
        public int npcsHired;
    }

    [Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        public string description;
        public string stat;
        public int target;
        public bool unlocked;
        public string icon;
    }

    [Serializable]
    class SaveData
    {
        public Stats stats;
        public List<string> unlocked;
    }

    private const string SaveKey = "AchievementProgress_v1";

    public static event Action<Achievement> OnAchievementUnlocked;

    public static Stats stats { get; private set; } = new Stats();

    private static List<Achievement> _achievements;
    public static IReadOnlyList<Achievement> achievements => _achievements;

    static AchievementService()
    {
        InitData();
        LoadProgress();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInitialized() { /* triggers static ctor */ }

    private static void InitData()
    {
        _achievements = new List<Achievement>()
        {
            new Achievement{ id="tree_1", name="Lesník začátečník", description="1 strom pokácen", stat=nameof(stats.treesCut), target=1, unlocked=false, icon="🌲"},
            new Achievement{ id="tree_10", name="Dřevorubec", description="10 stromů pokáceno", stat=nameof(stats.treesCut), target=10, unlocked=false, icon="🪓"},
            new Achievement{ id="tree_100", name="Král lesa", description="100 stromů pokáceno", stat=nameof(stats.treesCut), target=100, unlocked=false, icon="👑"},

            new Achievement{ id="stone_1", name="První kámen", description="1 kámen vytěžen", stat=nameof(stats.stoneMined), target=1, unlocked=false, icon="🪨"},
            new Achievement{ id="stone_10", name="Kameník", description="10 kamenů vytěženo", stat=nameof(stats.stoneMined), target=10, unlocked=false, icon="⛏️"},
            new Achievement{ id="stone_100", name="Mistr kamene", description="100 kamenů vytěženo", stat=nameof(stats.stoneMined), target=100, unlocked=false, icon="🏔️"},

            new Achievement{ id="ore_1", name="Zlatokop", description="1 ruda vytěžena", stat=nameof(stats.oreMined), target=1, unlocked=false, icon="⚒️"},
            new Achievement{ id="ore_10", name="Horník", description="10 rud vytěženo", stat=nameof(stats.oreMined), target=10, unlocked=false, icon="💎"},
            new Achievement{ id="ore_100", name="Důlní magnát", description="100 rud vytěženo", stat=nameof(stats.oreMined), target=100, unlocked=false, icon="👷"},

            new Achievement{ id="district_1", name="První expanze", description="1 distrikt zakoupen", stat=nameof(stats.districtsPurchased), target=1, unlocked=false, icon="🗺️"},
            new Achievement{ id="district_5", name="Stavitel měst", description="5 distriktů zakoupeno", stat=nameof(stats.districtsPurchased), target=5, unlocked=false, icon="🏙️"},
            new Achievement{ id="district_20", name="Císař území", description="20 distriktů zakoupeno", stat=nameof(stats.districtsPurchased), target=20, unlocked=false, icon="🌍"},

            new Achievement{ id="building_1", name="První dům", description="1 budova postavena", stat=nameof(stats.buildingsBuilt), target=1, unlocked=false, icon="🏠"},
            new Achievement{ id="building_5", name="Architekt", description="5 budov postaveno", stat=nameof(stats.buildingsBuilt), target=5, unlocked=false, icon="🏗️"},
            new Achievement{ id="building_20", name="Velkostavitel", description="20 budov postaveno", stat=nameof(stats.buildingsBuilt), target=20, unlocked=false, icon="🏛️"},

            new Achievement{ id="npc_1", name="První zaměstnanec", description="1 NPC najato", stat=nameof(stats.npcsHired), target=1, unlocked=false, icon="👤"},
            new Achievement{ id="npc_5", name="Manažer týmu", description="5 NPC najato", stat=nameof(stats.npcsHired), target=5, unlocked=false, icon="👥"},
            new Achievement{ id="npc_15", name="Vedoucí gildy", description="15 NPC najato", stat=nameof(stats.npcsHired), target=15, unlocked=false, icon="👨‍👩‍👧‍👦"}
        };
    }

    // --- Public API (staticke volani z hernich skriptu) ---
    public static void OnTreeCut() { stats.treesCut++; CheckAchievements(nameof(stats.treesCut)); }
    public static void OnStoneMined() { stats.stoneMined++; CheckAchievements(nameof(stats.stoneMined)); }
    public static void OnOreMined() { stats.oreMined++; CheckAchievements(nameof(stats.oreMined)); }
    public static void OnDistrictPurchased() { stats.districtsPurchased++; CheckAchievements(nameof(stats.districtsPurchased)); }
    public static void OnBuildingBuilt() { stats.buildingsBuilt++; CheckAchievements(nameof(stats.buildingsBuilt)); }
    public static void OnNPCHired() { stats.npcsHired++; CheckAchievements(nameof(stats.npcsHired)); }

    // Generic adder (useful from other code)
    public static void AddToStat(string statName, int amount = 1)
    {
        switch (statName)
        {
            case nameof(stats.treesCut): stats.treesCut += amount; break;
            case nameof(stats.stoneMined): stats.stoneMined += amount; break;
            case nameof(stats.oreMined): stats.oreMined += amount; break;
            case nameof(stats.districtsPurchased): stats.districtsPurchased += amount; break;
            case nameof(stats.buildingsBuilt): stats.buildingsBuilt += amount; break;
            case nameof(stats.npcsHired): stats.npcsHired += amount; break;
            default: return;
        }
        CheckAchievements(statName);
    }

    private static void CheckAchievements(string statPropertyName)
    {
        int currentValue = GetStatValueByName(statPropertyName);

        foreach (var achievement in _achievements)
        {
            if (!achievement.unlocked && achievement.stat == statPropertyName && currentValue >= achievement.target)
            {
                achievement.unlocked = true;
                UnlockAchievement(achievement);
            }
        }
    }

    private static int GetStatValueByName(string statName)
    {
        return statName switch
        {
            nameof(stats.treesCut) => stats.treesCut,
            nameof(stats.stoneMined) => stats.stoneMined,
            nameof(stats.oreMined) => stats.oreMined,
            nameof(stats.districtsPurchased) => stats.districtsPurchased,
            nameof(stats.buildingsBuilt) => stats.buildingsBuilt,
            nameof(stats.npcsHired) => stats.npcsHired,
            _ => 0
        };
    }

    private static void UnlockAchievement(Achievement achievement)
    {
        Debug.Log($"Achievement odemčen: {achievement.name}");
        OnAchievementUnlocked?.Invoke(achievement);
        SaveProgress();
    }

    // --- Dotazovací metody ---
    public static IEnumerable<(Achievement achievement, int progress, float percentage)> GetAllAchievements()
    {
        foreach (var a in _achievements)
        {
            int p = GetStatValueByName(a.stat);
            float perc = a.target > 0 ? Mathf.Min((p / (float)a.target) * 100f, 100f) : 0f;
            yield return (a, p, perc);
        }
    }

    public static (int unlocked, int total, float percentage, Stats stats) GetProgress()
    {
        int total = _achievements.Count;
        int unlocked = _achievements.Count(a => a.unlocked);
        float percent = total > 0 ? (unlocked / (float)total) * 100f : 0f;
        return (unlocked, total, percent, stats);
    }

    // --- Save / Load ---
    public static void SaveProgress()
    {
        var data = new SaveData
        {
            stats = stats,
            unlocked = _achievements.Where(a => a.unlocked).Select(a => a.id).ToList()
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static void LoadProgress()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string json = PlayerPrefs.GetString(SaveKey);
        try
        {
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;
            if (data.stats != null) stats = data.stats;
            if (data.unlocked != null)
            {
                foreach (var id in data.unlocked)
                {
                    var ach = _achievements.Find(a => a.id == id);
                    if (ach != null) ach.unlocked = true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Neplatný save JSON: {ex.Message}");
        }
    }

    public static void ResetProgress()
    {
        stats = new Stats();
        foreach (var a in _achievements) a.unlocked = false;
        PlayerPrefs.DeleteKey(SaveKey);
    }
}