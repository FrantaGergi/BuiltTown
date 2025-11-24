using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapSectorGenerator : MonoBehaviour
{
    [Header("Map settings")]
    public int sectorCount = 50;
    public float mapRadius = 20f; // v unity jednotkách
    public int verticesPerSector = 12; // kolik bodù po obvodu každého sektoru
    public float noiseScale = 1.5f;
    public float radiusVariation = 0.35f; // 0..1 (0 = plný kruh, 1 = silné výkyvy)
    public int seed = 12345; // deterministické

    [Header("Prefabs / styling")]
    public Material sectorMaterial;
    public Material lineMaterial;
    public float lineWidth = 0.08f;
    public Color lockedColor = Color.grey;
    public Color unlockedColor = Color.green;
    public Color builtColor = Color.cyan;

    private List<LandSector> sectors = new();

    private void Start()
    {
        // Optionally generate on start
          Generate();
    }

    public void Generate()
    {
        ClearExisting();

        // deterministický pseudo-random pøes seed
        System.Random prng = new System.Random(seed);
        float goldenAngle = 137.5077640500378546463487f; // degrees - dobré rozložení
        // vytvoøíme centra (rovnomìrné, deterministické)
        List<Vector2> centers = new List<Vector2>();
        for (int i = 0; i < sectorCount; i++)
        {
            // use golden angle spiral to distribute centers inside circle
            float r = Mathf.Sqrt((i + 0.5f) / sectorCount) * mapRadius * 0.8f;
            float ang = (i * goldenAngle) % 360f;
            float rad = ang * Mathf.Deg2Rad;
            centers.Add(new Vector2(Mathf.Cos(rad) * r, Mathf.Sin(rad) * r));
        }

        // For each center create a polygon by sampling angle slice around center but clipped to circle rim.
        for (int s = 0; s < sectorCount; s++)
        {
            Vector2 c = centers[s];
            // Build polygon points around the circumference relative to map center
            List<Vector2> poly = new List<Vector2>();

            // We'll sample angles around map center, but each sector will only occupy portion around its center angle
            // Determine baseAngle from center position
            float baseAngle = Mathf.Atan2(c.y, c.x) * Mathf.Rad2Deg;
            // sector angular width (deterministic, can vary slightly by index)
            float baseWidth = 360f / sectorCount;
            float widthVariation = (float)(prng.NextDouble() * 0.6 - 0.3f); // small deterministic via prng
            float sectorWidth = baseWidth * (1f + widthVariation * 0.3f);

            // sample verticesPerSector points between baseAngle - halfWidth .. + halfWidth
            float startA = baseAngle - sectorWidth * 0.5f;
            float step = sectorWidth / verticesPerSector;

            for (int v = 0; v < verticesPerSector; v++)
            {
                float ang = startA + v * step;
                float rad = ang * Mathf.Deg2Rad;

                // distance to map edge in this angle
                float maxR = mapRadius;

                // compute deterministic noise-based radius modulation
                float noiseX = Mathf.Cos(rad) * noiseScale + s * 0.13f + seed * 0.0001f;
                float noiseY = Mathf.Sin(rad) * noiseScale + s * 0.11f - seed * 0.0002f;
                float n = Mathf.PerlinNoise(noiseX, noiseY); // 0..1 deterministic
                float r = Mathf.Lerp(maxR * 0.3f, maxR, 0.9f * (1f - radiusVariation * n) + radiusVariation * n);

                // shrink radius a bit toward center to create islands that don't always touch edge
                // combine with distance from map center to sector center so tiles have different sizes
                float centerDist = c.magnitude;
                float centerFactor = Mathf.Lerp(1.0f, 0.6f, centerDist / mapRadius);
                r *= centerFactor;

                // produce final point in world XZ (map origin is this.transform.position)
                Vector2 localPoint = new Vector2(Mathf.Cos(rad) * r, Mathf.Sin(rad) * r);
                poly.Add(localPoint);
            }

            // ensure polygon is clockwise and unique
            Vector2[] polyArr = poly.ToArray();

            // create GameObject
            GameObject go = new GameObject($"Sector_{s}");
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            LandSector ls = go.AddComponent<LandSector>();
            ls.id = s;
            ls.polygon = polyArr;

            // Mesh
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshCollider mc = go.AddComponent<MeshCollider>();

            Mesh mesh = PolygonUtils.PolygonToMesh(polyArr);
            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            mr.sharedMaterial = sectorMaterial != null ? new Material(sectorMaterial) : new Material(Shader.Find("Standard"));

            // LineRenderer
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.loop = true;
            lr.positionCount = polyArr.Length;
            lr.useWorldSpace = false;
            lr.widthMultiplier = lineWidth;
            if (lineMaterial != null) lr.material = lineMaterial;
            else lr.material = new Material(Shader.Find("Sprites/Default"));
            for (int i = 0; i < polyArr.Length; i++)
            {
                lr.SetPosition(i, new Vector3(polyArr[i].x, 0.05f, polyArr[i].y));
            }

            // assign refs
            ls.meshFilter = mf;
            ls.meshRenderer = mr;
            ls.meshCollider = mc;
            ls.lineRenderer = lr;

            // initial color locked
            ls.SetColor(lockedColor);

            sectors.Add(ls);
        }

        Debug.Log($"Generated {sectors.Count} sectors (seed={seed}).");
    }

    public void ClearExisting()
    {
        // destroy children
        List<GameObject> toDestroy = new List<GameObject>();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            toDestroy.Add(c.gameObject);
        }
        foreach (var g in toDestroy)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.GameObjectUtility.SetParentAndAlign(g, null);
#endif
            DestroyImmediate(g);
        }
        sectors.Clear();
    }

    // Find sector by world point
    public LandSector GetSectorAtWorldPoint(Vector3 worldPoint)
    {
        foreach (var s in sectors)
        {
            if (s.ContainsPoint(worldPoint)) return s;
        }
        return null;
    }

    // example helper to unlock by id
    public void UnlockSector(int id)
    {
        var s = sectors.Find(x => x.id == id);
        if (s != null)
        {
            s.state = SectorState.Unlocked;
            s.SetColor(unlockedColor);
        }
    }

    // Editor utility
    private void OnValidate()
    {
        // auto-generate in editor when properties change? optional.
    }
}
