using UnityEngine;

// Bends the lamp's neck with the joystick
// The total bend angle is spread evenly over all neck bones (Neck_1 .. Neck_5),
// so the stem curves smoothly, the base and the lamp root stay where they are
public class NeckBendController : MonoBehaviour
{
    [SerializeField] MoveStickController joystick;

    [Header("Bones")]
    [SerializeField] Transform[] neckBones; // Neck_1 .. Neck_5, from bottom to top
    [SerializeField] Transform headBone;    // optional: Head, tilts a little extra (aiming the light)
    [SerializeField, Range(0f, 1f)] float headShare = 0.2f; // part of the bend the head gets on top

    [Header("Bend")]
    [SerializeField] float bendSpeed = 60f; // degrees per second (whole neck)
    [SerializeField] float maxBend = 45f;   // limit per direction (whole neck)
    [SerializeField] bool returnToRest;     // spring back when the joystick is released
    [SerializeField] float returnSpeed = 90f;

    [Header("Axes (bone local space)")]
    // Blender bones point along their local Y axis, so they bend around X (forward/back)
    // and Z (sideways). Swap or invert here if the neck bends the wrong way.
    [SerializeField] Vector3 forwardAxis = Vector3.right;   // joystick up/down
    [SerializeField] Vector3 sideAxis = Vector3.forward;    // joystick left/right
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    Quaternion[] neckRest;
    Quaternion headRest;
    Vector2 bend; // current total bend in degrees: x = sideways, y = forward/back

    void Awake()
    {
        // Remember the rest pose so the bend is always added on top of it
        neckRest = new Quaternion[neckBones.Length];
        for (int i = 0; i < neckBones.Length; i++)
            neckRest[i] = neckBones[i].localRotation;

        if (headBone) headRest = headBone.localRotation;
    }

    void Update()
    {
        Vector2 input = joystick ? joystick.Direction : Vector2.zero;
        if (invertX) input.x = -input.x;
        if (invertY) input.y = -input.y;

        if (input != Vector2.zero)
        {
            bend += input * bendSpeed * Time.deltaTime;
            bend.x = Mathf.Clamp(bend.x, -maxBend, maxBend);
            bend.y = Mathf.Clamp(bend.y, -maxBend, maxBend);
        }
        else if (returnToRest)
        {
            bend = Vector2.MoveTowards(bend, Vector2.zero, returnSpeed * Time.deltaTime);
        }
    }

    // LateUpdate runs after the Animator, so an idle animation cannot overwrite the bend
    void LateUpdate()
    {
        if (neckBones.Length == 0) return;

        // Each bone gets the same share, so 5 bones x 9 degrees = 45 degrees in total
        float neckShare = 1f - (headBone ? headShare : 0f);
        Vector2 perBone = bend * neckShare / neckBones.Length;
        Quaternion offset = Quaternion.AngleAxis(perBone.y, forwardAxis) * Quaternion.AngleAxis(perBone.x, sideAxis);

        for (int i = 0; i < neckBones.Length; i++)
            neckBones[i].localRotation = neckRest[i] * offset;

        if (headBone)
        {
            Vector2 head = bend * headShare;
            headBone.localRotation = headRest
                * Quaternion.AngleAxis(head.y, forwardAxis)
                * Quaternion.AngleAxis(head.x, sideAxis);
        }
    }
}