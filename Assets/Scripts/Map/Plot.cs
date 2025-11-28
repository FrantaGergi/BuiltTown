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

    public int costToUnlock;

    public PlotState state = PlotState.Locked;

}

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    Park
}

public enum PlotState
{
    Locked,
    AvailableToUnlock,
    Unlocked,
    Built
}
