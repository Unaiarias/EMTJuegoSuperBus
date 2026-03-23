using UnityEngine;
using Unity.Cinemachine;

public class CameraTravelByScreenY : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform target;        // CamTarget
    [SerializeField] private Rigidbody playerRb;      // Rigidbody del PlayerPrototipo
    [SerializeField] private CinemachineCamera camContraPicado;
    [SerializeField] private CinemachineCamera camFrontal;

    [Header("Viewport thresholds (0..1)")]
    [Range(0f, 1f)][SerializeField] private float enterFrontalY = 0.45f;
    [Range(0f, 1f)][SerializeField] private float exitFrontalY = 0.52f;

    [Header("Stability")]
    [SerializeField] private bool ignoreTargetHeight = true;
    [SerializeField] private float fixedTargetY = 0.6f;
    [SerializeField] private float minTimeBetweenSwitches = 0.4f;

    [Header("Return to ContraPicado (Z+ forward)")]
    [Tooltip("Velocidad mínima en +Z para contar como 'avanzar'")]
    [SerializeField] private float forwardSpeedThreshold = 0.35f;

    [Tooltip("Tiempo que debe mantener esa velocidad para volver a contrapicado")]
    [SerializeField] private float forwardHoldTime = 0.25f;

    [Tooltip("Delay tras entrar en frontal antes de poder volver")]
    [SerializeField] private float forwardReturnDelay = 0.25f;

    [Header("Optimization")]
    [SerializeField] private float checksPerSecond = 12f;

    private bool frontalActive;
    private float nextCheckTime;
    private float lastSwitchTime = -999f;
    private float enteredFrontalTime = -999f;

    private float forwardAccum; // cuánto tiempo llevo avanzando en +Z

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        SetFrontal(false);
    }

    private void Update()
    {
        if (mainCamera == null || target == null || camContraPicado == null || camFrontal == null)
            return;

        if (Time.unscaledTime < nextCheckTime) return;
        nextCheckTime = Time.unscaledTime + (1f / Mathf.Max(1f, checksPerSecond));

        if (Time.unscaledTime - lastSwitchTime < minTimeBetweenSwitches)
            return;

        Vector3 worldPos = target.position;
        if (ignoreTargetHeight) worldPos.y = fixedTargetY;

        Vector3 vp = mainCamera.WorldToViewportPoint(worldPos);
        if (vp.z < 0f) return;

        // Si estamos en frontal: retorno por avanzar en +Z de forma sostenida
        if (frontalActive && playerRb != null)
        {
            if (Time.unscaledTime - enteredFrontalTime > forwardReturnDelay)
            {
                float vz = playerRb.linearVelocity.z; // Z+ = adelante
                if (vz > forwardSpeedThreshold)
                    forwardAccum += (1f / Mathf.Max(1f, checksPerSecond));
                else
                    forwardAccum = 0f;

                if (forwardAccum >= forwardHoldTime)
                {
                    SetFrontal(false);
                    return;
                }
            }
        }

        // Entrada / salida por viewport (si prefieres SOLO por intención, se puede apagar el exit)
        if (!frontalActive && vp.y < enterFrontalY)
            SetFrontal(true);
        else if (frontalActive && vp.y > exitFrontalY)
            SetFrontal(false);
    }

    private void SetFrontal(bool active)
    {
        frontalActive = active;

        camFrontal.Priority = active ? 20 : 10;
        camContraPicado.Priority = active ? 10 : 20;

        lastSwitchTime = Time.unscaledTime;

        if (active)
        {
            enteredFrontalTime = Time.unscaledTime;
            forwardAccum = 0f; // reset acumulador al entrar
        }
        else
        {
            forwardAccum = 0f;
        }
    }
}