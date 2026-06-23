using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform target; // El jugador a seguir

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);

    [Header("Suavizado")]
    [SerializeField] private float smoothSpeed = 5f; // Velocidad de seguimiento

    [Header("Rotación")]
    [SerializeField] private bool followRotation = false; // Si la cámara debe rotar con el jugador
    [SerializeField] private float rotationSpeed = 3f;

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("No hay target asignado para la cámara");
            return;
        }

        // Posición deseada (donde queremos que esté la cámara)
        Vector3 desiredPosition = target.position + offset;

        // Movimiento suave hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Opcional: seguir la rotación del jugador
        if (followRotation)
        {
            Quaternion desiredRotation = target.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
    }
}