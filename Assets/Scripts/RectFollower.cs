using UnityEngine;

/// <summary>
/// Pøizpùsobuje target velikosti source, pouze pøi zmìnì rozmìrù.
/// Nejefektivnìjší zpùsob – žádný Update().
/// </summary>
[ExecuteAlways]
public class RectFollower : MonoBehaviour
{
    [SerializeField] private RectTransform source;
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector2 padding = Vector2.zero;
    [SerializeField] private bool copyPosition = false;
    [SerializeField] private bool copyWidth = true;
    [SerializeField] private bool copyHeight = true;

    private Vector2 lastSize;

    private void OnEnable()
    {
        UpdateTarget();
    }

    private void OnRectTransformDimensionsChange()
    {
        // Tato metoda se zavolá, když se zmìní rozmìr tohoto RectTransformu.
        // Ale my chceme reagovat na zmìnu source, ne sebe – proto malý trik:
        if (source == null || target == null) return;

        Vector2 currentSize = source.rect.size;
        if (currentSize != lastSize)
        {
            UpdateTarget();
            lastSize = currentSize;
        }
    }

    private void UpdateTarget()
    {
        if (source == null || target == null) return;

        Vector2 size = source.rect.size;
        Vector2 newSize = target.sizeDelta;

        if (copyWidth)
            newSize.x = size.x + padding.x;
        if (copyHeight)
            newSize.y = size.y + padding.y;

        target.sizeDelta = newSize;

        if (copyPosition)
            target.position = source.position;
    }

    private void OnValidate()
    {
        UpdateTarget();
    }
}
