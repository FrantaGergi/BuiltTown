using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Plot
{
    public int id;
    public Vector2 center;
    public List<Vector2> vertices;
    public bool isUnlocked;
    public BuildingSite Building;

    public int costToUnlock;

    public PlotState state = PlotState.Locked;

}



public enum PlotState
{
    Locked,
    AvailableToUnlock,
    Unlocked,
    Built
}
