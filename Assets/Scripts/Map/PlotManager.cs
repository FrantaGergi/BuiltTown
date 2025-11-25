using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlotManager : MonoBehaviour
{
    [Header("Nastavení Mapy")]
    [SerializeField] private float mapRadius = 50f;
    [SerializeField] private Vector3 mapCenter = Vector3.zero;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private List<Plot> plots = new List<Plot>();
    private List<LineRenderer> boundaryLines = new List<LineRenderer>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateBoundaryLinesFromChildren();
        RebuildPlotsFromBoundaryLines();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PopulateBoundaryLinesFromChildren()
    {
        boundaryLines.Clear();

        // Try to parse ids from names like "Plot_{id}_Boundary" so we can keep consistent ordering
        var parsed = new Dictionary<int, LineRenderer>();
        var unparsed = new List<LineRenderer>();

        foreach (Transform child in transform)
        {
            LineRenderer lr = child.GetComponent<LineRenderer>();
            if (lr != null && child.name.StartsWith("Plot_"))
            {
                // Expecting name like "Plot_3_Boundary" -> parts[1] == "3"
                string[] parts = child.name.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedId))
                {
                    parsed[parsedId] = lr;
                }
                else
                {
                    unparsed.Add(lr);
                }
            }
        }

        // Add parsed in order of id, then append any unparsed
        foreach (var kv in parsed.OrderBy(k => k.Key))
        {
            boundaryLines.Add(kv.Value);
        }

        foreach (var lr in unparsed)
        {
            boundaryLines.Add(lr);
        }
    }

    private void RebuildPlotsFromBoundaryLines()
    {
        plots.Clear();

        for (int i = 0; i < boundaryLines.Count; i++)
        {
            LineRenderer lr = boundaryLines[i];
            if (lr == null) continue;

            int posCount = lr.positionCount;
            List<Vector2> vertices = new List<Vector2>();

            for (int p = 0; p < posCount; p++)
            {
                Vector3 wp = lr.GetPosition(p);
                Vector2 local2D = new Vector2(wp.x - mapCenter.x, wp.z - mapCenter.z);
                vertices.Add(local2D);
            }

            // If last vertex duplicates the first (closed loop), remove the duplicate
            if (vertices.Count > 1 && Vector2.Distance(vertices[0], vertices[vertices.Count - 1]) < 0.01f)
            {
                vertices.RemoveAt(vertices.Count - 1);
            }

            // Compute center as average of vertices
            Vector2 center = Vector2.zero;
            if (vertices.Count > 0)
            {
                foreach (var v in vertices) center += v;
                center /= vertices.Count;
            }

            // Determine id from name if present
            int id = i;
            string name = lr.gameObject.name;
            if (name.StartsWith("Plot_"))
            {
                string[] parts = name.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedId)) id = parsedId;
            }

            // Determine unlocked state from color (approximate check)
            Color startCol = lr.startColor;
            bool isUnlocked = (startCol.g > 0.5f && startCol.r < 0.5f);

            Plot plot = new Plot
            {
                id = id,
                center = center,
                vertices = vertices,
                isUnlocked = isUnlocked,
                allowedBuilding = default
            };

            plots.Add(plot);
        }

        // Optional: sort plots by id so GetPlotAtPosition etc. are consistent with boundaryLines ordering
        plots = plots.OrderBy(p => p.id).ToList();
    }

    public Plot GetPlotAtPosition(Vector3 worldPosition)
    {
        Vector2 localPos = new Vector2(
            worldPosition.x - mapCenter.x,
            worldPosition.z - mapCenter.z
        );

        foreach (Plot plot in plots)
        {
            if (IsPointInPolygon(localPos, plot.vertices))
            {
                return plot;
            }
        }

        return null;
    }

    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool inside = false;
        int j = polygon.Count - 1;

        for (int i = 0; i < polygon.Count; i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    public List<Plot> GetAllPlots() => plots;

    public void UnlockPlot(int plotId)
    {
        Plot plot = plots.Find(p => p.id == plotId);
        if (plot != null)
        {
            plot.isUnlocked = true;
            UpdatePlotVisuals(plot);
        }
    }

    private void UpdatePlotVisuals(Plot plot)
    {
        if (plot.id < boundaryLines.Count)
        {
            LineRenderer lr = boundaryLines[plot.id];
            lr.startColor = plot.isUnlocked ? Color.green : Color.gray;
            lr.endColor = plot.isUnlocked ? Color.green : Color.gray;
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Nakresli kruh mapy
        Gizmos.color = Color.yellow;
        DrawCircleGizmo(mapCenter, mapRadius, 64);

        // Nakresli støedy pozemkù
        foreach (Plot plot in plots)
        {
            Gizmos.color = plot.isUnlocked ? Color.green : Color.red;
            Vector3 centerPos = new Vector3(
                mapCenter.x + plot.center.x,
                mapCenter.y,
                mapCenter.z + plot.center.y
            );
            Gizmos.DrawSphere(centerPos, 0.5f);
        }
    }

    private void DrawCircleGizmo(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle += angleStep;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(rad) * radius,
                0,
                Mathf.Sin(rad) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
