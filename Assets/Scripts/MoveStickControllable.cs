using UnityEngine;

public class MoveStickControllable : MonoBehaviour
{
    public enum Mode { Move, Rotate }
    public enum MovePlane { Ground, Vertical } // XZ (floor) or XY (wall)

    [SerializeField] MoveStickController joystick;
    [SerializeField] Mode mode = Mode.Move;

    [Header("Move")]
    [SerializeField] MovePlane plane = MovePlane.Ground;
    [SerializeField] float moveSpeed = 3f;   // units per second
    [SerializeField] Transform relativeTo;   // optional, e.g. the camera: "up" on the joystick = away from it

    [Header("Rotate")]
    [SerializeField] float rotateSpeed = 90f; // degrees per second
    [SerializeField] float maxPitch = 60f;    // limit for tilting up/down
    
    [Header("Input")]
    [SerializeField] bool invertX; // flip left/right
    [SerializeField] bool invertY; // flip up/down

    float yaw, pitch;

    void Start()
    {
        // Start from the object's current rotation
        Vector3 euler = transform.localEulerAngles;
        yaw = euler.y;
        pitch = euler.x > 180f ? euler.x - 360f : euler.x; // convert 0..360 to -180..180
    }

    void Update()
{
    if (!joystick) return;

    Vector2 input = joystick.Direction;
    if (input == Vector2.zero) return; // joystick centered or switched off

    // Optional axis flips, e.g. when the object faces the camera
    if (invertX) input.x = -input.x;
    if (invertY) input.y = -input.y;

    if (mode == Mode.Move) Move(input);
    else Rotate(input);
}

    void Move(Vector2 input)
    {
        Vector3 right, forwardOrUp;

        if (plane == MovePlane.Ground)
        {
            // Joystick up/down = forward/back on the floor, left/right = sideways
            right       = relativeTo ? Flatten(relativeTo.right)   : Vector3.right;
            forwardOrUp = relativeTo ? Flatten(relativeTo.forward) : Vector3.forward;
        }
        else
        {
            // Joystick up/down = up/down, left/right = sideways
            right       = relativeTo ? relativeTo.right : Vector3.right;
            forwardOrUp = relativeTo ? relativeTo.up    : Vector3.up;
        }

        Vector3 direction = right * input.x + forwardOrUp * input.y;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void Rotate(Vector2 input)
    {
        yaw   += input.x * rotateSpeed * Time.deltaTime; // left/right = turn
        pitch -= input.y * rotateSpeed * Time.deltaTime; // up = tilt up
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // Removes the vertical part so movement stays on the floor
    static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }
}