using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveStickController : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Images")]
    [SerializeField] Image stickImage;        // mushroom: swaps between the direction sprites
    [SerializeField] Image baseImage;         // wooden board: static, also acts as the touch area

    [Header("Stick sprites")]
    [SerializeField] Sprite center;
    [SerializeField] Sprite right, upRight, up, upLeft, left, downLeft, down, downRight;
    [SerializeField] Sprite stickDisabled;    // greyed-out mushroom

    [Header("Base sprites")]
    [SerializeField] Sprite baseDisabled;     // optional greyed-out board (leave empty to keep the board unchanged)

    [Header("Hover")]
    [SerializeField] GameObject hoverOverlay; // golden outline of the neutral mushroom

    [Header("Behaviour")]
    [SerializeField] float deadZone = 10f;      // distance from the pivot (UI units) before a direction is chosen
    [SerializeField] bool startPowered = false; // must match the toggle's "Is On"

    public Vector2 Direction { get; private set; } // e.g. (1,0) = right, (0.71,0.71) = up-right, (0,0) = centered
    public UnityEvent<Vector2> onDirectionChanged; // optional, fires only when the direction changes

    RectTransform rect;
    Sprite[] directionSprites;
    Sprite baseNormal;
    int currentIndex = -1; // -1 = centered
    bool powered;
    bool hovering;
    bool dragging;

    void Awake()
    {
        rect = (RectTransform)transform;
        baseNormal = baseImage.sprite;

        // Order matches the angle math below: counter-clockwise, starting at "right"
        directionSprites = new[] { right, upRight, up, upLeft, left, downLeft, down, downRight };

        powered = startPowered;
        ApplyPoweredLook();
        UpdateHover();
    }

    // Called by the toggle: true = usable, false = greyed out and locked
    public void SetPowered(bool on)
    {
        powered = on;
        SetIndex(-1);       // release the stick (reports (0,0) if it was tilted)
        ApplyPoweredLook();
        UpdateHover();      // no hover while switched off
    }

    void ApplyPoweredLook()
    {
        stickImage.sprite = powered ? center : stickDisabled;

        // Only swap the board if a greyed-out version is assigned
        if (baseDisabled != null)
            baseImage.sprite = powered ? baseNormal : baseDisabled;
    }

    // ---------- Hover ----------

    public void OnPointerEnter(PointerEventData e)
    {
        hovering = true;
        UpdateHover();
    }

    public void OnPointerExit(PointerEventData e)
    {
        hovering = false;
        UpdateHover();
    }

    void UpdateHover()
    {
        // Only while switched on, hovered and not being dragged
        if (hoverOverlay)
            hoverOverlay.SetActive(powered && hovering && !dragging);
    }

    // ---------- Input ----------

    public void OnPointerDown(PointerEventData e)
    {
        dragging = true;
        UpdateHover(); // hide the outline while the stick is being moved
        OnDrag(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!powered) return; // ignore input while switched off

        // Pointer position in the joystick's local space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect, e.position, e.pressEventCamera, out Vector2 local);

        // Measure from the mushroom's pivot (the foot of the stem), not the image center
        Vector2 pivot = rect.InverseTransformPoint(stickImage.rectTransform.position);
        Vector2 offset = local - pivot;

        // Too close to the pivot: stay in the neutral pose
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
        dragging = false;
        if (powered) SetIndex(-1);
        UpdateHover(); // show the outline again if the pointer is still over the joystick
    }

    // ---------- Direction ----------

    void SetIndex(int index)
    {
        if (index == currentIndex) return; // nothing changed
        currentIndex = index;

        if (index < 0)
        {
            stickImage.sprite = center;
            Direction = Vector2.zero;
        }
        else
        {
            stickImage.sprite = directionSprites[index];
            float rad = index * 45f * Mathf.Deg2Rad;
            Direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        onDirectionChanged?.Invoke(Direction);
    }
}