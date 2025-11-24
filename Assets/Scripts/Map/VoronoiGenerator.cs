using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;


public class VoronoiGenerator : MonoBehaviour
{
    [Header("Nastavení Mapy")]
    [SerializeField] private float mapRadius = 50f;
    [SerializeField] private int plotCount = 30;
    [SerializeField] private Vector3 mapCenter = Vector3.zero;

    [Header("Vizualizace")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.2f;
    [SerializeField] private float lineHeight = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool generateOnStart = true;

    private List<Plot> plots = new List<Plot>();
    private List<LineRenderer> boundaryLines = new List<LineRenderer>();

    void Start()
    {
        if (generateOnStart)
        {
            GeneratePlots();
        }
    }

    public void GeneratePlots()
    {
        ClearExistingPlots();

        // 1. Generuj náhodné body v kruhu
        List<Vector2> seedPoints = GenerateSeedPoints(plotCount, mapRadius);

        // 2. Vytvoø Voronoi buòky
        plots = CreateVoronoiCells(seedPoints);

        // 3. Vykresli hranice pomocí LineRenderer
        DrawPlotBoundaries();

        Debug.Log($"Vygenerováno {plots.Count} pozemkù");
    }

    private List<Vector2> GenerateSeedPoints(int count, float radius)
    {
        List<Vector2> points = new List<Vector2>();

        // Strategie: Poisson Disk Sampling pro rovnomìrné rozmístìní
        float minDistance = radius / Mathf.Sqrt(count) * 1.5f;
        int maxAttempts = 30;

        for (int i = 0; i < count; i++)
        {
            Vector2 point = Vector2.zero;
            bool validPoint = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Náhodný bod v kruhu
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float distance = Random.Range(0f, radius) * Mathf.Sqrt(Random.value);

                point = new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );

                // Kontrola vzdálenosti od ostatních bodù
                validPoint = true;
                foreach (Vector2 existingPoint in points)
                {
                    if (Vector2.Distance(point, existingPoint) < minDistance)
                    {
                        validPoint = false;
                        break;
                    }
                }

                if (validPoint) break;
            }

            if (validPoint || i == 0)
            {
                points.Add(point);
            }
        }

