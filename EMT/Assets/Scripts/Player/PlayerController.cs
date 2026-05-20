using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private int groundCheckEveryNFixedFrames = 2;

    [Header("References")]
    [SerializeField] private Transform groundCheckPoint;

    [Header("Particle Effects")]
    [SerializeField] private GameObject footstepParticlePrefab;
    [SerializeField] private Transform footstepSpawnPoint;
    [SerializeField] private GameObject landParticlePrefab;
    [SerializeField] private Transform landSpawnPoint;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] public AudioClip sonidoSalto;

    [Header("Particle Timing")]
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private float minSpeedForFootsteps = 1f;

    public InputActionReference triggerMovement;
    public InputActionReference triggerJump;

    private Rigidbody rb;
    private Transform camTr;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool wasGrounded;

    public Animator an;

    private readonly RaycastHit[] groundHits = new RaycastHit[1];
    private int fixedFrameCount;

    [SerializeField] private PlayerKnockback knockbackScript;

    private float footstepTimer;
    private bool wasMoving;

    private PlayerAtaque playerAtaque;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (Camera.main != null) camTr = Camera.main.transform;

        if (groundCheckPoint == null)
            groundCheckPoint = transform;

        if (footstepSpawnPoint == null)
            footstepSpawnPoint = transform;

        if (landSpawnPoint == null)
            landSpawnPoint = transform;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        knockbackScript = GetComponent<PlayerKnockback>();

        footstepTimer = 0f;
        wasMoving = false;
        wasGrounded = true;

        playerAtaque = GetComponent<PlayerAtaque>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
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
        an.SetBool("IsRun",true);
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
        an.SetBool("IsRun", false);
    }

    private void OnTriggerPressedJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void FixedUpdate()
    {
        // Si el juego está en pausa, no procesar movimiento ni partículas
        if (MenuPausa.IsGamePaused) return;

        if (camTr == null && Camera.main != null)
            camTr = Camera.main.transform;

        fixedFrameCount++;
        if (groundCheckEveryNFixedFrames <= 1 || (fixedFrameCount % groundCheckEveryNFixedFrames) == 0)
            CheckGrounded();

        HandleMovementDirectional();
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

        wasGrounded = isGrounded;
        isGrounded = hits > 0;
    }

    private void HandleMovementDirectional()
    {
        Vector2 input = moveInput;
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 camForward = (camTr != null) ? camTr.forward : Vector3.forward;
        Vector3 camRight = (camTr != null) ? camTr.right : Vector3.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camRight * input.x + camForward * input.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            if (playerAtaque == null || !playerAtaque.EstaMirandoDireccionAtaque())
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                rb.angularVelocity = Vector3.zero;
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            }
        }

        Vector3 v = rb.linearVelocity;
        Vector3 desiredXZ = moveDir * moveSpeed;

        if (knockbackScript == null || !knockbackScript.IsKnockbackActive())
        {
            rb.linearVelocity = new Vector3(desiredXZ.x, v.y, desiredXZ.z);
        }
    }

    private void HandleFootstepParticles()
    {
        bool isMoving = IsMoving() && isGrounded;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        if (isMoving && currentSpeed >= minSpeedForFootsteps && footstepParticlePrefab != null)
        {
            footstepTimer -= Time.fixedDeltaTime;

            if (footstepTimer <= 0f)
            {
                SpawnParticle(footstepParticlePrefab, footstepSpawnPoint);
                footstepTimer = footstepInterval;
                wasMoving = true;
            }
        }
        else
        {
            if (wasMoving)
            {
                footstepTimer = 0f;
                wasMoving = false;
            }
        }
    }

    private void HandleLandParticles()
    {
        if (!wasGrounded && isGrounded && landParticlePrefab != null)
        {
            SpawnParticle(landParticlePrefab, landSpawnPoint);
        }
    }

    private void SpawnParticle(GameObject particlePrefab, Transform spawnPoint)
    {
        if (particlePrefab != null && spawnPoint != null)
        {
            GameObject particleInstance = Instantiate(particlePrefab, spawnPoint.position, spawnPoint.rotation);

            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration;
                Destroy(particleInstance, duration + 0.5f);
            }
            else
            {
                Destroy(particleInstance, 2f);
            }
        }
    }

    public bool IsGrounded() => isGrounded;
    public Vector2 GetMoveInput() => moveInput;
    public bool IsMoving() => moveInput.sqrMagnitude > 0.01f;
}