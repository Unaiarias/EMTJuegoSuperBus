using UnityEngine;

public class CameraAnchorFollowXZDeadZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target; // CamTarget o Player

    [Header("Follow")]
    [SerializeField] private float smoothX = 14f;
    [SerializeField] private float smoothZ = 10f;

    [Header("Dead Zone in Z (world units)")]
    [Tooltip("Cuánto puede moverse el target en Z sin que la cámara lo recentre.")]
    [SerializeField] private float deadZoneZ = 2.0f;

    private float anchorZ;

    private void Start()
    {
        if (target)
        {
            anchorZ = target.position.z;
            Vector3 p = transform.position;
            p.x = target.position.x;
            p.z = anchorZ;
            transform.position = p;
        }
    }

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 p = transform.position;

        // 1) X: sigue siempre
        float desiredX = target.position.x;
        p.x = Damp(p.x, desiredX, smoothX);

        // 2) Z: dead zone
        // Si el target se aleja más de deadZoneZ, movemos el anchor hacia él, pero sin centrarlo totalmente
        float dz = target.position.z - anchorZ;
        if (dz > deadZoneZ) anchorZ = target.position.z - deadZoneZ;
        else if (dz < -deadZoneZ) anchorZ = target.position.z + deadZoneZ;

        p.z = Damp(p.z, anchorZ, smoothZ);

        transform.position = p;
    }

    private static float Damp(float current, float target, float smooth)
    {
        // Suavizado estable (no depende de framerate)
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        return Mathf.Lerp(current, target, t);
    }
}