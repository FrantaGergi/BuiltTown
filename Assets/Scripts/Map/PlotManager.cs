using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlotManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private float mapRadius = 50f;
    [SerializeField] private Vector3 mapCenter = Vector3.zero;

    [Header("Materials")]
    [SerializeField] private Material lockedPlotMaterial;
    [SerializeField] private Material availableToUnlockMaterial;
    [SerializeField] private Material unlockedPlotMaterial;
    [SerializeField] private Material buildedPlotMaterial;

    [Header("Initial state")]
    [SerializeField] private int initialUnlockedPlotId = 17;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private List<Plot> plots = new List<Plot>();
    private List<LineRenderer> boundaryLines = new List<LineRenderer>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateBoundaryLinesFromChildren();
        RebuildPlotsFromBoundaryLines();

        // Setup initial states and visuals
        InitializePlotStates(initialUnlockedPlotId);
        ApplyMaterialsToPlots();
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

            // Map previous isUnlocked to state
            plot.state = isUnlocked ? PlotState.Unlocked : PlotState.Locked;

            plots.Add(plot);
        }

        // Optional: sort plots by id so GetPlotAtPosition etc. are consistent with boundaryLines ordering
        plots = plots.OrderBy(p => p.id).ToList();
    }

    private void InitializePlotStates(int unlockedPlotId)
    {
        // Start with all locked
        foreach (var p in plots)
        {
            if (p.state == PlotState.Unlocked) // preserve if already unlocked from visuals
                continue;
            p.state = PlotState.Locked;
            p.isUnlocked = false;
        }

        // Unlock the specified plot if it exists
        var startPlot = plots.Find(p => p.id == unlockedPlotId);
        if (startPlot != null)
        {
            startPlot.state = PlotState.Unlocked;
            startPlot.isUnlocked = true;

            // Set neighbors to AvailableToUnlock
            foreach (var p in plots)
            {
                if (p.state == PlotState.Locked && AreNeighbors(startPlot, p))
                {
                    p.state = PlotState.AvailableToUnlock;
                }
            }
        }
    }

    private bool AreNeighbors(Plot a, Plot b, float epsilon = 0.01f)
    {
        if (a.vertices == null || b.vertices == null) return false;

        // Build list of edges as unordered pairs
        var edgesA = new List<(Vector2, Vector2)>();
        for (int i = 0; i < a.vertices.Count; i++)
        {
            Vector2 v0 = a.vertices[i];
            Vector2 v1 = a.vertices[(i + 1) % a.vertices.Count];
            edgesA.Add((v0, v1));
        }

        var edgesB = new List<(Vector2, Vector2)>();
        for (int i = 0; i < b.vertices.Count; i++)
        {
            Vector2 v0 = b.vertices[i];
            Vector2 v1 = b.vertices[(i + 1) % b.vertices.Count];
            edgesB.Add((v0, v1));
        }

        foreach (var ea in edgesA)
        {
            foreach (var eb in edgesB)
            {
                bool match01 = Vector2.Distance(ea.Item1, eb.Item1) < epsilon && Vector2.Distance(ea.Item2, eb.Item2) < epsilon;
                bool match02 = Vector2.Distance(ea.Item1, eb.Item2) < epsilon && Vector2.Distance(ea.Item2, eb.Item1) < epsilon;
                if (match01 || match02) return true;
            }
        }

        return false;
    }

    private LineRenderer GetLineRendererForPlot(Plot plot)
    {
        // Try to find by name first
        string expectedPrefix = $"Plot_{plot.id}_";
        foreach (var lr in boundaryLines)
        {
            if (lr == null) continue;
            if (lr.gameObject.name.StartsWith(expectedPrefix)) return lr;
        }

        // Fallback: if index aligns
        if (plot.id >= 0 && plot.id < boundaryLines.Count)
            return boundaryLines[plot.id];

        return null;
    }

    private void ApplyMaterialsToPlots()
    {
        foreach (var plot in plots)
        {
            UpdatePlotVisuals(plot);
        }
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
        if (plot != null && plot.state != PlotState.Unlocked)
        {
            plot.isUnlocked = true;
            plot.state = PlotState.Unlocked;

            // Make neighbors available to unlock
            foreach (var p in plots)
            {
                if (p.state == PlotState.Locked && AreNeighbors(plot, p))
                {
                    p.state = PlotState.AvailableToUnlock;
                }
            }

            UpdatePlotVisuals(plot);
        }
    }

    public void SetPlotBuilt(int plotId)
    {
        Plot plot = plots.Find(p => p.id == plotId);
        if (plot != null)
        {
            plot.state = PlotState.Built;
            UpdatePlotVisuals(plot);
        }
    }

    private void UpdatePlotVisuals(Plot plot)
    {
        LineRenderer lr = GetLineRendererForPlot(plot);
        if (lr == null) return;

        switch (plot.state)
        {
            case PlotState.Locked:
                if (lockedPlotMaterial != null) lr.material = lockedPlotMaterial;
                lr.startColor = Color.gray;
                lr.endColor = Color.gray;
                break;
            case PlotState.AvailableToUnlock:
                if (availableToUnlockMaterial != null) lr.material = availableToUnlockMaterial;
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
                break;
            case PlotState.Unlocked:
                if (unlockedPlotMaterial != null) lr.material = unlockedPlotMaterial;
                lr.startColor = Color.green;
                lr.endColor = Color.green;
                break;
            case PlotState.Built:
                if (buildedPlotMaterial != null) lr.material = buildedPlotMaterial;
                lr.startColor = Color.blue;
                lr.endColor = Color.blue;
                break;
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
