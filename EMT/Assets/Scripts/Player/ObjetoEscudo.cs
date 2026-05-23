using UnityEngine;
using System.Collections;

public class ObjetoEscudo : MonoBehaviour
{
    [Header("Configuración del Escudo")]
    [SerializeField] private float duracionEscudo = 8f;

    [Header("Referencias")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private string nombreHandPoint = "HandPoint2";

    [Header("Particle Effects")]
    [SerializeField] private GameObject barreraParticlePrefab;
    [SerializeField] private Vector3 offsetParticulas = Vector3.zero;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip sonidoEquipar;

    private bool recogido = false;
    private Transform handPoint;
    private Transform jugadorTransform;
    private PlayerVida playerVida;
    private PlayerAtaque playerAtaque;
    private Vector3 escalaOriginal;
    private Collider objetoCollider;
    private GameObject efectoInstanciado;
    private ParticleSystem efectoParticleSystem;
    private AudioSource audioSource;
    private Renderer objetoRenderer;

    private RotacionContinua rotacionContinua;

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

        if (playerVida != null && playerVida.objetoEscudoActual != null)
        {
            Debug.Log("Ya tienes un escudo en la mano. No puedes recoger otro.");
            recogido = false;
            return;
        }

        recogido = true;

        // AÑADIDO: Hacer invisible el objeto al recogerlo
        if (objetoRenderer != null)
        {
            objetoRenderer.enabled = false;
        }

        jugadorTransform = jugador.transform;

        if (playerVida == null)
        {
            Debug.LogError("No se encontró el componente PlayerVida en el jugador");
            Destroy(gameObject);
            return;
        }

        playerAtaque = jugador.GetComponent<PlayerAtaque>();
        if (playerAtaque == null)
        {
            Debug.LogError("No se encontró el componente PlayerAtaque en el jugador");
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

        playerVida.objetoEscudoActual = this;

        if (sonidoEquipar != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoEquipar);
            Debug.Log("?? Reproduciendo sonido de equipar escudo");
        }

        ActivarEscudo(true);
        ActivarParticulasBarrera();
        StartCoroutine(DesactivarEscudoDespuesDeTiempo());

        Debug.Log($"¡Escudo recogido! Invulnerable durante {duracionEscudo} segundos");
    }

    private void ActivarEscudo(bool activar)
    {
        if (playerAtaque == null) return;
        playerAtaque.isBarrera = activar;

        if (activar)
        {
            Debug.Log("ESCUDO ACTIVADO - Eres invulnerable");
        }
        else
        {
            Debug.Log("ESCUDO DESACTIVADO - Ya no eres invulnerable");
        }
    }

    private void ActivarParticulasBarrera()
    {
        if (barreraParticlePrefab != null && jugadorTransform != null)
        {
            efectoInstanciado = Instantiate(barreraParticlePrefab, jugadorTransform.position + offsetParticulas, jugadorTransform.rotation);

            efectoParticleSystem = efectoInstanciado.GetComponent<ParticleSystem>();
            if (efectoParticleSystem != null)
            {
                var main = efectoParticleSystem.main;
                main.loop = true;
                efectoParticleSystem.Play();
                Debug.Log("Partículas de barrera activadas");
            }
            else
            {
                efectoParticleSystem = efectoInstanciado.GetComponentInChildren<ParticleSystem>();
                if (efectoParticleSystem != null)
                {
                    var main = efectoParticleSystem.main;
                    main.loop = true;
                    efectoParticleSystem.Play();
                    Debug.Log("Partículas de barrera (hijo) activadas");
                }
                else
                {
                    Debug.LogWarning("El prefab de partículas no tiene componente ParticleSystem");
                }
            }
        }
        else
        {
            if (barreraParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para la barrera");
            if (jugadorTransform == null)
                Debug.LogWarning("No se tiene referencia al transform del jugador");
        }
    }

    private void DesactivarParticulasBarrera()
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
                Debug.Log("Partículas de barrera desactivadas");
            }
            else
            {
                Destroy(efectoInstanciado);
            }

            efectoInstanciado = null;
            efectoParticleSystem = null;
        }
    }

    private IEnumerator DesactivarEscudoDespuesDeTiempo()
    {
        yield return new WaitForSeconds(duracionEscudo);

        ActivarEscudo(false);
        DesactivarParticulasBarrera();

        if (playerVida != null && playerVida.objetoEscudoActual == this)
        {
            playerVida.objetoEscudoActual = null;
        }

        Destroy(gameObject);
        Debug.Log("Escudo desapareció después de " + duracionEscudo + " segundos");
    }

    private void OnDestroy()
    {
        if (recogido && playerAtaque != null)
        {
            ActivarEscudo(false);
        }

        if (recogido && efectoInstanciado != null)
        {
            Destroy(efectoInstanciado);
        }

        if (recogido && playerVida != null && playerVida.objetoEscudoActual == this)
        {
            playerVida.objetoEscudoActual = null;
        }
    }
}