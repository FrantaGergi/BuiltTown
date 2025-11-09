using System;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public event Action<int> OnMoneyChanged;

    private int money;
    public int MoneySaveData => money;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int Money => money;

    public bool TrySpend(int amount)
    {
        if (money < amount)
            return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);
        return true;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }
}
