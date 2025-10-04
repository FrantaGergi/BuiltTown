using UnityEngine;
using System.Collections;

public class BuildingPlot : MonoBehaviour
{
    public Canvas plotCanvas;
    public Transform player;
    public Transform centerPoint;
    [Range(0f, 20f)]
    public float radius;

    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float scaleDuration = 0.5f;

    private Coroutine scaleCoroutine;
    private bool isVisible = false;

    void Start()
    {
        if (plotCanvas != null)
        {
            plotCanvas.enabled = false;
            plotCanvas.transform.localScale = Vector3.zero;
        }
    }

    private void OnDrawGizmos()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPoint.position, radius);
        }
    }


    void Update()
    {
        if (player == null || plotCanvas == null)
            return;

        bool shouldShow = Vector3.Distance(centerPoint.position, player.position) < radius;

        if (shouldShow && !isVisible)
        {
            isVisible = true;
            plotCanvas.enabled = true;
            StartScaleAnimation(new Vector3(0.00470765447f, 0.049999997f, 0.0048121796f));
        }
        else if (!shouldShow && isVisible)
        {
            isVisible = false;
            StartScaleAnimation(Vector3.zero, disableOnEnd: true);
        }

        if (isVisible)
        {
            Vector3 targetPosition = new Vector3(player.position.x, plotCanvas.transform.position.y, player.position.z);
            Vector3 direction = targetPosition - plotCanvas.transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                plotCanvas.transform.rotation = Quaternion.Slerp(plotCanvas.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void StartScaleAnimation(Vector3 targetScale, bool disableOnEnd = false)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCanvas(targetScale, disableOnEnd));
    }

    private IEnumerator ScaleCanvas(Vector3 targetScale, bool disableOnEnd)
    {
        Vector3 startScale = plotCanvas.transform.localScale;
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            plotCanvas.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / scaleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        plotCanvas.transform.localScale = targetScale;
        if (disableOnEnd && targetScale == Vector3.zero)
            plotCanvas.enabled = false;
    }
}
