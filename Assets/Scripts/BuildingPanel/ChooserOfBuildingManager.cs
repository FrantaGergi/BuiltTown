using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChooserOfBuildingManager : MonoBehaviour
{
    public bool isChooserOfBuildingOpen = false;

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private UIChooserOfBuilding uiChooserOfBuilding;

    [Header("Open animation")]
    [SerializeField, Tooltip("Délka animace otevøení / zavøení v sekundách")] private float openAnimationDuration = 0.18f;
    [SerializeField, Tooltip("Easing køivka pro animaci (0..1)")] private AnimationCurve openEase = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private Plot currentPlot;

    // Coroutine reference pro bezpeèné pøepínání / zrušení
    private Coroutine openCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiChooserOfBuilding.Hide();
        isChooserOfBuildingOpen = false;
    }

    public void CloseUIChooser()
    {
        // pokud bìží otevírací coroutine, zrušíme ji a zavøeme UI plynule
        if (openCoroutine != null)
        {
            StopCoroutine(openCoroutine);
            openCoroutine = null;
        }

        // pokud je aktivní UI, skryjeme ho pøes jeho API
        uiChooserOfBuilding.Hide();
        isChooserOfBuildingOpen = false;
    }

    public void OpenBuildingChooser(Plot plot)
    {
        if (plot == null) return;

        // pøiprav obsah ihned (texty / ceny) aby byl UI pøipravený k animaci
        uiChooserOfBuilding.ShowBigHouseCosts(plot);
        uiChooserOfBuilding.ShowMiniHouseCosts(plot);

        currentPlot = plot;

        // restartujeme pøípadnou bìžící coroutine (blbuvzdorné)
        if (openCoroutine != null)
        {
            StopCoroutine(openCoroutine);
            openCoroutine = null;
        }

        // nastavíme flag hned - UI se bude otevírat
        isChooserOfBuildingOpen = true;

        // spustíme otevírací sekvenci o jeden frame pozdìji s animací
        openCoroutine = StartCoroutine(OpenWithDelayAndAnimate());
    }

    private IEnumerator OpenWithDelayAndAnimate()
    {
        // poèkej jeden frame, aby se vstupní event, který spustil otevøení, zpracoval
        yield return null;

        if (uiChooserOfBuilding == null)
        {
            isChooserOfBuildingOpen = false;
            openCoroutine = null;
            yield break;
        }

        // Zavolat Show() -> aktivuje UI (pokud není aktivní)
        uiChooserOfBuilding.Show();

        // Najdi RectTransform, který budeme animovat
        RectTransform rt = uiChooserOfBuilding.GetComponent<RectTransform>();
        if (rt == null)
        {
            // fallback: zkus první RectTransform v children
            rt = uiChooserOfBuilding.GetComponentInChildren<RectTransform>();
        }

        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        // Pokud máme RectTransform, nastavíme poèáteèní stav a animujeme scale
        if (rt != null)
        {
            // uložíme aktuální finální scale (pro pøípad, že Show() nastaví nìco jiného)
            targetScale = rt.localScale;
            // nastavíme na 0 a animujeme na target
            rt.localScale = startScale;

            float t = 0f;
            while (t < openAnimationDuration)
            {
                t += Time.deltaTime;
                float norm = Mathf.Clamp01(t / openAnimationDuration);
                float eased = openEase != null ? openEase.Evaluate(norm) : Mathf.SmoothStep(0f, 1f, norm);
                rt.localScale = Vector3.LerpUnclamped(startScale, targetScale, eased);
                yield return null;
            }
            rt.localScale = targetScale;
        }

        // dokonèeno
        openCoroutine = null;
    }

    public void OnBigBuildingSelected()
    {
        if (currentPlot == null || currentPlot.BigBuilding == null) return;

        currentPlot.BigBuilding.buildingCore.buildingReward.FinalRentAmount = currentPlot.BRentPrice;
        currentPlot.BigBuilding.buildingCore.buildingReward.FinalRewardAmount = currentPlot.BSellPrice;
        CloseUIChooser();

        currentPlot.state = PlotState.Built;
        currentPlot.BigBuilding.gameObject.SetActive(true);
    }

    public void OnMiniBuildingSelected()
    {
        if (currentPlot == null || currentPlot.MiniBuilding == null) return;

        currentPlot.MiniBuilding.buildingCore.buildingReward.FinalRentAmount = currentPlot.MRentPrice;
        currentPlot.MiniBuilding.buildingCore.buildingReward.FinalRewardAmount = currentPlot.MSellPrice;
        CloseUIChooser();

        currentPlot.state = PlotState.Built;
        currentPlot.MiniBuilding.gameObject.SetActive(true);
    }
}

