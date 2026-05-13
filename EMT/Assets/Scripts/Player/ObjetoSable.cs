using UnityEngine;
using System.Collections;

public class ObjetoSable : MonoBehaviour
{
    [Header("Configuración del Sable")]
    [SerializeField] private float duracionSable = 8f;
    [SerializeField] private float multiplicadorDano = 2f;

    [Header("Referencias")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private string nombreHandPoint = "HandPoint2";

    [Header("Sound Effects")]
    [SerializeField] private AudioClip sonidoEquipar;

    private bool recogido = false;
    private Transform handPoint;
    private PlayerVida playerVida;
    private Vector3 escalaOriginal;
    private Collider objetoCollider;
    private AudioSource audioSource;

    public bool palo=false;

    public bool estaActivo { get; private set; } = false;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        objetoCollider = GetComponent<Collider>();

        if (objetoCollider != null)
        {
            objetoCollider.isTrigger = true;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoEquipar != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogido) return;

        if (other.CompareTag(tagJugador))
        {
            RecogerObjeto(other.gameObject);
        }
    }

    private void RecogerObjeto(GameObject jugador)
    {
        playerVida = jugador.GetComponent<PlayerVida>();

        if (playerVida != null && playerVida.objetoSableActual != null)
        {
            Debug.Log("Ya tienes un sable equipado");
            return;
        }

        recogido = true;
        estaActivo = true;

        if (playerVida == null)
        {
            Destroy(gameObject);
            return;
        }

        handPoint = jugador.transform.Find(nombreHandPoint);
        if (handPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        if (objetoCollider != null)
        {
            objetoCollider.enabled = false;
        }
        
        transform.SetParent(handPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = escalaOriginal;

        playerVida.ActualizarMultiplicadorDano(multiplicadorDano);
        playerVida.objetoSableActual = this;
        playerVida.paleando = true;

        if (sonidoEquipar != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoEquipar);
        }
        
        StartCoroutine(DesactivarSable());
        Debug.Log($"¡Sable equipado! Daño multiplicado x{multiplicadorDano} durante {duracionSable} segundos");
    }

    private IEnumerator DesactivarSable()
    {
       
        yield return new WaitForSeconds(duracionSable);
        
        if (playerVida != null)
        {
            playerVida.ActualizarMultiplicadorDano(1f);
            if (playerVida.objetoSableActual == this)
            {
                playerVida.objetoSableActual = null;
            }
        }

        estaActivo = false;
        palo = false;
        Destroy(gameObject);
        Debug.Log("Sable desapareció");
    }

    private void OnDestroy()
    {
        if (recogido && playerVida != null && playerVida.objetoSableActual == this)
        {
            palo = true;
            playerVida.ActualizarMultiplicadorDano(1f);
            playerVida.objetoSableActual = null;
            estaActivo = false;
        }
    }

    public bool EstaRecogido()
    {
        return recogido;
    }
}