        return points;
    }

    private List<Plot> CreateVoronoiCells(List<Vector2> seedPoints)
    {
        List<Plot> newPlots = new List<Plot>();

        // Pro každý seed point vytvoø pozemek
        for (int i = 0; i < seedPoints.Count; i++)
        {
            Plot plot = new Plot
            {
                id = i,
                center = seedPoints[i],
                isUnlocked = (i == 0), // První pozemek odemèený
                vertices = CalculateVoronoiCell(seedPoints[i], seedPoints, mapRadius)
            };

            newPlots.Add(plot);
        }

        return newPlots;
    }

    private List<Vector2> CalculateVoronoiCell(Vector2 center, List<Vector2> allPoints, float radius)
    {
        List<Vector2> vertices = new List<Vector2>();

        // Poèet smìrù pro aproximaci (víc = pøesnìjší, ale pomalejší)
        int rayCount = 64;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (float)i / rayCount * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Najdi nejbližší hranici
            float minDistance = radius;

            foreach (Vector2 otherPoint in allPoints)
            {
                if (otherPoint == center) continue;

                // Perpendicular bisector mezi center a otherPoint
                Vector2 midPoint = (center + otherPoint) / 2f;
                Vector2 perpendicular = Vector2.Perpendicular(otherPoint - center).normalized;

                // Prùseèík paprsku s perpendicular bisector
                float distance = IntersectRayWithLine(center, direction, midPoint, perpendicular);

                if (distance > 0 && distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            // OmeŸ na kruh mapy
            Vector2 vertex = center + direction * minDistance;
            if (vertex.magnitude > radius)
            {
                vertex = vertex.normalized * radius;
            }

            vertices.Add(vertex);
        }

        // Zjednodušení polygonu (Douglas-Peucker)
        vertices = SimplifyPolygon(vertices, 0.5f);

        return vertices;
    }

    private float IntersectRayWithLine(Vector2 rayOrigin, Vector2 rayDir, Vector2 linePoint, Vector2 lineDir)
    {
        float cross = rayDir.x * lineDir.y - rayDir.y * lineDir.x;
        if (Mathf.Abs(cross) < 0.001f) return float.MaxValue;

        Vector2 diff = linePoint - rayOrigin;
        float t = (diff.x * lineDir.y - diff.y * lineDir.x) / cross;

        return t;
    }

    private List<Vector2> SimplifyPolygon(List<Vector2> points, float tolerance)
    {
        if (points.Count < 3) return points;

        List<Vector2> simplified = new List<Vector2>();
        simplified.Add(points[0]);

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 prev = simplified[simplified.Count - 1];
            Vector2 current = points[i];
            Vector2 next = points[i + 1];

            float distance = PointToLineDistance(current, prev, next);

            if (distance > tolerance)
            {
                simplified.Add(current);
            }
        }

        simplified.Add(points[points.Count - 1]);

        return simplified;
    }

    private float PointToLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float len = line.magnitude;
        if (len < 0.001f) return Vector2.Distance(point, lineStart);

        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(point - lineStart, line) / (len * len)));
        Vector2 projection = lineStart + t * line;

        return Vector2.Distance(point, projection);
    }

    private void DrawPlotBoundaries()
    {
        foreach (Plot plot in plots)
        {
            GameObject lineObj = new GameObject($"Plot_{plot.id}_Boundary");
            lineObj.transform.parent = transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = plot.vertices.Count + 1;
            lr.loop = true;
            lr.useWorldSpace = true; // GLOBAL souøadnice!

            // Pøevod 2D na 3D GLOBAL souøadnice
            for (int i = 0; i < plot.vertices.Count; i++)
            {
                Vector3 worldPos = new Vector3(
                    mapCenter.x + plot.vertices[i].x,
                    mapCenter.y + lineHeight,
                    mapCenter.z + plot.vertices[i].y
                );
                lr.SetPosition(i, worldPos);
            }

            // Uzavøi polygon
            Vector3 firstPos = new Vector3(
                mapCenter.x + plot.vertices[0].x,
                mapCenter.y + lineHeight,
                mapCenter.z + plot.vertices[0].y
            );
            lr.SetPosition(plot.vertices.Count, firstPos);

            // Barva podle stavu
            lr.startColor = plot.isUnlocked ? UnityEngine.Color.green : UnityEngine.Color.gray;
            lr.endColor = plot.isUnlocked ? UnityEngine.Color.green : UnityEngine.Color.gray;

            boundaryLines.Add(lr);
        }
    }

    private void ClearExistingPlots()
    {
        foreach (LineRenderer lr in boundaryLines)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        boundaryLines.Clear();
        plots.Clear();
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
            lr.startColor = plot.isUnlocked ? Color.green : UnityEngine.Color.gray;
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

[System.Serializable]
public class Plot
{
    public int id;
    public Vector2 center;
    public List<Vector2> vertices;
    public bool isUnlocked;
    public BuildingType allowedBuilding;
}

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    Park
}

