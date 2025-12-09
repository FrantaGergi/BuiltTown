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

    private int rentDivisor = 10000;
    private int sellDivisor = 5000;
    public int costToUnlock;
    public int MRentPrice
    {
        get
        {
            float multiplier = costToUnlock / rentDivisor;
            var site = MiniBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (int)((multiplier * site.buildingCore.buildingReward.BuildingRentAmount)
                + site.buildingCore.buildingReward.BuildingRentAmount);
        }
    }

    public int MSellPrice
    {
        get
        {
            float multiplier = costToUnlock / sellDivisor;
            var site = MiniBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (int)((multiplier * site.buildingCore.buildingReward.BuildingRewardAmount)
                + site.buildingCore.buildingReward.BuildingRewardAmount);
        }
    }

    public int BRentPrice
    {
        get
        {
            float multiplier = costToUnlock / rentDivisor;
            var site = BigBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (int)((multiplier * site.buildingCore.buildingReward.BuildingRentAmount)
                + site.buildingCore.buildingReward.BuildingRentAmount);
        }
    }

    public int BSellPrice
    {
        get
        {
            float multiplier = costToUnlock / sellDivisor;
            var site = BigBuilding;
            if (site?.buildingCore?.buildingReward == null)
                return 0;
            return (int)((multiplier * site.buildingCore.buildingReward.BuildingRewardAmount)
                + site.buildingCore.buildingReward.BuildingRewardAmount);
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
