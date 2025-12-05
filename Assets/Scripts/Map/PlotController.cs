using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private float mapRadius = 100f;
    [SerializeField] private Vector3 mapCenter = Vector3.zero;

    [Header("Materials")]
    [SerializeField] private Material lockedPlotMaterial;
    [SerializeField] private Material availableToUnlockMaterial;
    [SerializeField] private Material unlockedPlotMaterial;
    [SerializeField] private Material buildedPlotMaterial;

    [Header("Initial state")]
    [SerializeField] private int initialUnlockedPlotId = 17;

    [Header("Neighbor detection - NEW SETTINGS")]
    [SerializeField, Range(0.1f, 10f)] private float neighborDistanceThreshold = 5.0f;
    [SerializeField] private int minSharedVertices = 2;
    [SerializeField] private bool debugNeighbors = false;
    [SerializeField] private bool debugAllPairs = false;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private List<Plot> plots = new List<Plot>();
    private List<LineRenderer> boundaryLines = new List<LineRenderer>();

    [Header("Settings")]
    public int[] plots_prices_Multiplier = new int[20];
    public int default_plot_price = 4500;

    void Start()
    {
        PopulateBoundaryLinesFromChildren();
        RebuildPlotsFromBoundaryLines();

        InitializePlotStates(initialUnlockedPlotId);
        ApplyMaterialsToPlots();

        if(plots_prices_Multiplier.Length < plots.Count)
        {
            Debug.LogError("PlotController: plots_prices_Multiplier array length is less than number of plots. Some plots will have default price 0.");
        }
        else
        {
            for(int i = 0; i < plots.Count; i++)
            {
                int multiplier = plots_prices_Multiplier[i];
                int cost = Factorial(multiplier) * default_plot_price;
                plots.First(p => p.id == i).costToUnlock = cost;
            }
        }
    }

    private int Factorial(int n)
    {
        int result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }



    private void PopulateBoundaryLinesFromChildren()
    {
        boundaryLines.Clear();

        var parsed = new Dictionary<int, LineRenderer>();
        var unparsed = new List<LineRenderer>();

        foreach (Transform child in transform)
        {
            LineRenderer lr = child.GetComponent<LineRenderer>();
            if (lr != null && child.name.StartsWith("Plot_"))
            {
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

            if (vertices.Count > 1 && Vector2.Distance(vertices[0], vertices[vertices.Count - 1]) < 0.01f)
            {
                vertices.RemoveAt(vertices.Count - 1);
            }

            Vector2 center = Vector2.zero;
            if (vertices.Count > 0)
            {
                foreach (var v in vertices) center += v;
                center /= vertices.Count;
            }

            int id = i;
            string name = lr.gameObject.name;
            if (name.StartsWith("Plot_"))
            {
                string[] parts = name.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedId)) id = parsedId;
            }

            Plot plot = new Plot(
                id,center, vertices,0,PlotState.Locked,
                GetBuildingSiteInPlot(lr.gameObject, "Mini"),
                GetBuildingSiteInPlot(lr.gameObject, "Big")
                );

            plots.Add(plot);
        }

        plots = plots.OrderBy(p => p.id).ToList();
    }
    /// <summary>
    /// it ignores big or small building text, just returns building site in the plot
    /// </summary>
    /// <param name="nameOfBuilding"></param>
    public BuildingSite GetBuildingSiteInPlot(GameObject ParentHolder,string nameOfBuilding)
    {
        var buildingSite = ParentHolder.gameObject.GetComponentInChildren<BuildingSite>(true);
       
        BuildingSite bigBuilding =
            ParentHolder.gameObject
            .GetComponentsInChildren<BuildingSite>(true)
            .FirstOrDefault(b =>
            b.gameObject.name.IndexOf("Big", System.StringComparison.OrdinalIgnoreCase) >= 0);
    
        if(bigBuilding != null)
            return bigBuilding;
        else
            return null;
    }

    private void InitializePlotStates(int unlockedPlotId)
    {
        foreach (var p in plots)
        {
            p.state = PlotState.Locked;
            p.isUnlocked = false;
        }

        var startPlot = plots.Find(p => p.id == unlockedPlotId);
        if (startPlot != null)
        {
            startPlot.state = PlotState.Unlocked;
            startPlot.isUnlocked = true;

            var available = new List<int>();
            foreach (var p in plots)
            {
                if (p.state == PlotState.Locked && AreNeighbors(startPlot, p))
                {
                    p.state = PlotState.AvailableToUnlock;
                    available.Add(p.id);
                }
            }

            if (debugNeighbors)
                Debug.Log("PlotController: initial unlocked " + startPlot.id + ", found " + available.Count + " neighbors: " + string.Join(", ", available));
        }
        else
        {
            Debug.LogWarning("PlotController: initialUnlockedPlotId " + unlockedPlotId + " not found among plots.");
        }
    }

    // NEW NEIGHBOR CHECK METHOD - ASCII ONLY
    private bool AreNeighbors(Plot a, Plot b)
    {
        if (a.vertices == null || b.vertices == null) return false;
        if (a.vertices.Count < 3 || b.vertices.Count < 3) return false;

        int sharedVertexCount = 0;
        List<(Vector2, Vector2, float)> closeVertices = new List<(Vector2, Vector2, float)>();

        foreach (Vector2 vertexA in a.vertices)
        {
            foreach (Vector2 vertexB in b.vertices)
            {
                float distance = Vector2.Distance(vertexA, vertexB);

                if (distance < neighborDistanceThreshold)
                {
                    sharedVertexCount++;
                    closeVertices.Add((vertexA, vertexB, distance));
                    break;
                }
            }
        }

        bool areNeighborsByVertices = sharedVertexCount >= minSharedVertices;

        float centerDistance = Vector2.Distance(a.center, b.center);

        float centerDistanceThreshold = mapRadius * 0.75f; // TRY ADJUSTING THIS FACTOR IF NEEDED
        bool areNeighborsByCenter = centerDistance < centerDistanceThreshold;

        bool result = areNeighborsByVertices && areNeighborsByCenter;

        if (debugNeighbors && result || debugAllPairs)
        {
            string resultStr = result ? "NEIGHBOR" : "NOT NEIGHBOR";
        }

        return result;
    }

    private LineRenderer GetLineRendererForPlot(Plot plot)
    {
        string expectedPrefix = "Plot_" + plot.id + "_";
        foreach (var lr in boundaryLines)
        {
            if (lr == null) continue;
            if (lr.gameObject.name.StartsWith(expectedPrefix)) return lr;
        }

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
                (point.x < (polygon[j].x - polygon[i].x) *
                (point.y - polygon[i].y) /
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

            var available = new List<int>();
            foreach (var p in plots)
            {
                if (p.state == PlotState.Locked && AreNeighbors(plot, p))
                {
                    p.state = PlotState.AvailableToUnlock;
                    available.Add(p.id);
                }
            }

            if (debugNeighbors)
                Debug.Log("PlotController: unlocked " + plot.id + "for $" + plot.costToUnlock + ", found " + available.Count + " new available neighbors: " + string.Join(", ", available));

            UpdatePlotVisuals(plot);
            ApplyMaterialsToPlots();
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

        Gizmos.color = Color.yellow;
        DrawCircleGizmo(mapCenter, mapRadius, 64);

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
