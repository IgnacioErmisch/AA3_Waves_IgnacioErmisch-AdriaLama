using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GerstnerWater : MonoBehaviour
{
    [Header("Resolución de la malla")]
    public int resolutionX = 80;
    public int resolutionZ = 80;
    public float size = 20f;

    [Header("Parámetro de cresta (Steepness Q)")]
    [Range(0f, 1f)]
    [Tooltip("0 = sinusoidal, 1 = Gerstner puro (crestas más puntiagudas)")]
    public float steepness = 0.5f;

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] vertices;

    void Start()
    {
        GenerateMesh();
    }

    void Update()
    {
        if (WaveManager.Instance == null || !WaveManager.Instance.useGerstner) return;
        DeformMesh();
    }

    
    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "GerstnerWater";
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

        int[] triangles = new int[resolutionX * resolutionZ * 6];
        int ti = 0;
        for (int z = 0; z < resolutionZ; z++)
        {
            for (int x = 0; x < resolutionX; x++)
            {
                int bl = z * (resolutionX + 1) + x;
                int br = bl + 1;
                int tl = bl + (resolutionX + 1);
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
            // Posición original en espacio mundo
            float x0 = baseVertices[i].x + transform.position.x;
            float z0 = baseVertices[i].z + transform.position.z;

            float dx = 0f, dy = 0f, dz = 0f;

            foreach (var w in waves)
            {
                Vector2 dir = w.direction.normalized;
                float k = 2f * Mathf.PI / w.wavelength;   // número de onda
                float omega = w.speed * k;                 // frecuencia angular ω

                // Fase del vértice: φi = k*(D·P0) - ω*t + fase_inicial
                float dot = dir.x * x0 + dir.y * z0;
                float phase = k * dot - omega * t + w.phase;

                float sinP = Mathf.Sin(phase);
                float cosP = Mathf.Cos(phase);

                // Q controla la "agudeza" de la cresta
                float Q = steepness;

                // Desplazamiento horizontal (X y Z)
                dx += -Q * w.amplitude * dir.x * sinP;
                dz += -Q * w.amplitude * dir.y * sinP;

                // Desplazamiento vertical (Y)
                dy += w.amplitude * cosP;
            }

            // Aplicamos el desplazamiento sobre la posición base LOCAL
            vertices[i] = new Vector3(
                baseVertices[i].x + dx,
                dy,
                baseVertices[i].z + dz
            );
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}