public class PlotInputHandler : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private VoronoiGenerator voronoiGenerator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera minimapCamera;

    [Header("Nastavení")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool useMinimapInput = true;

    private Plot selectedPlot;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandlePlotClick();
        }
    }

    private void HandlePlotClick()
    {
        // Kontrola jestli neklikáme na UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Raycast ze správné kamery
        Camera activeCamera = GetActiveCamera();
        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Plot clickedPlot = voronoiGenerator.GetPlotAtPosition(hit.point);

            if (clickedPlot != null)
            {
                OnPlotClicked(clickedPlot);
            }
        }
    }

    private Camera GetActiveCamera()
    {
        // Zjisti jestli je kurzor nad minimapou
        if (useMinimapInput && minimapCamera != null)
        {
            Rect minimapRect = minimapCamera.pixelRect;
            if (minimapRect.Contains(Input.mousePosition))
            {
                return minimapCamera;
            }
        }

        return mainCamera;
    }

    private void OnPlotClicked(Plot plot)
    {
        selectedPlot = plot;

        Debug.Log($"Kliknuto na pozemek #{plot.id}");

        if (!plot.isUnlocked)
        {
            // Zobraz UI pro odemèení
            PlotUIManager.Instance?.ShowUnlockDialog(plot);
        }
        else
        {
            // Zobraz UI pro stavbu
            PlotUIManager.Instance?.ShowBuildingMenu(plot);
        }
    }

    public Plot GetSelectedPlot() => selectedPlot;
}
public class PlotUIManager : MonoBehaviour
{
    public static PlotUIManager Instance { get; private set; }

    [Header("UI Panely")]
    [SerializeField] private GameObject unlockPanel;
    [SerializeField] private GameObject buildingPanel;

    [Header("Unlock Panel")]
    [SerializeField] private TextMeshProUGUI unlockTitleText;
    [SerializeField] private TextMeshProUGUI unlockPriceText;
    [SerializeField] private Button unlockButton;
    [SerializeField] private Button cancelButton;

    [Header("Building Panel")]
    [SerializeField] private TextMeshProUGUI buildingTitleText;
    [SerializeField] private Transform buildingButtonContainer;
    [SerializeField] private GameObject buildingButtonPrefab;

    [Header("Game Manager")]
    [SerializeField] private VoronoiGenerator voronoiGenerator;

    private Plot currentPlot;
    private int playerMoney = 10000;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        unlockPanel.SetActive(false);
        buildingPanel.SetActive(false);

        unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    public void ShowUnlockDialog(Plot plot)
    {
        currentPlot = plot;

        int price = CalculateUnlockPrice(plot);

        unlockTitleText.text = $"Pozemek #{plot.id}";
        unlockPriceText.text = $"Cena: ${price}";

        unlockButton.interactable = (playerMoney >= price);

        unlockPanel.SetActive(true);
        buildingPanel.SetActive(false);
    }

    public void ShowBuildingMenu(Plot plot)
    {
        currentPlot = plot;

        buildingTitleText.text = $"Stavba na pozemku #{plot.id}";

        // Vygeneruj tlaèítka pro budovy
        foreach (Transform child in buildingButtonContainer)
        {
            Destroy(child.gameObject);
        }

        string[] buildings = { "Dùm", "Obchod", "Továrna", "Park" };
        foreach (string building in buildings)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingButtonContainer);
            Button btn = buttonObj.GetComponent<Button>();
            TextMeshProUGUI btnText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            btnText.text = building;
            btn.onClick.AddListener(() => OnBuildingSelected(building));
        }

        unlockPanel.SetActive(false);
        buildingPanel.SetActive(true);
    }

    private void OnUnlockButtonClicked()
    {
        int price = CalculateUnlockPrice(currentPlot);

        if (playerMoney >= price)
        {
            playerMoney -= price;
            voronoiGenerator.UnlockPlot(currentPlot.id);

            Debug.Log($"Pozemek #{currentPlot.id} odemèen!");

            unlockPanel.SetActive(false);
        }
    }

    private void OnCancelButtonClicked()
    {
        unlockPanel.SetActive(false);
        buildingPanel.SetActive(false);
    }

    private void OnBuildingSelected(string buildingName)
    {
        Debug.Log($"Postaveno: {buildingName} na pozemku #{currentPlot.id}");

        // Zde pøidej logiku stavby

        buildingPanel.SetActive(false);
    }

    private int CalculateUnlockPrice(Plot plot)
    {
        // Cena roste s ID pozemku
        return 1000 + (plot.id * 500);
    }
}