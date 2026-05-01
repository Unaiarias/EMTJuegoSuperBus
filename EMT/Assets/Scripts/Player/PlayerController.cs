using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // grados/seg (suavidad al girar)
    //[SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private int groundCheckEveryNFixedFrames = 2; // 1=siempre

    [Header("References")]
    [SerializeField] private Transform groundCheckPoint;

    [Header("Particle Effects")]
    [SerializeField] private GameObject footstepParticlePrefab; // Prefab de partículas para caminar
    [SerializeField] private Transform footstepSpawnPoint; // Punto donde spawnear las partículas al caminar (puede ser un GameObject vacío)
    [SerializeField] private GameObject landParticlePrefab; // Prefab de partículas para impacto de salto
    [SerializeField] private Transform landSpawnPoint; // Punto donde spawnear las partículas al aterrizar

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource; // Fuente de audio para reproducir sonidos
    [SerializeField] public AudioClip sonidoSalto;

    [Header("Particle Timing")]
    [SerializeField] private float footstepInterval = 0.5f; // Intervalo entre partículas al caminar
    [SerializeField] private float minSpeedForFootsteps = 1f; // Velocidad mínima para que aparezcan partículas

    public InputActionReference triggerMovement;
    public InputActionReference triggerJump;

    private Rigidbody rb;
    private Transform camTr; // cache cámara

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool wasGrounded; // Para detectar cuando aterrizamos

    private readonly RaycastHit[] groundHits = new RaycastHit[1];
    private int fixedFrameCount;

    [SerializeField] private PlayerKnockback knockbackScript;

    // Variables para las partículas al caminar
    private float footstepTimer;
    private bool wasMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Cachear cámara si ya existe
        if (Camera.main != null) camTr = Camera.main.transform;

        if (groundCheckPoint == null)
            groundCheckPoint = transform;

        // Si no se asignaron puntos de spawn, usar el transform del player
        if (footstepSpawnPoint == null)
            footstepSpawnPoint = transform;

        if (landSpawnPoint == null)
            landSpawnPoint = transform;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Obtener el script de knockback del mismo objeto
        knockbackScript = GetComponent<PlayerKnockback>();

        // Inicializar variables
        footstepTimer = 0f;
        wasMoving = false;
        wasGrounded = true;

        // Si no hay AudioSource asignado, intentar obtenerlo del mismo GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Si no existe, agregar uno
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void OnEnable()
    {
        triggerMovement.action.performed += OnTriggerPressedMovement;
        triggerMovement.action.canceled += OnMovementCanceled;
        triggerMovement.action.Enable();

        triggerJump.action.performed += OnTriggerPressedJump;
        triggerJump.action.Enable();
    }

    private void OnDisable()
    {
        triggerMovement.action.performed -= OnTriggerPressedMovement;
        triggerMovement.action.canceled -= OnMovementCanceled;
        triggerMovement.action.Disable();

        triggerJump.action.performed -= OnTriggerPressedJump;
        triggerJump.action.Disable();
    }

    private void OnTriggerPressedMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnTriggerPressedJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void FixedUpdate()
    {
        // Si en Awake no existía Camera.main, la cacheamos aquí una vez
        if (camTr == null && Camera.main != null)
            camTr = Camera.main.transform;

        fixedFrameCount++;
        if (groundCheckEveryNFixedFrames <= 1 || (fixedFrameCount % groundCheckEveryNFixedFrames) == 0)
            CheckGrounded();

        HandleMovementDirectional();
        //HandleJump();
        HandleFootstepParticles();
        HandleLandParticles();
    }

    private void CheckGrounded()
    {
        int hits = Physics.RaycastNonAlloc(
            groundCheckPoint.position,
            Vector3.down,
            groundHits,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        wasGrounded = isGrounded; // Guardar estado anterior antes de actualizar
        isGrounded = hits > 0;
    }

    private void HandleMovementDirectional()
    {
        // Input (x = izquierda/derecha, y = arriba/abajo)
        Vector2 input = moveInput;
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Base de movimiento según cámara (si no hay cámara, fallback a mundo)
        Vector3 camForward = (camTr != null) ? camTr.forward : Vector3.forward;
        Vector3 camRight = (camTr != null) ? camTr.right : Vector3.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Dirección en mundo acorde a la pantalla
        Vector3 moveDir = (camRight * input.x + camForward * input.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // Rotar player hacia donde se mueve
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

            // Evita que la física meta giros mientras tú fuerzas la rotación
            rb.angularVelocity = Vector3.zero;

            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        // Mover en esa dirección (solo XZ) y mantener la Y
        Vector3 v = rb.linearVelocity;
        Vector3 desiredXZ = moveDir * moveSpeed;

        // SOLO toca XZ si NO hay knockback
        if (knockbackScript == null || !knockbackScript.IsKnockbackActive())
        {
            rb.linearVelocity = new Vector3(desiredXZ.x, v.y, desiredXZ.z);
        }
        // Si hay knockback, la velocidad la maneja PlayerKnockback con AddForce
    }

    /*
    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            Vector3 v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;

            // Reproducir sonido de salto
            ReproducirSonidoSalto();

            jumpPressed = false;
        }
        else if (jumpPressed && !isGrounded)
        {
            jumpPressed = false;
        }
    }
    */

    // Método para reproducir el sonido de salto
    /*private void ReproducirSonidoSalto()
    {
        if (sonidoSalto != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoSalto);
            Debug.Log("Reproduciendo sonido de salto");
        }
        else if (sonidoSalto == null)
        {
            Debug.LogWarning("No se ha asignado el clip de sonido de salto (sonidoSalto)");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("No se ha asignado el AudioSource");
        }
    }*/

    private void HandleFootstepParticles()
    {
        // Verificar si estamos en el suelo y nos estamos moviendo
        bool isMoving = IsMoving() && isGrounded;

        // Obtener la velocidad horizontal actual
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        if (isMoving && currentSpeed >= minSpeedForFootsteps && footstepParticlePrefab != null)
        {
            // Reducir el timer
            footstepTimer -= Time.fixedDeltaTime;

            // Si el timer llega a 0, instanciar partículas
            if (footstepTimer <= 0f)
            {
                SpawnParticle(footstepParticlePrefab, footstepSpawnPoint);
                footstepTimer = footstepInterval;
                wasMoving = true;
            }
        }
        else
        {
            // Resetear timer si no nos movemos
            if (wasMoving)
            {
                footstepTimer = 0f;
                wasMoving = false;
            }
        }
    }

    private void HandleLandParticles()
    {
        // Detectar cuando aterrizamos (antes estábamos en el aire y ahora estamos en el suelo)
        if (!wasGrounded && isGrounded && landParticlePrefab != null)
        {
            SpawnParticle(landParticlePrefab, landSpawnPoint);
        }
    }

    private void SpawnParticle(GameObject particlePrefab, Transform spawnPoint)
    {
        if (particlePrefab != null && spawnPoint != null)
        {
            // Instanciar el prefab en la posición del spawnPoint
            GameObject particleInstance = Instantiate(particlePrefab, spawnPoint.position, spawnPoint.rotation);

            // Opcional: Destruir automáticamente el efecto después de un tiempo
            // Esto es útil si los prefabs no se autodestruyen
            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration;
                Destroy(particleInstance, duration + 0.5f);
            }
            else
            {
                // Si no tiene ParticleSystem, destruir después de 2 segundos por defecto
                Destroy(particleInstance, 2f);
            }
        }
    }

    public bool IsGrounded() => isGrounded;
    public Vector2 GetMoveInput() => moveInput;
    public bool IsMoving() => moveInput.sqrMagnitude > 0.01f;
}