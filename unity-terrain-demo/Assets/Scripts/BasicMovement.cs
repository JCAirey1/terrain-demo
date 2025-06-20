using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMovement : MonoBehaviour
{
    public Camera _cam;
    public const float speed = 10.0f;
    public const float rotationSpeed = 100.0f;
    public const float camPanSpeed = 5.0f;

    private Vector2 moveInput;
    private float verticalInput;
    private float camPanInput;
    private InputAction moveAction;
    private InputAction verticalAction;
    private InputAction ctrlAction;
    private InputAction camPanAction;

    private void Awake()
    {
        // Movement (WASD/Arrow keys/Gamepad)
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
            type: InputActionType.Value
        );
        verticalAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/q")
            .With("Positive", "<Keyboard>/e");

        // Ctrl key
        ctrlAction = new InputAction(
            type: InputActionType.Button,
            binding: "<Keyboard>/leftCtrl"
        );

        // Camera pan (R = up, F = down)
        camPanAction = new InputAction(
            type: InputActionType.Value
        );
        camPanAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/f")
            .With("Positive", "<Keyboard>/r");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        verticalAction.Enable();
        ctrlAction.Enable();
        camPanAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        verticalAction.Disable();
        ctrlAction.Disable();
        camPanAction.Disable();
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        verticalInput = verticalAction.ReadValue<float>();
        camPanInput = camPanAction.ReadValue<float>();
        bool ctrlHeld = ctrlAction.ReadValue<float>() > 0.5f;

        float ztranslation = moveInput.y * speed * Time.deltaTime;
        float ytranslation = verticalInput * speed * Time.deltaTime;
        float xtranslation = 0f;
        float rotation = 0f;

        if (ctrlHeld)
        {
            rotation = moveInput.x * rotationSpeed * Time.deltaTime;
        }
        else
        {
            xtranslation = moveInput.x * speed * Time.deltaTime;
        }

        // Camera panning up/down
        if (_cam != null && Mathf.Abs(camPanInput) > 0.01f)
        {
            _cam.transform.Translate(Vector3.up * camPanInput * camPanSpeed * Time.deltaTime, Space.Self);
        }

        transform.Translate(xtranslation, ytranslation, ztranslation);
        transform.Rotate(0, rotation, 0);
    }
}
