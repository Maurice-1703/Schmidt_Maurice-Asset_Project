using UnityEngine;

public class BlobGroup : MonoBehaviour
{
    [SerializeField] Light targetLight;           // the light inside the bulb
    [SerializeField] PlantLampController bulb;       // tints the bulb's emission with the same color
    [SerializeField] ColorBlob[] blobs;
    [SerializeField] Color allOffColor = Color.white; // fallback when every blob is off

    [Header("Optional")]
    [SerializeField] Animator plantAnimator;      // plays the plant animation on every click
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
        bool anyOn = false;

        foreach (var blob in blobs)
        {
            if (!blob.IsOn) continue;
            mixed += blob.ChannelColor;
            anyOn = true;
        }

        if (!anyOn) mixed = allOffColor;

        // Clamp each channel to 0..1 and force full opacity
        // (adding colors also adds their alpha values)
        mixed.r = Mathf.Clamp01(mixed.r);
        mixed.g = Mathf.Clamp01(mixed.g);
        mixed.b = Mathf.Clamp01(mixed.b);
        mixed.a = 1f;

        // Same color for the light and the glowing bulb
        targetLight.color = mixed;
        if (bulb) bulb.SetEmissionColor(mixed);
    }
}