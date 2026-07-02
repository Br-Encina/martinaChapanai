using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

// Versión simplificada de movimiento para un juego isométrico.
// Solo maneja movimiento por teclado (WASD / flechas), sin animaciones,
// sin sprint ni salto. Usa el mismo asset "InputSystem_Actions" 
// (acción Player/Move) que ya tenías configurado en tu otro proyecto.

[RequireComponent(typeof(CharacterController))]
public class IsometricMovementController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ajuste de cámara isométrica")]
    [Tooltip("Ángulo para alinear el input del teclado con los ejes visuales de tu cámara isométrica. Probá 45 como punto de partida.")]
    [SerializeField] private float isometricAngle = 45f;

    [Header("Gravedad simple")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravity = -0.05f;

    InputSystem_Actions inputActions;
    private CharacterController characterController;

    private Vector2 currentMovementInput;
    private Vector3 currentMovement;
    private bool isMovementPressed;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.started += OnMovementInput;
        inputActions.Player.Move.performed += OnMovementInput;
        inputActions.Player.Move.canceled += OnMovementInput;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.started -= OnMovementInput;
        inputActions.Player.Move.performed -= OnMovementInput;
        inputActions.Player.Move.canceled -= OnMovementInput;
        inputActions.Player.Disable();
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.sqrMagnitude > 0.0001f;
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        // Tomamos el input crudo del teclado (X = horizontal, Y = vertical)
        Vector3 inputDirection = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y);

        // Lo rotamos para que coincida con los ejes visuales de la cámara isométrica
        Quaternion isoRotation = Quaternion.Euler(0f, isometricAngle, 0f);
        Vector3 isometricDirection = isoRotation * inputDirection;

        currentMovement.x = isometricDirection.x * movementSpeed;
        currentMovement.z = isometricDirection.z * movementSpeed;

        // Rotar al personaje hacia la dirección en la que se mueve
        if (isMovementPressed)
        {
            Vector3 facingDirection = new Vector3(currentMovement.x, 0f, currentMovement.z);
            Quaternion targetRotation = Quaternion.LookRotation(facingDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = groundedGravity;
        }
        else
        {
            currentMovement.y += gravity * Time.deltaTime;
        }
    }
}