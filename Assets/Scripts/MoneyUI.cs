using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private bool subscribed = false;

    private void OnEnable()
    {
        // Pokud už existuje Instance, pøihlas se hned
        if (MoneyManager.Instance != null)
        {
            Subscribe();
            UpdateUI(MoneyManager.Instance.Money);
            return;
        }
    }

    private void OnDisable()
    {
      

        // Odhlaš, pokud jsme byli pøihlášeni
        if (subscribed && MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged -= UpdateUI;
            subscribed = false;
        }
    }

    private void Start()
    {
        Subscribe();
    }



    private void Subscribe()
    {
        if (subscribed) return;
        MoneyManager.Instance.OnMoneyChanged += UpdateUI;
        subscribed = true;
        UpdateUI(MoneyManager.Instance.Money);
    }

    private void UpdateUI(int value)
    {
        if (moneyText == null) return;
        moneyText.text = FormatNumber(value);
    }

    private string FormatNumber(long value)
    {
        if (value < 1000)
            return value.ToString();

        if (value < 1_000_000)
        {
            double v = Math.Floor((value / 1000d) * 10) / 10; // floor na 0.1k
            return v.ToString("0.#") + "k";
        }

        if (value < 1_000_000_000)
        {
            double v = Math.Floor((value / 1_000_000d) * 10) / 10; // floor na 0.1M
            return v.ToString("0.#") + "M";
        }

        {
            double v = Math.Floor((value / 1_000_000_000d) * 10) / 10; // floor na 0.1B
            return v.ToString("0.#") + "B";
        }
    }
}
