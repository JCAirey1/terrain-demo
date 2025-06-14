using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : MonoBehaviour
{
    public Camera _cam;
    public const float speed = 10.0f;
    public const float rotationSpeed = 100.0f;

    private Vector2 moveInput;
    private InputAction moveAction;

    private void Awake()
    {
        // Create a new InputAction for movement (WASD/Arrow keys by default)
        moveAction = new InputAction(
            type: InputActionType.Value,
            binding: "<Gamepad>/leftStick"
        );
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        float translation = moveInput.y * speed * Time.deltaTime;
        float rotation = moveInput.x * rotationSpeed * Time.deltaTime;

        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);
    }
}