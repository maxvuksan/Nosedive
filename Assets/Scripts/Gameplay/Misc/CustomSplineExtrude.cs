using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public sealed class CustomSplineExtrude : MonoBehaviour
{
    [Header("Geometry")] [Min(0.001f)] [SerializeField]
    private float radius = 0.1f;

    [Min(3)] [SerializeField] private int numberOfSides = 12;

    [Min(0.01f)] [SerializeField] private float density = 2f;

    [Header("UV")] [Tooltip("Number of texture repeats along the entire spline.")] [Min(0.001f)] [SerializeField]
    private float textureTiling = 1f;

    [Header("Generated")] [HideInInspector] [SerializeField]
    private Mesh generatedMesh;

    private SplineContainer splineContainer;
    private MeshFilter meshFilter;


    [Button("Generate")]
    public void GenerateSpline()
    {
        CacheComponents();

        Spline spline = splineContainer.Spline;

        if (spline == null)
            return;

        float length = spline.GetLength();

        if (length <= Mathf.Epsilon)
            return;

        int sides = Mathf.Max(3, numberOfSides);

        // Number of rings along the spline.
        int ringCount = Mathf.Max(
            2,
            Mathf.CeilToInt(length * density) + 1
        );

        // +1 gives us two vertices at the seam:
        //
        // U = 0
        // U = 1
        //
        // They occupy the same position but have
        // different UV coordinates.
        int verticesPerRing = sides + 1;

        int vertexCount = ringCount * verticesPerRing;
        int triangleCount = (ringCount - 1) * sides * 6;

        var vertices = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[triangleCount];

        float angleStep = Mathf.PI * 2f / sides;

        GenerateVertices(
            spline,
            length,
            ringCount,
            sides,
            vertices,
            normals,
            uvs,
            angleStep
        );

        GenerateTriangles(
            ringCount,
            sides,
            verticesPerRing,
            triangles
        );

        Mesh mesh = CreateMesh(
            vertices,
            normals,
            uvs,
            triangles
        );

        ReplaceGeneratedMesh(mesh);
    }


    private void GenerateVertices(
        Spline spline,
        float length,
        int ringCount,
        int sides,
        Vector3[] vertices,
        Vector3[] normals,
        Vector2[] uvs,
        float angleStep)
    {
        for (int ring = 0; ring < ringCount; ring++)
        {
            float normalizedDistance = ring / (float)(ringCount - 1);
            float distance = normalizedDistance * length;
            float t = distance / length;

            Vector3 position = spline.EvaluatePosition(t);
            Vector3 tangent = spline.EvaluateTangent(t);

            tangent.Normalize();

            Quaternion rotation = GetFrame(tangent);

            int ringStart = ring * (sides + 1);

            // V travels from 0 -> textureTiling
            // over the entire spline.
            float v = normalizedDistance * textureTiling;

            for (int side = 0; side <= sides; side++)
            {
                int index = ringStart + side;

                float u = side / (float)sides;

                float angle = side * angleStep;

                Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                vertices[index] = position + rotation * (radial * radius);
                normals[index] = rotation * radial;
                uvs[index] = new Vector2(u, v);
            }
        }
    }


    private static void GenerateTriangles(
        int ringCount,
        int sides,
        int verticesPerRing,
        int[] triangles)
    {
        int index = 0;

        for (int ring = 0; ring < ringCount - 1; ring++)
        {
            int current = ring * verticesPerRing;
            int next = (ring + 1) * verticesPerRing;

            for (int side = 0; side < sides; side++)
            {
                int a = current + side;
                int b = current + side + 1;
                int c = next + side;
                int d = next + side + 1;

                // Outward-facing winding.
                triangles[index++] = a;
                triangles[index++] = b;
                triangles[index++] = c;

                triangles[index++] = b;
                triangles[index++] = d;
                triangles[index++] = c;
            }
        }
    }


    private static Mesh CreateMesh(
        Vector3[] vertices,
        Vector3[] normals,
        Vector2[] uvs,
        int[] triangles)
    {
        var mesh = new Mesh
        {
            name = "CustomSplineExtrude"
        };

        if (vertices.Length > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        return mesh;
    }


    private Quaternion GetFrame(Vector3 tangent)
    {
        if (tangent.sqrMagnitude < 0.000001f)
            return Quaternion.identity;

        tangent.Normalize();

        // Keep the frame stable when the tangent
        // approaches world up/down.
        Vector3 up = Vector3.up;

        if (Mathf.Abs(Vector3.Dot(tangent, up)) > 0.99f)
            up = Vector3.forward;

        return Quaternion.LookRotation(tangent, up);
    }


    private void CacheComponents()
    {
        if (splineContainer == null)
        {
            splineContainer = GetComponent<SplineContainer>();
        }

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
    }


    private void ReplaceGeneratedMesh(Mesh mesh)
    {
        Mesh oldMesh = generatedMesh;

        generatedMesh = mesh;

        meshFilter.sharedMesh = generatedMesh;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (oldMesh != null)
                DestroyImmediate(oldMesh);
        }
        else
        {
            if (oldMesh != null)
                Destroy(oldMesh);
        }
#else
        if (oldMesh != null)
            Destroy(oldMesh);
#endif
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        radius = Mathf.Max(0.001f, radius);
        numberOfSides = Mathf.Max(3, numberOfSides);
        density = Mathf.Max(0.01f, density);
        textureTiling = Mathf.Max(0.001f, textureTiling);
    }


    private void OnDestroy()
    {
        if (generatedMesh == null)
            return;

        if (Application.isPlaying)
            Destroy(generatedMesh);
        else
            DestroyImmediate(generatedMesh);
    }

#endif
}