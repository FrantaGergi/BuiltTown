using UnityEngine;
using UnityEngine.InputSystem;

public class UIBuildingMailboxController : MonoBehaviour
{
    [Header("References ->")]
    [SerializeField] RectTransform optionPanel;
    [SerializeField] TMPro.TextMeshProUGUI optionDescriptionText;
    [SerializeField] RectTransform informationPanel;
    [SerializeField] RectTransform progressbar;
    [SerializeField] RectTransform maxSizeProgressBar;
    [SerializeField] TMPro.TextMeshProUGUI timeToEarnText;
    [SerializeField] TMPro.TextMeshProUGUI titleText;
    [SerializeField] TMPro.TextMeshProUGUI descriptionText;

    private float maxProgressWidth = 100f;
    private float timeToEarnValue = 0f;
    private float currentTimeToEarnValue = 0f;

    private Mailbox mailbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (optionPanel != null) optionPanel.gameObject.SetActive(false);
        if (informationPanel != null) informationPanel.gameObject.SetActive(false);

        // zajistit rozmìry UI (pokud layout ještì není hotový)
        Canvas.ForceUpdateCanvases();
        if (maxSizeProgressBar != null)
        {
            maxProgressWidth = maxSizeProgressBar.rect.width;
            if (maxProgressWidth <= 0f)
                maxProgressWidth = maxSizeProgressBar.sizeDelta.x;
            if (maxProgressWidth <= 0f)
                maxProgressWidth = 100f;
        }

        ApplyProgress(0f);
    }

    // Update is called once per frame
    void Update()
    {
        // když je otevøené informationPanel, animujeme a poèítáme èas dolù
        if (informationPanel != null && informationPanel.gameObject.activeSelf)
        {
            if (timeToEarnValue > 0f)
            {
                // ubíráme èas; pokud dosáhne nuly, ihned restartujeme od poèátku (s zachováním pøebytku)
                currentTimeToEarnValue -= Time.deltaTime;

                if (currentTimeToEarnValue <= 0f)
                {
                    // zachovat „pøebytek“ èasu tak, aby cyklus nezaostával (precizní opakování)
                    float overflow = -currentTimeToEarnValue; // >= 0
                    // remainder v rozmezí [0, timeToEarnValue)
                    float remainder = Mathf.Repeat(overflow, timeToEarnValue);
                    currentTimeToEarnValue = timeToEarnValue - remainder;
                    // Pozn.: tímto se timer okamžitì restartuje a bìží znovu
                }
            }

            float normalized;
            if (timeToEarnValue <= 0f)
            {
                // one-time: pokud už vyplaceno (currentTimeToEarnValue == 0) -> plný progress
                normalized = currentTimeToEarnValue <= 0f ? 1f : 0f;
            }
            else
            {
                // pøímo podle zbývajícího èasu — odpovídá skuteènému èasu
                float safeCurrent = Mathf.Clamp(currentTimeToEarnValue, 0f, timeToEarnValue);
                normalized = Mathf.Clamp01(1f - (safeCurrent / timeToEarnValue));
            }

            ApplyProgress(normalized);

            // aktualizuj jen pokud je text aktivní (mùže být vypnut pro jednorázovou vyplacenou odmìnu)
            if (timeToEarnText != null && timeToEarnText.gameObject.activeSelf)
                timeToEarnText.text = FormatTime(Mathf.Max(0f, currentTimeToEarnValue));
        }
    }

    public void OnCloseMailbox()
    {
        if (optionPanel != null) optionPanel.gameObject.SetActive(false);
        if (informationPanel != null) informationPanel.gameObject.SetActive(false);

        MenuManager.Instance.CloseUIEnviroment(); // obnoví hru a ovládání hráèe pøi zavøení mailboxu
    }

    public void SetOption(Mailbox mailbox, string rentAmmont, string sellAmmount)
    {
        MenuManager.Instance.OpenUIEnviroment(); // zajišuje omezení hráèe a hry pøi otevøení mailboxu

        this.mailbox = mailbox;

        if (optionDescriptionText != null)
        {
            optionDescriptionText.text = $"Rent for ${rentAmmont}\nSell for ${sellAmmount}";
        }

        if (optionPanel != null) optionPanel.gameObject.SetActive(true);
        if (informationPanel != null) informationPanel.gameObject.SetActive(false);
    }

    // ammountToShow je ponecháno ve volání (mùžeš upravit dle potøeby)
    public void SetInformation(int ammountToShow, float timeToEarn, float currentTimeToEarn)
    {
        MenuManager.Instance.OpenUIEnviroment(); // zajišuje omezení hráèe a hry pøi otevøení mailboxu

        if (optionPanel != null) optionPanel.gameObject.SetActive(false);
        if (informationPanel != null) informationPanel.gameObject.SetActive(true);

        // uložíme hodnoty, Update() bude pøímo odrážet plynutí èasu
        timeToEarnValue = timeToEarn;
        currentTimeToEarnValue = timeToEarn > 0f ? Mathf.Clamp(currentTimeToEarn, 0f, timeToEarn) : 0f;

        // zachováno podle tvého požadavku
        if (titleText != null)
            titleText.text = timeToEarn == 0 ? "Building Sold" : "Building Rented";

        if(maxSizeProgressBar != null)
            maxSizeProgressBar.gameObject.SetActive(timeToEarn > 0f);

        if (descriptionText != null)
            descriptionText.text = timeToEarn == 0 ?
                $"You have sold the building and received ${ammountToShow}." :
                $"You are receiving ${ammountToShow} every {FormatTime(timeToEarn)}.";

        // pokud jde o jednorázové vyplacení (timeToEarn == 0), vypni progress bar a timer text
        if (timeToEarn <= 0f)
        {
            if (progressbar != null)
                progressbar.gameObject.SetActive(false);
            if (timeToEarnText != null)
                timeToEarnText.gameObject.SetActive(false);
        }
        else
        {
            // renta: zajisti, aby byly prvky aktivní
            if (progressbar != null)
                progressbar.gameObject.SetActive(true);
            if (timeToEarnText != null)
                timeToEarnText.gameObject.SetActive(true);
        }

        float normalized;
        if (timeToEarnValue <= 0f)
            normalized = currentTimeToEarnValue <= 0f ? 1f : 0f;
        else
            normalized = Mathf.Clamp01(1f - (currentTimeToEarnValue / timeToEarnValue));

        // aplikuj progress
        ApplyProgress(normalized);

        if (timeToEarnText != null && timeToEarnText.gameObject.activeSelf)
            timeToEarnText.text = FormatTime(currentTimeToEarnValue);
    }

    // aplikuje šíøku progress baru (pøedpokládá, že maxSizeProgressBar urèuje maximální šíøku)
    private void ApplyProgress(float normalized)
    {
        if (progressbar == null || maxSizeProgressBar == null)
            return;

        float width = maxSizeProgressBar.rect.width;
        if (width <= 0f) width = maxProgressWidth;
        float newWidth = Mathf.Clamp01(normalized) * width;
        progressbar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    // jednoduché formátování èasu jako "m:ss"
    private string FormatTime(float seconds)
    {
        if (seconds <= 0f) return "00:00";
        int total = Mathf.CeilToInt(seconds);
        int minutes = total / 60;
        int secs = total % 60;
        return $"{minutes}:{secs:00}";
    }

    public void RentIt() 
    {
        mailbox.RentBuilding();
    }
    public void SellIt() 
    {
        mailbox.SellBuilding();
    }
}
