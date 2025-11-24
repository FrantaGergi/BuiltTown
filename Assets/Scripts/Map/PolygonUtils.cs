using System.Collections.Generic;
using UnityEngine;

public static class PolygonUtils
{
    // Winding number / raycast approach for point in polygon (2D XZ)
    public static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        int j = poly.Length - 1;
        for (int i = 0; i < poly.Length; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                inside = !inside;
        }
        return inside;
    }

    // Create a simple mesh from polygon (fan triangulation from centroid)
    public static Mesh PolygonToMesh(Vector2[] poly)
    {
        Vector3[] verts = new Vector3[poly.Length];
        Vector3 centroid = Vector3.zero;
        for (int i = 0; i < poly.Length; i++)
        {
            verts[i] = new Vector3(poly[i].x, 0f, poly[i].y);
            centroid += verts[i];
        }
        centroid /= poly.Length;

        List<int> tris = new List<int>();
        for (int i = 0; i < poly.Length; i++)
        {
            int next = (i + 1) % poly.Length;
            tris.Add(i);
            tris.Add(next);
            tris.Add(poly.Length); // centroid index
        }

        Vector3[] finalVerts = new Vector3[poly.Length + 1];
        for (int i = 0; i < poly.Length; i++) finalVerts[i] = verts[i];
        finalVerts[poly.Length] = centroid;

        Mesh m = new Mesh();
        m.vertices = finalVerts;
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
