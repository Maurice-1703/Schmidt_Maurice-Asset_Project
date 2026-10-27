using UnityEngine;

public class BlobGroup : MonoBehaviour
{
    [SerializeField] Light targetLight;            // the light inside the bulb
    [SerializeField] PlantLampController bulb;     // tints the bulb's emission with the same color
    [SerializeField] ColorBlob[] blobs;
    [SerializeField] Color allOffColor = new Color(0.35f, 0.35f, 0.35f); // fallback when every blob is off

    [Header("All blobs on")]
    [SerializeField] Color allOnBulbColor = new Color32(0xEA, 0xC2, 0x4B, 0xFF); // warm base color instead of pure white
    [SerializeField] bool alsoTintLight = false;   // use the warm color for the light as well

    [Header("Optional")]
    [SerializeField] Animator plantAnimator;       // plays the plant animation on every click
    [SerializeField] string playTrigger = "Play";

    void Start() => UpdateLight();

    public void OnBlobToggled()
    {
        UpdateLight();
        if (plantAnimator) plantAnimator.SetTrigger(playTrigger);
    }

    void UpdateLight()
    {
        // Additive color mixing: sum up the channel colors of all active blobs
        Color mixed = Color.black;
        int activeCount = 0;

        foreach (var blob in blobs)
        {
            if (!blob.IsOn) continue;
            mixed += blob.ChannelColor;
            activeCount++;
        }

        if (activeCount == 0) mixed = allOffColor;

        // Clamp each channel to 0..1 and force full opacity
        // (adding colors also adds their alpha values)
        mixed.r = Mathf.Clamp01(mixed.r);
        mixed.g = Mathf.Clamp01(mixed.g);
        mixed.b = Mathf.Clamp01(mixed.b);
        mixed.a = 1f;

        // With all blobs on, the warm base color replaces pure white
        bool allOn = activeCount == blobs.Length;
        Color bulbColor  = allOn ? allOnBulbColor : mixed;
        Color lightColor = (allOn && alsoTintLight) ? allOnBulbColor : mixed;

        targetLight.color = lightColor;
        if (bulb) bulb.SetEmissionColor(bulbColor);
    }
}