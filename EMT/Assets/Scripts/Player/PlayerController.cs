using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // grados/seg (suavidad al girar)
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private int groundCheckEveryNFixedFrames = 2; // 1=siempre

    [Header("References")]
    [SerializeField] private Transform groundCheckPoint;

    public InputActionReference triggerMovement;
    public InputActionReference triggerJump;

    private Rigidbody rb;
    private Transform camTr; // cache cámara

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;

    private readonly RaycastHit[] groundHits = new RaycastHit[1];
    private int fixedFrameCount;

    [SerializeField] private PlayerKnockback knockbackScript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Cachear cámara si ya existe
        if (Camera.main != null) camTr = Camera.main.transform;

        if (groundCheckPoint == null)
            groundCheckPoint = transform;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;


        // Obtener el script de knockback del mismo objeto
        knockbackScript = GetComponent<PlayerKnockback>();
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
        HandleJump();
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

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            Vector3 v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;
            jumpPressed = false;
        }
        else if (jumpPressed && !isGrounded)
        {
            jumpPressed = false;
        }
    }

    public bool IsGrounded() => isGrounded;
    public Vector2 GetMoveInput() => moveInput;
    public bool IsMoving() => moveInput.sqrMagnitude > 0.01f;
}