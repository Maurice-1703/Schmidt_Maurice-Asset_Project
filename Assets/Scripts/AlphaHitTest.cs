using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaHitTest : MonoBehaviour
{
    [Range(0f, 1f)] public float threshold = 0.5f; // minimum pixel opacity that counts as a hit

    void Awake()
    {
        // Ignore clicks on transparent pixels (requires Read/Write on the texture)
        GetComponent<Image>().alphaHitTestMinimumThreshold = threshold;
    }
}