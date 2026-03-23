using UnityEngine;

public class CameraAnchorFollowX : MonoBehaviour
{
    [SerializeField] private Transform player;   // usa CamTarget o el Player root
    [SerializeField] private float smooth = 12f; // 8-15 va bien

    private void LateUpdate()
    {
        if (!player) return;

        Vector3 p = transform.position;
        float targetX = player.position.x;

        // Sigue solo X. Mantén Y y Z.
        p.x = Mathf.Lerp(p.x, targetX, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        transform.position = p;
    }
}