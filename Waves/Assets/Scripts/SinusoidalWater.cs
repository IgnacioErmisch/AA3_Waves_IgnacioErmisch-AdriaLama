using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SinusoidalWater : MonoBehaviour
{
    [Header("Resolución de la malla")]
    [Tooltip("Número de divisiones en X")]
    public int resolutionX = 80;
    [Tooltip("Número de divisiones en Z")]
    public int resolutionZ = 80;
    [Tooltip("Tamaño total del plano de agua (metros)")]
    public float size = 20f;

    // --- referencia interna ---
    private Mesh mesh;
    private Vector3[] baseVertices;   // posiciones originales (plano plano)
    private Vector3[] vertices;       // posiciones modificadas cada frame

    void Start()
    {
        GenerateMesh();
    }

    void Update()
    {
        if (WaveManager.Instance == null || WaveManager.Instance.useGerstner) return;
        DeformMesh();
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "SinusoidalWater";
        GetComponent<MeshFilter>().mesh = mesh;

        int vCountX = resolutionX + 1;
        int vCountZ = resolutionZ + 1;
        baseVertices = new Vector3[vCountX * vCountZ];
        Vector2[] uvs = new Vector2[baseVertices.Length];

        for (int z = 0; z < vCountZ; z++)
        {
            for (int x = 0; x < vCountX; x++)
            {
                int i = z * vCountX + x;
                float fx = (float)x / resolutionX * size - size * 0.5f;
                float fz = (float)z / resolutionZ * size - size * 0.5f;
                baseVertices[i] = new Vector3(fx, 0f, fz);
                uvs[i] = new Vector2((float)x / resolutionX, (float)z / resolutionZ);
            }
        }

        // Triángulos
        int[] triangles = new int[resolutionX * resolutionZ * 6];
        int ti = 0;
        for (int z = 0; z < resolutionZ; z++)
        {
            for (int x = 0; x < resolutionX; x++)
            {
                int bl = z * vCountX + x;
                int br = bl + 1;
                int tl = bl + vCountX;
                int tr = tl + 1;
                triangles[ti++] = bl; triangles[ti++] = tl; triangles[ti++] = br;
                triangles[ti++] = br; triangles[ti++] = tl; triangles[ti++] = tr;
            }
        }

        vertices = (Vector3[])baseVertices.Clone();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }
    void DeformMesh()
    {
        float t = Time.time;
        WaveParams[] waves = WaveManager.Instance.waves;

        for (int i = 0; i < vertices.Length; i++)
        {
            float x = baseVertices[i].x + transform.position.x;
            float z = baseVertices[i].z + transform.position.z;
            float y = 0f;

            foreach (var w in waves)
            {
                Vector2 dir = w.direction.normalized;
                float k = 2f * Mathf.PI / w.wavelength;
                // Proyección sobre la dirección de la ola
                float xProj = dir.x * x + dir.y * z;
                // Fórmula sinusoidal: A·sin(k·(xProj - v·t) + φ)
                y += w.amplitude * Mathf.Sin(k * (xProj - w.speed * t) + w.phase);
            }

            // Solo modificamos Y; X y Z se quedan en la posición original
            vertices[i] = new Vector3(baseVertices[i].x, y, baseVertices[i].z);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}