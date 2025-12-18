#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AchievementServiceWindow : EditorWindow
{
    [MenuItem("Window/Achievements")]
    public static void ShowWindow()
    {
        var w = GetWindow<AchievementServiceWindow>("Achievements");
        w.minSize = new Vector2(420, 300);
    }

    Vector2 _scroll;

    void OnGUI()
    {
        if (Application.isPlaying == false)
        {
            EditorGUILayout.HelpBox("AchievementService bÏûÌ i v editoru, ale nÏkterÈ runtime ud·losti se nezobrazÌ. Spusù hru pro plnÈ testov·nÌ.", MessageType.Info);
        }

        var prog = AchievementService.GetProgress();
        EditorGUILayout.LabelField("Progress", $"{prog.unlocked}/{prog.total} ({prog.percentage:F1}%)");
        EditorGUILayout.Space(6);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save")) AchievementService.SaveProgress();
        if (GUILayout.Button("Load")) AchievementService.LoadProgress();
        if (GUILayout.Button("Reset")) AchievementService.ResetProgress();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var item in AchievementService.GetAllAchievements())
        {
            var a = item.achievement;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{a.icon} {a.name}", EditorStyles.boldLabel, GUILayout.Width(260));
            EditorGUILayout.LabelField(a.unlocked ? "UNLOCKED" : "locked", a.unlocked ? EditorStyles.boldLabel : EditorStyles.miniLabel, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            float pct = item.percentage / 100f;
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 14), pct, $"{item.progress}/{a.target}");
            EditorGUILayout.LabelField(a.description);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Testing helpers", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Tree")) { AchievementService.OnTreeCut(); Repaint(); }
        if (GUILayout.Button("Add Stone")) { AchievementService.OnStoneMined(); Repaint(); }
        if (GUILayout.Button("Add Ore")) { AchievementService.OnOreMined(); Repaint(); }
        if (GUILayout.Button("Hire NPC")) { AchievementService.OnNPCHired(); Repaint(); }
        EditorGUILayout.EndHorizontal();
    }
}
#endif