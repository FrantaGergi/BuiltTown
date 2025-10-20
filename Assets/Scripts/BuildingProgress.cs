using UnityEngine;

/// <summary>
/// Øídí vizuální prùbìh stavby a zobrazení stavu materiálù.
/// </summary>
public class BuildingProgress : MonoBehaviour
{
    [Header("Prefaby stavby")]
    public GameObject prefabStage1; // 1: èistá parcela
    public GameObject prefabStage2; // 2: bednìní (kámen)
    public GameObject prefabStage3; // 3: kostra (døevo)
    public GameObject prefabStage4; // 4: hotová stavba (ruda)

    [Header("Materiály")]
    public BuildingMaterial stone;  // pro bednìní
    public BuildingMaterial wood;   // pro kostru
    public BuildingMaterial ore;    // pro finální stavbu


    /// <summary>
    /// Zavolej po každé zmìnì materiálù.
    /// Pøepne správný prefab a aktualizuje stav všech vizuálních indikátorù.
    /// </summary>
    public void UpdateVisuals()
    {
        if (prefabStage1 == null)
        {
            return;
        }
        //Vypni všechny fáze, aby se zobrazila jen ta aktuální
        prefabStage1.SetActive(false);
        prefabStage2.SetActive(false);
        prefabStage3.SetActive(false);
        prefabStage4.SetActive(false);

        //Urèi, které materiály už jsou kompletní
        bool hasStone = stone.current >= stone.required;
        bool hasWood = wood.current >= wood.required;
        bool hasOre = ore.current >= ore.required;

        //Nastav aktivní prefab podle dokonèenosti
        int currentStage = 1;

        if (hasStone && hasWood && hasOre)
        {
            prefabStage4.SetActive(true);
            currentStage = 4;
            DestroyPacks();
        }
        else if (hasStone && hasWood)
        {
            prefabStage3.SetActive(true);
            currentStage = 3;
        }
        else if (hasStone)
        {
            prefabStage2.SetActive(true);
            currentStage = 2;
        }
        else
        {
            prefabStage1.SetActive(true);
            currentStage = 1;
        }

        //Aktualizuj vizuální stav jednotlivých materiálù
        // - použité materiály (souèasné i minulé fáze) ukazují 1/3 kapacity
        // - ostatní ukazují svùj reálný progress
        UpdateMaterialVisual(stone, currentStage > 1);
        UpdateMaterialVisual(wood, currentStage > 2);
        UpdateMaterialVisual(ore, currentStage > 3);
    }


    /// <summary>
    /// Zobrazí správný vizuální stav materiálu podle jeho aktuálního progresu.
    /// </summary>
    void UpdateMaterialVisual(BuildingMaterial mat, bool used)
    {
        if (mat == null || mat.visualStages == null)
            return;

        //Skryj všechny vizuální úrovnì materiálu
        foreach (var v in mat.visualStages)
            if (v != null)
                v.SetActive(false);

        //Urèi procentuální stav (0–1)
        float ratio;

        if (!used)
        {
            // Materiál se ještì používá — zobraz reálný progress
            ratio = Mathf.Clamp01((float)mat.current / mat.required);
        }
        else
        {
            // Materiál už byl použit — zobraz symbolicky 1/3 zásoby
            ratio = 0.33f;
        }

        // Vyber správný vizuální model podle pomìru
        var stages = mat.visualStages;
        if (stages.Length == 0)
            return;

        if (ratio >= 1f && stages.Length > 3)
            stages[3].SetActive(true);
        else if (ratio > 0.66f && stages.Length > 2)
            stages[2].SetActive(true);
        else if (ratio > 0.33f && stages.Length > 1)
            stages[1].SetActive(true);
        else
            stages[0].SetActive(true);
    }

    public void DestroyPacks()
    {
        Destroy(stone.gameObject);
        Destroy(wood.gameObject);
        Destroy(ore.gameObject);

    }
}
