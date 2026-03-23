using UnityEngine;

public class BalaScript : MonoBehaviour
{

    private float timer=0;
    private float tiempoDes = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < tiempoDes)
        {
            timer += Time.deltaTime;
        }
        if (timer >= tiempoDes)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }

        if (other.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }


}
