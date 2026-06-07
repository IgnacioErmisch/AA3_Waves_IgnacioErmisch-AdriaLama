using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Buoy : MonoBehaviour
{
    [Header("Propiedades físicas de la boya")]
    [Tooltip("Radio de la boya (metros) - para calcular el área de base")]
    public float radius = 0.3f;

    [Tooltip("Altura total de la boya (metros)")]
    public float height = 0.6f;

    [Tooltip("Densidad del fluido ρ (kg/m³) — agua = 1000")]
    public float fluidDensity = 1000f;

    [Header("Amortiguamiento")]
    [Tooltip("Amortiguamiento lineal cuando está en el agua")]
    public float waterDrag = 3f;

    [Tooltip("Amortiguamiento angular en el agua")]
    public float waterAngularDrag = 1f;

    // --- componentes ---
    private Rigidbody rb;
    private float airDrag;
    private float airAngularDrag;

    // Constante gravitacional (usamos Physics.gravity.magnitude para coherencia)
    private float g;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        airDrag = rb.linearDamping;
        airAngularDrag = rb.angularDamping;
        g = Mathf.Abs(Physics.gravity.y);
    }

    void FixedUpdate()
    {
        if (WaveManager.Instance == null) return;

        // Altura del agua en la posición horizontal de la boya
        float waterHeight = WaveManager.Instance.GetWaterHeight(transform.position.x,transform.position.z);

        // Parte inferior de la boya
        float bottomY = transform.position.y - height * 0.5f;
        // Parte superior de la boya
        float topY = transform.position.y + height * 0.5f;

        // Profundidad sumergida (cuánto de la boya está bajo el agua)
        float submergedDepth = Mathf.Clamp(waterHeight - bottomY, 0f, height);

        if (submergedDepth > 0f)
        {
            // Volumen desplazado: V = π·r²·h_sumergida
            float volumeDisplaced = Mathf.PI * radius * radius * submergedDepth;

            // Fuerza de empuje (flotabilidad): F = ρ·g·V
            float buoyancyForce = fluidDensity * g * volumeDisplaced;

            // Aplicamos la fuerza hacia arriba en el centro de masa
            rb.AddForceAtPosition(
                Vector3.up * buoyancyForce,
                new Vector3(transform.position.x, waterHeight, transform.position.z),
                ForceMode.Force
            );

            // Amortiguamiento por estar en el agua
            rb.linearDamping = waterDrag;
            rb.angularDamping = waterAngularDrag;
        }
        else
        {
            // Fuera del agua: drag normal
            rb.linearDamping = airDrag;
            rb.angularDamping = airAngularDrag;
        }

        // Debug visual: línea hasta la superficie del agua
        Debug.DrawLine(
            transform.position,
            new Vector3(transform.position.x, waterHeight, transform.position.z),
            submergedDepth > 0f ? Color.cyan : Color.red
        );
    }

    // Dibuja el gizmo de la boya en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}