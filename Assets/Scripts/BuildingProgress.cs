using UnityEngine;

public class BuildingProgress : MonoBehaviour
{


    [Header("Prefaby stavby")]
    public GameObject prefabStage1; // èistá parcela
    public GameObject prefabStage2; // bednìní (kámen)
    public GameObject prefabStage3; // kostra (døevo)
    public GameObject prefabStage4; // hotová (ruda)

    [Header("Materiály")]
    public BuildingMaterial stone;
    public BuildingMaterial wood;
    public BuildingMaterial ore;

    
    

    public void UpdateVisuals()
    {
        //Vypni všechny prefaby
        prefabStage1.SetActive(false);
        prefabStage2.SetActive(false);
        prefabStage3.SetActive(false);
        prefabStage4.SetActive(false);

        // Urèi aktuální fázi
        bool hasStone = stone.current >= stone.required;
        bool hasWood = wood.current >= wood.required;
        bool hasOre = ore.current >= ore.required;

        if (hasStone && hasWood && hasOre)
            prefabStage4.SetActive(true);
        else if (hasStone && hasWood)
            prefabStage3.SetActive(true);
        else if (hasStone)
            prefabStage2.SetActive(true);
        else
            prefabStage1.SetActive(true);

        //Aktualizuj materiálové vizuály
        UpdateMaterialVisual(stone, hasStone);
        UpdateMaterialVisual(wood, hasWood);
        UpdateMaterialVisual(ore, hasOre);
    }

    void UpdateMaterialVisual(BuildingMaterial mat, bool isUsedInStage)
    {
        if (mat == null || mat.visualStages == null)
            return;

        foreach (var v in mat.visualStages)
            if (v != null)
                v.SetActive(false);

        float ratio = (float)mat.current / mat.required;

        // Pokud je materiál u pouitı, nech aspoò 1/3 viditelnou
        if (isUsedInStage)
        {
            ratio = Mathf.Max(ratio, 0.33f);
        }

        var stages = mat.visualStages;

        if (stages.Length == 0)
            return;


         if (ratio > 0.66f && stages.Length > 2)
            stages[2].SetActive(true);
        else if (ratio > 0.33f && stages.Length > 1)
            stages[1].SetActive(true);
        else if (ratio >= 0f)
            stages[0].SetActive(true);
    }
}
