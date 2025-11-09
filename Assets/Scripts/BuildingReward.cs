using UnityEngine;

public class BuildingReward : MonoBehaviour
{
    public int reward = 50;

    public void GiveReward()
    {
        MoneyManager.Instance.AddMoney(reward);
    }
}
