using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WireGenerator : MonoBehaviour {

    [Range(0.05f, 5f)]
    public float spacing = 1;
    
    [Tooltip("The actual thickness/radius of the 3D wire in world units")]
    public float wireRadius = 0.2f;

    [Range(3, 24)]
    [Tooltip("How round the wire is. Higher numbers mean a smoother tube.")]
    public int radialSegments = 8;
    
    public bool autoUpdate;

    [Button("Generate")]
    public void UpdateWire()
    {
        PointPath path = GetComponent<PathCreator>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoints(spacing);

        for (int i = 0; i < points.Length; i++)
        {
            if (!IsFinite(points[i]))
            {
                Debug.LogWarning($"WireGenerator: sampled point {i} was not finite ({points[i]}) - skipping mesh generation. This usually means two anchor points in the path are overlapping.", this);
                return;
            }
        }

        print("Points Path: " + points.Length);
        GetComponent<MeshFilter>().mesh = CreateWireMesh(points, path.IsClosed);
    }

    static bool IsFinite(Vector3 v)
    {
        return !float.IsNaN(v.x) && !float.IsInfinity(v.x)
            && !float.IsNaN(v.y) && !float.IsInfinity(v.y)
            && !float.IsNaN(v.z) && !float.IsInfinity(v.z);
    }
    
    Mesh CreateWireMesh(Vector3[] points, bool isClosed)
    {
        if (points == null || points.Length < 2) return new Mesh();

        int totalRingVerts = radialSegments + 1; 
        int numVerts = points.Length * totalRingVerts;
        Vector3[] verts = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uvs = new Vector2[numVerts];

        int numSegmentSteps = isClosed ? points.Length : points.Length - 1;
        int numTris = numSegmentSteps * radialSegments * 2;
        int[] tris = new int[numTris * 3];

        int vertIndex = 0;
        int triIndex = 0;

        Vector3 lastUpReference = Vector3.up;

        for (int i = 0; i < points.Length; i++)
        {
            // 1. Calculate forward vector in WORLD SPACE to ensure accurate path-following angles
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            if (i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }
            forward.Normalize();

            // 2. Parallel Transport Frame tracking inside world coordinates
            Vector3 right = Vector3.Cross(lastUpReference, forward).normalized;
            if (right.sqrMagnitude < 0.001f)
            {
                Vector3 fallbackAxis = Mathf.Abs(forward.y) > 0.9f ? Vector3.forward : Vector3.up;
                right = Vector3.Cross(fallbackAxis, forward).normalized;
            }
            Vector3 up = Vector3.Cross(forward, right).normalized;
            lastUpReference = up;

            float v = i / (float)(points.Length - 1);

            for (int r = 0; r <= radialSegments; r++)
            {
                float angle = (r / (float)radialSegments) * Mathf.PI * 2f;
                Vector3 unitCirclePos = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                
                // 3. Construct the vertex completely in WORLD SPACE first
                Vector3 worldVertPos = points[i] + unitCirclePos * wireRadius;

                // 4. FIX: Transform the completed point from World Space down into Local Space
                // This ensures the path twists, offsets, and scale are handled uniformly together!
                verts[vertIndex] = transform.InverseTransformPoint(worldVertPos);
                
                // Convert the direction vector to local space for flawless lighting normals
                normals[vertIndex] = transform.InverseTransformDirection(unitCirclePos).normalized;

                float u = r / (float)radialSegments;
                uvs[vertIndex] = new Vector2(u, v);

                if (i < points.Length - 1 || isClosed)
                {
                    if (r < radialSegments)
                    {
                        int currentRingVert = vertIndex;
                        int nextRingVert = (vertIndex + totalRingVerts) % numVerts;
                        int currentRingNextVert = vertIndex + 1;
                        int nextRingNextVert = (vertIndex + 1 + totalRingVerts) % numVerts;

                        tris[triIndex] = currentRingVert;
                        tris[triIndex + 1] = currentRingNextVert;
                        tris[triIndex + 2] = nextRingVert;

                        tris[triIndex + 3] = currentRingNextVert;
                        tris[triIndex + 4] = nextRingNextVert;
                        tris[triIndex + 5] = nextRingVert;

                        triIndex += 6;
                    }
                }
                vertIndex++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "ProceduralWire";
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = normals;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }
}