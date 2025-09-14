using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float speed = 8f;

    private RectTransform target;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {

        target = transform as RectTransform;

        originalScale = target.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        target.localScale = Vector3.Lerp(target.localScale, targetScale, Time.unscaledDeltaTime * speed); 
    }

    private void OnEnable()
    {
        // Resetovat velikost pokaždé, když se objekt zapne
        target.localScale = originalScale;
        targetScale = originalScale;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
