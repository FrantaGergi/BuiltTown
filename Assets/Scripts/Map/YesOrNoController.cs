using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class YesOrNoController : MonoBehaviour
{
    [Header("References ->")]
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    public bool YesOrNoPanelEnabled = false;


    private void Start()
    {
        Hide();
    }
    public void Show(string content, System.Action yesAction)
    {
        contentText.text = content;
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            yesAction?.Invoke();
            Hide();
        });
        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(Hide);

        // Deaktivujeme okamžitou interakci tlaèítek — zabráníme "propagaci" pùvodního kliknutí, které otevøelo panel.
        yesButton.interactable = false;
        noButton.interactable = false;

        // Aktivace panelu
        gameObject.SetActive(true);
        YesOrNoPanelEnabled = true;

        // Vyèistíme vybraný UI element, aby se nepøenášelo selektování / starý input
        EventSystem.current?.SetSelectedGameObject(null);

        // Povolit tlaèítka až v dalším frame (yield null) -> kliknutí které panel otevøelo nebude aktivovat tlaèítko
        StartCoroutine(EnableButtonsNextFrame());
    }

    private IEnumerator EnableButtonsNextFrame()
    {
        yield return null;
        yesButton.interactable = true;
        noButton.interactable = true;
    }

    public void Hide()
    {
       gameObject.SetActive(false);
       YesOrNoPanelEnabled = false;
    }
}
