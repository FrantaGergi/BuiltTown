using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Plot
{
    public int id;
    public Vector2 center;
    public List<Vector2> vertices;
    public bool isUnlocked;
    public BuildingSite MiniBuilding;
    public BuildingSite BigBuilding;
    public BuildingSite CurrentBuilding;

    public int costToUnlock;
    public int MRentPrice
    {
        get
        {
            var site = MiniBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (costToUnlock / 1000) + site.buildingCore.buildingReward.BuildingRentAmount;
        }
    }

    public int MSellPrice
    {
        get
        {
            var site = MiniBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (costToUnlock / 1500) + site.buildingCore.buildingReward.BuildingRewardAmount;
        }
    }

    public int BRentPrice
    {
        get
        {
            var site = BigBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (costToUnlock / 1000) + site.buildingCore.buildingReward.BuildingRewardAmount;
        }
    }

    public int BSellPrice
    {
        get
        {
            var site = BigBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (costToUnlock / 1500) + site.buildingCore.buildingReward.BuildingRentAmount;
        }
    }

    public PlotState state = PlotState.Locked;


    public Plot(int id, Vector2 center, List<Vector2> vertices, int costToUnlock,
        PlotState plotState, BuildingSite MiniBuilding, BuildingSite BigBuilding)
    {
        this.id = id;
        this.center = center;
        this.vertices = vertices;
        this.costToUnlock = costToUnlock;
        this.isUnlocked = false;
        this.CurrentBuilding = null;
        this.MiniBuilding = MiniBuilding;
        this.BigBuilding = BigBuilding;
        this.state = plotState;
    }



}



public enum PlotState
{
    Locked,
    AvailableToUnlock,
    Unlocked,
    Built
}
