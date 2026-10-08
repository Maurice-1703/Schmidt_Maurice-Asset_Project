using UnityEngine;
using UnityEngine.EventSystems;

public class ColorBlob : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Color channelColor = Color.red; // the base color this blob adds to the light
    [SerializeField] bool startOn = true;            // all blobs start switched on (white light)
    [SerializeField] GameObject hoverOverlay;        // golden outline shown while hovering
    [SerializeField] GameObject activeOverlay;       // brighter sprite shown while switched on
    [SerializeField] BlobGroup group;

    public Color ChannelColor => channelColor;
    public bool IsOn { get; private set; }

    void Awake() => SetOn(startOn);

    // Hide the hover outline whenever the plant gets re-enabled
    void OnEnable() => hoverOverlay.SetActive(false);

    public void OnPointerEnter(PointerEventData e) => hoverOverlay.SetActive(true);
    public void OnPointerExit(PointerEventData e)  => hoverOverlay.SetActive(false);

    public void OnPointerClick(PointerEventData e)
    {
        SetOn(!IsOn);          // toggle this color channel
        group.OnBlobToggled(); // let the group recalculate the light color
    }

    public void SetOn(bool on)
    {
        IsOn = on;
        activeOverlay.SetActive(on);
    }
}