using UnityEngine;
using UnityEngine.UI;

public class PlantLampController : MonoBehaviour
{
    [SerializeField] Renderer targetRenderer; // e.g. PlantLamp_Head_LightSource_Bulb_Glass
    [SerializeField] Material offMaterial;    // e.g. LightBulbOff
    [SerializeField] Material onMaterial;     // emissive material
    [SerializeField] bool startOn = false;    // must match the toggle's "Is On"

    [Header("Emission intensity")]
    [SerializeField] Slider intensitySlider;                     // its min/max define the range
    [SerializeField] string emissionProperty = "_EmissionColor"; // URP Lit; custom shaders may use another name
    [ColorUsage(false, false)]
    [SerializeField] Color emissionColor = new Color(0.918f, 0.761f, 0.294f, 1f);          // base color, without intensity
    [SerializeField] float minIntensity = 3f; // "Intensity" as in the HDR color picker, at the slider minimum
    [SerializeField] float maxIntensity = 5f; // at the slider maximum

    MaterialPropertyBlock block;
    float sliderValue;
    bool isOn;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        sliderValue = intensitySlider.value;
        SetPowered(startOn);
    }

    // Called by the toggle: true = on (emissive), false = off
    public void SetPowered(bool on)
    {
        isOn = on;
        targetRenderer.sharedMaterial = on ? onMaterial : offMaterial;
        ApplyEmission();
    }

    // Called by the slider (Dynamic float)
    public void SetEmissionIntensity(float value)
    {
        sliderValue = value;
        ApplyEmission();
    }

    // Called by BlobGroup: the mixed RGB color becomes the base color of the emission
    public void SetEmissionColor(Color color)
    {
        emissionColor = color;
        ApplyEmission();
    }

    void ApplyEmission()
    {
        if (!isOn)
        {
            // Remove the override so the off material keeps its own look
            targetRenderer.SetPropertyBlock(null);
            return;
        }

        // Map the slider range (e.g. 15..75) to the intensity range (e.g. 3..5)
        float t = Mathf.InverseLerp(intensitySlider.minValue, intensitySlider.maxValue, sliderValue);
        float pickerIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        // The HDR color picker doubles the brightness per intensity step: color * 2^intensity
        Color hdrColor = emissionColor * Mathf.Pow(2f, pickerIntensity);

        targetRenderer.GetPropertyBlock(block);
        block.SetColor(emissionProperty, hdrColor);
        targetRenderer.SetPropertyBlock(block);
    }
}