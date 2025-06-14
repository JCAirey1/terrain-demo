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
    private InputAction ctrlAction;

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
    }

    private void OnEnable()
    {
        moveAction.Enable();
        verticalAction.Enable();
        ctrlAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        verticalAction.Disable();
        ctrlAction.Disable();
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        verticalInput = verticalAction.ReadValue<float>();
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

        Debug.Log($"cv {ctrlAction.ReadValue<float>()} verticalInput: {verticalInput}, ytranslation: {ytranslation}, ctrlHeld: {ctrlHeld}, xtranslation: {xtranslation}, rotation: {rotation}");

        transform.Translate(xtranslation, ytranslation, ztranslation);
        transform.Rotate(0, rotation, 0);
    }
}
