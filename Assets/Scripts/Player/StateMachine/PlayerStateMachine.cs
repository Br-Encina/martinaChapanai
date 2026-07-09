using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    InputSystem_Actions action;
    CharacterController characterController;
    public CharacterController CharacterController { get { return characterController; } }
    Animator animator;
    public Animator Animator { get { return animator; } }

    #region Movement Variables

    int isRunningHash;
    public int IsRunningHash { get { return isRunningHash; } }

    int isCrouchingHash;
    public int IsCrouchingHash { get { return isCrouchingHash; } }

    Vector2 currentMovementInput;
    public Vector3 CurrentMovementInput { get { return currentMovementInput; } }

    Vector3 appliedMovement;
    public float ApliedMovementX { get { return appliedMovement.x; } set { appliedMovement.x = value; } }
    public float ApliedMovementY { get { return appliedMovement.y; } set { appliedMovement.y = value; } }
    public float ApliedMovementZ { get { return appliedMovement.z; } set { appliedMovement.z = value; } }

    bool isMovementPressed;
    public bool IsMovementPressed { get { return isMovementPressed; } }

    bool isCrouchPressed;
    public bool IsCrouchPressed { get { return isCrouchPressed; } }

    [SerializeField] private float rotationSpeed = 540f; // grados por segundo, velocidad constante de giro

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4f; // Velocidad única, sin distinción walk/run
    public float MoveSpeed { get { return moveSpeed; } }


    [SerializeField] private float crouchMoveSpeed = 2f;
    public float CrouchMoveSpeed { get { return crouchMoveSpeed; } }

    [Header("Ajuste de cámara isométrica")]
    [Tooltip("Ángulo para alinear el input del teclado con los ejes visuales de tu cámara isométrica.")]
    [SerializeField] private float isometricAngle = 45f;
    public float IsometricAngle { get { return isometricAngle; } }

    [Header("Gravedad")]
    [SerializeField] private float gravity = -9.8f;
    public float Gravity { get { return gravity; } }

    #endregion

    #region Crouch / Detection  Variables 
    [Header("Deteccion por enemigos")]
    [Tooltip("Altura del punto de detecion cuando el player esta parado.")]
    [SerializeField] private float standingDetectionHeight = 1.8f;
    [Tooltip("Altura del punto de detecion cuando el player esta agachado.")]
    [SerializeField] private float crouchingDetectionHeight = 0.5f;

    // Punto del mundo que el enemigo usa para raycastear - cambia con el estado

    Vector3 detectionPoint;
    public Vector3 DetectionPoint { get { return detectionPoint; } set { detectionPoint = value; } }


    // El CharacterController cambia de altura al agacharse
    [Tooltip("Altura del CharacterController cuando el player esta parado.")]
    [SerializeField] private float standingCCHeight = 2f;
    [Tooltip("Altura del CharacterController cuando el player esta agachado.")]
    [SerializeField] private float crouchingCCHeight = 1f;

    public float StandingCCHeight { get { return standingCCHeight; } }
    public float CrouchingCCHeight { get { return crouchingCCHeight; } }

    bool isCrouching;
    public bool IsCrouching { get { return isCrouching; } set { isCrouching = value; } }
            
    #endregion

           
    PlayerBaseState currentState;
    public PlayerBaseState CurrentState { get { return currentState; } set { currentState = value; } }

    PlayerStateFactory states;

    public string newcurrentState;

    private void Awake()
    {
        action = new InputSystem_Actions();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        states = new PlayerStateFactory(this);
        currentState = states.Grounded();
        currentState.EnterState();

        isRunningHash = Animator.StringToHash("isRunning");
        isCrouchingHash = Animator.StringToHash("isCrouching");

        action.Player.Move.started += onMovementInput;
        action.Player.Move.canceled += onMovementInput;
        action.Player.Move.performed += onMovementInput;
        action.Player.Crouch.started += onCrouch;
        action.Player.Crouch.canceled += onCrouch;
    }

    void Start()
    {
        characterController.height = standingCCHeight;
        characterController.Move(appliedMovement * Time.deltaTime);
    }

    void Update()
    {
        handleRotatio();

        characterController.Move(appliedMovement * Time.deltaTime);
        currentState.UpdateStates();
        OnSwithState(currentState);
    }

    void handleRotatio()
    {
        Vector3 positionTolookAt;
        positionTolookAt.x = appliedMovement.x;
        positionTolookAt.y = 0.0f;
        positionTolookAt.z = appliedMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed && positionTolookAt.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionTolookAt);
            transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    //Actualiza el punto de deteccion según si esta agachado o parado
    void UpdateDetectionPoint()
    {
        float height = isCrouching ? crouchingDetectionHeight : standingDetectionHeight;
        detectionPoint = transform.position + Vector3.up * height;
    }
    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.sqrMagnitude > 0.0001f;
    }

    void onCrouch(InputAction.CallbackContext context)
    {
        isCrouchPressed = context.ReadValueAsButton();
    }
    private void OnEnable()
    {
        action.Player.Enable();
    }

    private void OnDisable()
    {
        action.Player.Disable();
    }

    void OnSwithState(PlayerBaseState currentState)
    {
        if (newcurrentState != currentState.GetType().Name)
        {
            newcurrentState = currentState.GetType().Name;
            Debug.Log("Current Player State: " + newcurrentState);
        }
    }

    // gizmo para ver el punto de deteccion en el editor
    private void OnDrawGizmos()
    {
        float height = isCrouching ? crouchingDetectionHeight : standingDetectionHeight;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.up * height, 0.1f);
    }
}
