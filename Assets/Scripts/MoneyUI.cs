using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void OnEnable()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += UpdateUI;
            UpdateUI(MoneyManager.Instance.Money);
        }
    }

    private void OnDisable()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= UpdateUI;
    }

    private void UpdateUI(int value)
    {
        moneyText.text = value.ToString("F2");
    }
}
