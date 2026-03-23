using UnityEngine;

public class EnemyRetreat : MonoBehaviour
{
    public float retreatDistance = 3f;
    public float retreatSpeed = 4f;
    public float waitAfterRetreat = 1.5f;
    public Transform target;   // Jugador

    bool isRetreating;
    float retreatedAmount;
    float waitTimer;

    void Update()
    {
        if (!isRetreating || target == null) return;

        if (retreatedAmount < retreatDistance)
        {
            Vector3 dirAway = (transform.position - target.position).normalized;
            Vector3 move = dirAway * retreatSpeed * Time.deltaTime;

            transform.position += move;
            retreatedAmount += move.magnitude;
        }
        else
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAfterRetreat)
            {
                isRetreating = false; // aquí el behaviour tree puede pasar a otro estado
            }
        }
    }

    public void StartRetreat(Transform newTarget)
    {
        target = newTarget;
        isRetreating = true;
        retreatedAmount = 0f;
        waitTimer = 0f;
    }

    public bool IsRetreatFinished => !isRetreating;
}
