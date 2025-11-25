using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Plot
{
    public int id;
    public Vector2 center;
    public List<Vector2> vertices;
    public bool isUnlocked;
    public BuildingType allowedBuilding;
}

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    Park
}
