using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Achievement Icon DB")]
public class AchievementIconDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string id;
        public Sprite sprite;
    }

    public List<Entry> entries = new List<Entry>();

    // runtime cache for O(1) lookup
    private Dictionary<string, Sprite> _cache;

    private void BuildCache()
    {
        _cache = new Dictionary<string, Sprite>();
        if (entries == null) return;
        foreach (var e in entries)
        {
            if (string.IsNullOrEmpty(e.id) || e.sprite == null) continue;
            if (!_cache.ContainsKey(e.id)) _cache.Add(e.id, e.sprite);
        }
    }

    public Sprite GetSprite(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_cache == null) BuildCache();
        return _cache.TryGetValue(id, out var s) ? s : null;
    }
}