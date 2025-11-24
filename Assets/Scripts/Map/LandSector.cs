using UnityEngine;

public enum SectorState { Locked, ForSale, Unlocked, Built }

public class LandSector : MonoBehaviour
{
    public int id;
    public Vector2[] polygon; // v world XZ (y ignorujeme)
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public LineRenderer lineRenderer;
    public SectorState state = SectorState.Locked;

    public void SetColor(Color c)
    {
        if (meshRenderer != null)
            meshRenderer.material.color = c;
    }

    public bool ContainsPoint(Vector3 worldPoint)
    {
        Vector2 p = new Vector2(worldPoint.x, worldPoint.z);
        return PolygonUtils.PointInPolygon(p, polygon);
    }
}
