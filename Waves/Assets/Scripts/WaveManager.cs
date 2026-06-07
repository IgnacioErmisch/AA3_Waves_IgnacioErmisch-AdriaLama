using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Tipo de ola activo")]
    public bool useGerstner = true;   // Toggle: true = Gerstner, false = Sinusoidal

    [Header("Parámetros compartidos de ola")]
    public WaveParams[] waves = new WaveParams[]
    {
        new WaveParams { amplitude = 0.5f, wavelength = 8f, direction = new Vector2(1f, 0f), speed = 1.5f, phase = 0f },
        new WaveParams { amplitude = 0.3f, wavelength = 5f, direction = new Vector2(0.8f, 0.6f), speed = 1.2f, phase = 1.2f },
        new WaveParams { amplitude = 0.2f, wavelength = 3f, direction = new Vector2(0f, 1f), speed = 1.0f, phase = 2.4f }
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float GetWaterHeight(float x, float z)
    {
        float height = 0f;
        float t = Time.time;

        foreach (var w in waves)
        {
            Vector2 dir = w.direction.normalized;
            float k = 2f * Mathf.PI / w.wavelength;   // número de onda
            float omega = w.speed * k;                 // frecuencia angular

            if (useGerstner)
            {
                // Gerstner: componente Y  = A * cos(k*(D·x) - omega*t + phi)
                float dot = dir.x * x + dir.y * z;
                height += w.amplitude * Mathf.Cos(k * dot - omega * t + w.phase);
            }
            else
            {
                // Sinusoidal: Y = A * sin(2π/L * (x - v*t) + phi)
                // Proyectamos x sobre la dirección de la ola
                float xProj = dir.x * x + dir.y * z;
                height += w.amplitude * Mathf.Sin(k * (xProj - w.speed * t) + w.phase);
            }
        }
        return height;
    }
}
[System.Serializable]
public class WaveParams
{
    [Tooltip("Amplitud A (metros)")]
    public float amplitude = 0.5f;

    [Tooltip("Longitud de onda L (metros)")]
    public float wavelength = 8f;

    [Tooltip("Dirección D (se normaliza automáticamente)")]
    public Vector2 direction = new Vector2(1f, 0f);

    [Tooltip("Velocidad de fase v (m/s)")]
    public float speed = 1.5f;

    [Tooltip("Fase inicial φ (radianes)")]
    public float phase = 0f;
}