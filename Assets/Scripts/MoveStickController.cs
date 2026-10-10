using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MoveStickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Sprites")]
    [SerializeField] Sprite center;
    [SerializeField] Sprite right, upRight, up, upLeft, left, downLeft, down, downRight;
    [SerializeField] Sprite disabled;        // greyed-out sprite while switched off

    [Header("Behaviour")]
    [SerializeField] float deadZone = 10f;   // distance from the center (UI units) before a direction is chosen
    [SerializeField] bool startPowered = false; // must match the toggle's "Is On"

    public Vector2 Direction { get; private set; } // e.g. (1,0) = right, (0.71,0.71) = up-right, (0,0) = centered
    public UnityEvent<Vector2> onDirectionChanged; // optional, fires only when the direction changes

    Image image;
    RectTransform rect;
    Sprite[] directionSprites;
    int currentIndex = -1; // -1 = centered
    bool powered;

    void Awake()
    {
        image = GetComponent<Image>();
        rect = (RectTransform)transform;

        // Order matches the angle math below: counter-clockwise, starting at "right"
        directionSprites = new[] { right, upRight, up, upLeft, left, downLeft, down, downRight };

        powered = startPowered;
        image.sprite = powered ? center : disabled;
    }

    // Called by the toggle: true = usable, false = greyed out and locked
    public void SetPowered(bool on)
    {
        powered = on;
        SetIndex(-1);                          // release the stick (reports (0,0) if it was tilted)
        image.sprite = on ? center : disabled; // show the grey sprite while switched off
    }

    public void OnPointerDown(PointerEventData e) => OnDrag(e);

    public void OnDrag(PointerEventData e)
    {
        if (!powered) return; // ignore input while switched off

        // Pointer position relative to the middle of the joystick image
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect, e.position, e.pressEventCamera, out Vector2 local);
        Vector2 offset = local - rect.rect.center;

        // Too close to the center: stay in the neutral pose
        if (offset.magnitude < deadZone)
        {
            SetIndex(-1);
            return;
        }

        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg; // 0° = right, counter-clockwise
        int index = Mathf.RoundToInt(angle / 45f);                     // snap to the nearest of 8 directions
        SetIndex((index + 8) % 8);                                     // map -4..4 to 0..7
    }

    // Called even if the pointer is released outside the image
    public void OnPointerUp(PointerEventData e)
    {
        if (powered) SetIndex(-1);
    }

    void SetIndex(int index)
    {
        if (index == currentIndex) return; // nothing changed
        currentIndex = index;

        if (index < 0)
        {
            image.sprite = center;
            Direction = Vector2.zero;
        }
        else
        {
            image.sprite = directionSprites[index];
            float rad = index * 45f * Mathf.Deg2Rad;
            Direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        onDirectionChanged?.Invoke(Direction);
    }
}