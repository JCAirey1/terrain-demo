using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : MonoBehaviour
{
    public Camera _cam;
    public const float speed = 10.0f;
    public const float rotationSpeed = 100.0f;

    private Vector2 moveInput;
    private float verticalInput;
    private InputAction moveAction;
    private InputAction verticalAction;

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

        // Vertical movement (Q = down, E = up)
        verticalAction = new InputAction(
            type: InputActionType.Value,
            binding: "<Gamepad>/leftStick"
        );

        verticalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/q")
            .With("Positive", "<Keyboard>/e");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        verticalAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        verticalAction.Disable();
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        verticalInput = verticalAction.ReadValue<float>();

        float ztranslation = moveInput.y * speed * Time.deltaTime;
        float ytranslation = verticalInput * speed * Time.deltaTime;
        float rotation = moveInput.x * rotationSpeed * Time.deltaTime;

        Debug.Log($"verticalInput: {verticalInput}, ytranslation: {ztranslation}");

        transform.Translate(0, ytranslation, ztranslation);
        transform.Rotate(0, rotation, 0);
    }
}