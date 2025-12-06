using TMPro;
using UnityEngine;
using System.Collections;

public class UIChooserOfBuilding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform miniHouse;
    [SerializeField] private RectTransform bigHouse;

    [SerializeField] private TextMeshProUGUI mWoodCount;
    [SerializeField] private TextMeshProUGUI mStoneCount;
    [SerializeField] private TextMeshProUGUI mOreCount;
    [SerializeField] private TextMeshProUGUI bWoodCount;
    [SerializeField] private TextMeshProUGUI bStoneCount;
    [SerializeField] private TextMeshProUGUI bOreCount;

    [SerializeField] private TextMeshProUGUI mRentCount;
    [SerializeField] private TextMeshProUGUI mSellCount;
    [SerializeField] private TextMeshProUGUI bRentCount;
    [SerializeField] private TextMeshProUGUI bSellCount;

    [Header("Show animation")]
    [SerializeField, Tooltip("Délka animace zvìtšení (v sekundách)")] private float showDuration = 0.18f;
    [SerializeField, Tooltip("Køivka easingu pro animaci")] private AnimationCurve showEase = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private Vector3 originalContentScale = Vector3.one;
    private Coroutine scaleCoroutine;

    void Start()
    {
        if (content != null)
        {
            // ulož originální scale pro animaci; pokud content je aktivní, použij jeho scale, jinak použij (1,1,1)
            originalContentScale = content.localScale != Vector3.zero ? content.localScale : Vector3.one;
        }
    }

    void Update()
    {
    }


    public void ShowMiniHouseCosts(Plot plot, bool show)
    {
        if (plot.MiniBuilding == null)
        {
            miniHouse.gameObject.SetActive(false);
            return;
        }

        mWoodCount.text = plot.MiniBuilding.buildingCore.woodCost.ToString();
        mStoneCount.text = plot.MiniBuilding.buildingCore.stoneCost.ToString();
        mOreCount.text = plot.MiniBuilding.buildingCore.oreCost.ToString();
        mRentCount.text = plot.MRentPrice.ToString();
        mSellCount.text = plot.MSellPrice.ToString();
        miniHouse.gameObject.SetActive(show);

       // plot.MiniBuilding.buildingCore.buildingReward.FinalAmmountToGive = p
    }
    public void ShowBigHouseCosts(Plot plot, bool show)
    {
        if (plot.BigBuilding == null)
        {
            bigHouse.gameObject.SetActive(false);
            return;
        }

        bWoodCount.text = plot.BigBuilding.buildingCore.woodCost.ToString();
        bStoneCount.text = plot.BigBuilding.buildingCore.stoneCost.ToString();
        bOreCount.text = plot.BigBuilding.buildingCore.oreCost.ToString();
        bRentCount.text = plot.BRentPrice.ToString();
        bSellCount.text = plot.BSellPrice.ToString();
        bigHouse.gameObject.SetActive(show);
    }


    public void Hide()
    {
        // Zastav animaci pokud bìží a skryj content
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        if (content != null)
        {
            // pøi schování ponecháme scale=original (nebo nastavit na nulový, podle potøeby)
            content.localScale = originalContentScale;
            content.gameObject.SetActive(false);
        }
    }

    // Pùvodní bezparametrové volání zachováme (bude animovat)
    public void Show()
    {
        Show(true);
    }

    // Nová overload: umožní zapnout/vypnout animaci
    public void Show(bool animate)
    {
        if (content == null)
        {
            Debug.LogWarning("UIChooserOfBuilding.Show: content není pøiøazen.");
            return;
        }

        // Ujistíme se, že máme uložený originální scale
        if (originalContentScale == Vector3.zero)
            originalContentScale = Vector3.one;

        // Zastav pøedchozí animaci
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        // Aktivuj GameObject pøed startem animace
        content.gameObject.SetActive(true);

        if (!animate || showDuration <= 0f)
        {
            content.localScale = originalContentScale;
            return;
        }

        // Nastav poèáteèní scale na nulu a spus plynulou animaci do originálu
        content.localScale = Vector3.zero;
        scaleCoroutine = StartCoroutine(ScaleRoutine(Vector3.zero, originalContentScale, showDuration));
    }

    private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / duration);
            float eased = showEase.Evaluate(norm);
            content.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        content.localScale = to;
        scaleCoroutine = null;
    }

   
}
