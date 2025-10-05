using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Building")]
public class BuildingSO : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject prefab0;
    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;

    [Header("Info")]
    public string buildingName;
    public Sprite icon;
    public string description;
    public int price;

}
    
