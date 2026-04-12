using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossJumpAnimationController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject shadowObject;
    public NavMeshAgent agent;
    public float jumpHeight = 6f;
    public float damageRadius = 4f;

    [Header("Timing")]
    public float upDuration = 0.35f;
    public float downDuration = 0.45f;

    bool isJumping;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (shadowObject != null) shadowObject.SetActive(false);
    }

    // Animation Event 1: INICIO (después del trigger)
    public void JumpStart()
    {
        if (isJumping || player == null) return;

        // Desactivar NavMeshAgent
        if (agent != null) agent.enabled = false;

        // Activar sombra
        if (shadowObject != null)
        {
            shadowObject.transform.position = player.position + Vector3.up * 0.02f;
            shadowObject.transform.localScale = Vector3.one * damageRadius * 2f;
            shadowObject.SetActive(true);
        }

        isJumping = true;
    }

    // Animation Event 2: SUBIR
    public void JumpUp()
    {
        Vector3 topPos = player.position + Vector3.up * jumpHeight;
        StartCoroutine(MoveTo(transform.position, topPos, upDuration));
    }

    // Animation Event 3: BAJAR
    public void JumpDown()
    {
        Vector3 landingPos = player.position + Vector3.up * 0.1f;
        StartCoroutine(MoveTo(transform.position, landingPos, downDuration));
    }

    // Animation Event 4: FINAL
    public void JumpEnd()
    {
        if (shadowObject != null) shadowObject.SetActive(false);

        // Reactivar NavMeshAgent
        if (agent != null)
        {
            agent.enabled = true;
            agent.ResetPath();
        }

        isJumping = false;
    }

    IEnumerator MoveTo(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
    }
}