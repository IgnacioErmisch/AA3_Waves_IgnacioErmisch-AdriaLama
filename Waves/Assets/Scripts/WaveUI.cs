using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WaveUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Toggle waveToggle;           // Toggle que cambia el tipo de ola
    public TextMeshProUGUI labelText;   // Etiqueta que muestra el modo activo
    public TextMeshProUGUI infoText;    // Texto con parámetros en tiempo real

    [Header("Referencias de malla")]
    public GerstnerWater gerstnerWater;
    public SinusoidalWater sinusoidalWater;

    void Start()
    {
        if (waveToggle != null)
            waveToggle.onValueChanged.AddListener(OnToggleChanged);

        UpdateUI();
    }

    void Update()
    {
        // Actualiza el texto informativo cada frame
        if (infoText != null && WaveManager.Instance != null)
        {
            string mode = WaveManager.Instance.useGerstner ? "GERSTNER" : "SINUSOIDAL";
            int waveCount = WaveManager.Instance.waves.Length;
            infoText.text = $"Modo: {mode}\nNúmero de olas: {waveCount}\nTiempo: {Time.time:F1}s";
        }
    }

    // Llamado cuando el usuario mueve el Toggle
    void OnToggleChanged(bool isOn)
    {
        if (WaveManager.Instance == null) return;
        WaveManager.Instance.useGerstner = isOn;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (WaveManager.Instance == null) return;

        bool gerstner = WaveManager.Instance.useGerstner;

        if (labelText != null)
            labelText.text = gerstner ? "Gerstner" : "Sinusoidal";

        // Activa/desactiva los GameObjects de agua correspondientes
        if (gerstnerWater != null)
            gerstnerWater.gameObject.SetActive(gerstner);
        if (sinusoidalWater != null)
            sinusoidalWater.gameObject.SetActive(!gerstner);
    }

    // También puede llamarse desde un botón en el inspector
    public void SwitchToGerstner() { if (waveToggle) waveToggle.isOn = true; }
    public void SwitchToSinusoidal() { if (waveToggle) waveToggle.isOn = false; }
}