using System;
using System.Collections;
using UnityEngine;

public class BuildingReward : MonoBehaviour
{
    public enum RewardMode
    {
        OneTime,
        Recurring
    }

    [Header("Mode")]
    public RewardMode mode = RewardMode.OneTime;
    [Tooltip("Spustí se automaticky pøi vytvoøení komponenty (Awake/Start).")]
    public bool startOnAwake = false;

    [Header("One-time reward")]
    [SerializeField] private int rewardAmount = 50;

    [Header("Recurring (rent)")]
    [SerializeField] private int rentAmount = 5;
    [SerializeField, Tooltip("Interval v sekundách (default 5 minut = 300s)")]
    private float rentIntervalSeconds = 300f;

    // Event volaný pokaždé, když je vyplacena èástka (amount)
    public event Action<int> OnRewardPaid;

    private Coroutine rentCoroutine;

    private void Start()
    {
        if (startOnAwake)
            Trigger();
    }

    private void OnDestroy()
    {
        StopRecurring();
    }

    /// <summary>
    /// Spustí odmìnu podle aktuálního režimu.
    /// </summary>
    public void Trigger()
    {
        if (mode == RewardMode.OneTime)
        {
            StartCoroutine(GiveOneTimeWhenReady());
        }
        else
        {
            StartRecurring();
        }
    }

    /// <summary>
    /// Okamžité jednorázové vyplacení (pokud MoneyManager není dostupný hned, poèkáme).
    /// </summary>
    public IEnumerator GiveOneTimeWhenReady()
    {
        yield return WaitForMoneyManager();
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(rewardAmount);
            OnRewardPaid?.Invoke(rewardAmount);
        }
    }

    /// <summary>
    /// Spustí periodické vyplácení (rent).
    /// </summary>
    public void StartRecurring()
    {
        if (rentCoroutine != null) return;
        rentCoroutine = StartCoroutine(RunRentCoroutine());
    }

    /// <summary>
    /// Zastaví periodické vyplácení.
    /// </summary>
    public void StopRecurring()
    {
        if (rentCoroutine != null)
        {
            StopCoroutine(rentCoroutine);
            rentCoroutine = null;
        }
    }

    private IEnumerator RunRentCoroutine()
    {
        yield return WaitForMoneyManager();

        if (MoneyManager.Instance == null)
        {
            Debug.LogWarning($"{name}: MoneyManager.Instance není dostupný — rent nebude vyplácen.");
            yield break;
        }

        // První platba ihned, nebo poèkat interval? -- aktuálnì platba ihned a poté každých rentIntervalSeconds.
        while (true)
        {
            MoneyManager.Instance.AddMoney(rentAmount);
            OnRewardPaid?.Invoke(rentAmount);
            yield return new WaitForSeconds(rentIntervalSeconds);
        }
    }

    /// <summary>
    /// Èeká na vytvoøení MoneyManager.Instance s timeoutem.
    /// </summary>
    private IEnumerator WaitForMoneyManager()
    {
        const int maxFrames = 600; // ~10s pøi 60 FPS
        int frames = 0;
        while (MoneyManager.Instance == null && frames < maxFrames)
        {
            frames++;
            yield return null;
        }
    }

    // Dodateèné utilitky pro runtime konfiguraci:
    public void SetOneTimeAmount(int amount) => rewardAmount = amount;
    public void SetRentAmount(int amount) => rentAmount = amount;
    public void SetRentIntervalSeconds(float seconds) => rentIntervalSeconds = Mathf.Max(1f, seconds);
}
