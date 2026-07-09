using UnityEngine;

// Requiere que el Player tenga el tag "Player" asignado en Unity.
// Los waypoints son GameObjects vacíos que creás en la escena y arrastrás al array desde el Inspector.

public class EnemyController : MonoBehaviour
{
    // ── Estados internos ──────────────────────────────────────────────────────
    enum EnemyState { Patrolling, PlayerDetected }
    EnemyState currentState = EnemyState.Patrolling;

    // Propiedad pública para que EnemyVisionCone pueda leer el estado
    public bool IsDetected { get { return currentState == EnemyState.PlayerDetected; } }

    // ── Patrulla ──────────────────────────────────────────────────────────────
    [Header("Patrulla")]
    [Tooltip("GameObjects vacíos que marcan la ruta del enemigo.")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float rotationSpeed = 5f;
    [Tooltip("Distancia al waypoint para considerarlo alcanzado y pasar al siguiente.")]
    [SerializeField] float waypointTolerance = 0.3f;

    int currentWaypointIndex = 0;

    // ── Detección ─────────────────────────────────────────────────────────────
    [Header("Detección")]
    [Tooltip("Distancia máxima a la que el enemigo puede ver al jugador.")]
    [SerializeField] float detectionRange = 6f;
    [Tooltip("Ángulo total del cono de visión (ej: 90 = 45° a cada lado).")]
    [SerializeField][Range(10f, 360f)] float detectionAngle = 90f;
    [Tooltip("Si está activado, el enemigo necesita línea de vista directa (sin paredes de por medio).")]
    [SerializeField] bool requireLineOfSight = true;
    [SerializeField] LayerMask obstacleLayer;

    // ── Referencias ───────────────────────────────────────────────────────────
    Transform playerTransform;
    PauseManager pauseManager;
    Animator animator;

    [Header("Ajuste")]
    [SerializeField] float eyeHeight = 1f;

    // Exponer estos valores para que EnemyVisionCone pueda leerlos
    public float DetectionRange { get { return detectionRange; } }
    public float DetectionAngle { get { return detectionAngle; } }
    public float EyeHeight { get { return eyeHeight; } }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("[EnemyController] No se encontró un GameObject con tag 'Player'.");

        pauseManager = FindAnyObjectByType<PauseManager>();
        if (pauseManager == null)
            Debug.LogWarning("[EnemyController] No se encontró un PauseManager en la escena.");

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                DetectPlayer();
                break;

            case EnemyState.PlayerDetected:
                break;
        }
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        bool isMoving = direction.magnitude > waypointTolerance;
        if (animator != null)
            animator.SetBool("isWalking", isMoving);

        if (isMoving)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void DetectPlayer()
    {
        if (playerTransform == null) return;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Usamos el DetectionPoint del player (torso/pecho) en vez de sus pies.
        // Así una pared baja bloquea al player agachado pero no al que está parado.
        PlayerStateMachine playerSM = playerTransform.GetComponent<PlayerStateMachine>();
        Vector3 targetPoint = playerSM != null ? playerSM.DetectionPoint : playerTransform.position + Vector3.up * 1.2f;

        // Para el chequeo de ángulo y distancia usamos dirección horizontal
        Vector3 toPlayer = targetPoint - eyePosition;
        Vector3 toPlayerFlat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distance = toPlayerFlat.magnitude;

        // 1. ¿Está dentro del rango?
        if (distance > detectionRange) return;

        // 2. ¿Está dentro del ángulo del cono?
        float angle = Vector3.Angle(transform.forward, toPlayerFlat);
        if (angle > detectionAngle / 2f) return;

        // 3. ¿Tiene línea de vista directa hacia el punto de detección del player?
        if (requireLineOfSight)
        {
            Vector3 direction = (targetPoint - eyePosition).normalized;
            float rayDistance = Vector3.Distance(eyePosition, targetPoint);
            if (Physics.Raycast(eyePosition, direction, rayDistance, obstacleLayer))
                return; // La pared bloquea la vista
        }

        OnPlayerDetected();
    }

    void OnPlayerDetected()
    {
        currentState = EnemyState.PlayerDetected;
        Debug.Log("[EnemyController] ¡Jugador detectado por " + gameObject.name + "!");

        if (pauseManager != null)
            pauseManager.PauseGame();
    }

    // Gizmos para los waypoints (el cono ahora lo dibuja EnemyVisionCone como mesh real)
    private void OnDrawGizmos()
    {
        if (waypoints == null) return;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.color = (i == currentWaypointIndex) ? Color.green : Color.yellow;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            Gizmos.color = Color.yellow;
            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
        }
    }
}