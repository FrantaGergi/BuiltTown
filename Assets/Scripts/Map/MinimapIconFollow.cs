using UnityEngine;

public class MinimapIconFollow : MonoBehaviour
{
    public RectTransform minimapRect;
    public RectTransform icon;
    public Transform target;
    public Camera minimapCamera;

    void Update()
    {
        float worldSize = minimapCamera.orthographicSize * 2f;

        Vector3 relative = target.position - minimapCamera.transform.position;

        float normX = relative.x / worldSize;
        float normY = relative.z / worldSize;

        float uiX = normX * minimapRect.rect.width;
        float uiY = normY * minimapRect.rect.height;

        icon.anchoredPosition = new Vector2(uiX, uiY);
    }

}
