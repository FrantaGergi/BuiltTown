using UnityEngine;

public class BuildingMaterial : MonoBehaviour
{
    public string materialName;
    public int required; // kolik je potøeba celkem
    public int current;  // kolik je aktuálnì
    public GameObject[] visualStages; // 3 prefaby: 1/3, 2/3, plné
}
