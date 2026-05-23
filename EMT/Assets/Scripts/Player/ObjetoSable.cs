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

    [Header("Particle Effects")]
    [SerializeField] private GameObject sableParticlePrefab;
    [SerializeField] private Vector3 offsetParticulas = Vector3.zero;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip sonidoEquipar;

    private bool recogido = false;
    private Transform handPoint;
    private Transform jugadorTransform;
    private PlayerVida playerVida;
    private Vector3 escalaOriginal;
    private Collider objetoCollider;
    private AudioSource audioSource;
    private GameObject efectoInstanciado;
    private ParticleSystem efectoParticleSystem;
    private Renderer objetoRenderer; 

    private RotacionContinua rotacionContinua;

    public bool palo = false;
    public bool estaActivo { get; private set; } = false;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        objetoCollider = GetComponent<Collider>();
        objetoRenderer = GetComponent<Renderer>();
        rotacionContinua = GetComponent<RotacionContinua>();

        if (rotacionContinua == null)
        {
            rotacionContinua = gameObject.AddComponent<RotacionContinua>();
        }

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

    private void Update()
    {
        if (efectoInstanciado != null && jugadorTransform != null)
        {
            efectoInstanciado.transform.position = jugadorTransform.position + offsetParticulas;
            efectoInstanciado.transform.rotation = jugadorTransform.rotation;
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

        if (rotacionContinua != null)
        {
            rotacionContinua.DetenerRotacion();
        }

        if (playerVida != null && playerVida.objetoSableActual != null)
        {
            Debug.Log("Ya tienes un sable equipado");
            return;
        }

        recogido = true;
        estaActivo = true;

        if (objetoRenderer != null)
        {
            objetoRenderer.enabled = false;
        }

        jugadorTransform = jugador.transform;

        if (playerVida == null)
        {
            Destroy(gameObject);
            return;
        }

        handPoint = jugador.transform.Find(nombreHandPoint);
        if (handPoint == null)
        {
            Debug.LogError($"No se encontró un GameObject llamado '{nombreHandPoint}' como hijo del jugador");
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

        ActivarParticulasSable();

        StartCoroutine(DesactivarSable());
        Debug.Log($"¡Sable equipado! Daño multiplicado x{multiplicadorDano} durante {duracionSable} segundos");
    }

    private void ActivarParticulasSable()
    {
        if (sableParticlePrefab != null && jugadorTransform != null)
        {
            efectoInstanciado = Instantiate(sableParticlePrefab, jugadorTransform.position + offsetParticulas, jugadorTransform.rotation);

            efectoParticleSystem = efectoInstanciado.GetComponent<ParticleSystem>();
            if (efectoParticleSystem != null)
            {
                var main = efectoParticleSystem.main;
                main.loop = true;
                efectoParticleSystem.Play();
                Debug.Log("Partículas del sable activadas alrededor del jugador");
            }
            else
            {
                efectoParticleSystem = efectoInstanciado.GetComponentInChildren<ParticleSystem>();
                if (efectoParticleSystem != null)
                {
                    var main = efectoParticleSystem.main;
                    main.loop = true;
                    efectoParticleSystem.Play();
                    Debug.Log("Partículas del sable (hijo) activadas alrededor del jugador");
                }
                else
                {
                    Debug.LogWarning("El prefab de partículas no tiene componente ParticleSystem");
                }
            }
        }
        else
        {
            if (sableParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para el sable");
            if (jugadorTransform == null)
                Debug.LogWarning("No se tiene referencia al transform del jugador");
        }
    }

    private void DesactivarParticulasSable()
    {
        if (efectoInstanciado != null)
        {
            if (efectoParticleSystem != null)
            {
                var emission = efectoParticleSystem.emission;
                emission.enabled = false;
                efectoParticleSystem.Stop();
                float tiempoRestante = efectoParticleSystem.main.duration;
                Destroy(efectoInstanciado, tiempoRestante);
                Debug.Log("Partículas del sable desactivadas");
            }
            else
            {
                Destroy(efectoInstanciado);
            }

            efectoInstanciado = null;
            efectoParticleSystem = null;
        }
    }

    private IEnumerator DesactivarSable()
    {
        yield return new WaitForSeconds(duracionSable);

        DesactivarParticulasSable();

        if (playerVida != null)
        {
            playerVida.ActualizarMultiplicadorDano(1f);
            if (playerVida.objetoSableActual == this)
            {
                playerVida.objetoSableActual = null;
            }
            playerVida.paleando = false;
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
            playerVida.paleando = false;
            estaActivo = false;
        }

        if (recogido && efectoInstanciado != null)
        {
            Destroy(efectoInstanciado);
        }
    }

    public bool EstaRecogido()
    {
        return recogido;
    }
}