using UnityEngine;

public class CollDestruirEnemigo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Muerte")
        {
            Destroy(gameObject);
        }
    }
}
