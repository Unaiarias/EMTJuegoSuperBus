using UnityEngine;
using System.Collections;

public class ObjetoEscudo : MonoBehaviour
{
    [Header("Configuración del Escudo")]
    [SerializeField] private float duracionEscudo = 8f; // Duración en segundos del escudo en la mano

    [Header("Referencias")]
    [SerializeField] private string tagJugador = "Player"; // Tag del jugador para detectar la colisión
    [SerializeField] private string nombreHandPoint = "HandPoint2"; // Nombre del GameObject vacío en la mano

    [Header("Particle Effects")]
    [SerializeField] private GameObject barreraParticlePrefab; // Prefab de partículas para la barrera
    [SerializeField] private Vector3 offsetParticulas = Vector3.zero; // Offset opcional para ajustar posición

    private bool recogido = false; // Para evitar que se recoja múltiples veces
    private Transform handPoint; // Referencia al punto de la mano del jugador
    private Transform jugadorTransform; // Referencia al transform del jugador
    private PlayerVida playerVida; // Referencia al script PlayerVida
    private PlayerAtaque playerAtaque; // Referencia al script PlayerAtaque para activar/desactivar isBarrera
    private Vector3 escalaOriginal; // Escala original del objeto
    private Collider objetoCollider; // Collider del objeto
    private GameObject efectoInstanciado; // Referencia al efecto de partículas instanciado
    private ParticleSystem efectoParticleSystem; // Referencia al ParticleSystem para controlar loop

    private void Start()
    {
        // Guardar la escala original y el collider
        escalaOriginal = transform.localScale;
        objetoCollider = GetComponent<Collider>();

        // Asegurarse de que el objeto tiene trigger activado
        if (objetoCollider != null)
        {
            objetoCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        // Si las partículas están activadas y tenemos referencia al jugador, actualizar posición
        if (efectoInstanciado != null && jugadorTransform != null)
        {
            efectoInstanciado.transform.position = jugadorTransform.position + offsetParticulas;
            efectoInstanciado.transform.rotation = jugadorTransform.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya fue recogido, ignorar
        if (recogido) return;

        // Verificar si el objeto que entra es el jugador
        if (other.CompareTag(tagJugador))
        {
            RecogerObjeto(other.gameObject);
        }
    }

    private void RecogerObjeto(GameObject jugador)
    {
        // Verificar si ya hay un objeto escudo en la mano
        playerVida = jugador.GetComponent<PlayerVida>();
        if (playerVida != null && playerVida.objetoEscudoActual != null)
        {
            Debug.Log("Ya tienes un escudo en la mano. No puedes recoger otro.");
            recogido = false; // No marcar como recogido
            return; // Salir del método sin recoger el objeto
        }

        recogido = true;

        // Guardar referencia al transform del jugador
        jugadorTransform = jugador.transform;

        // Obtener referencia al PlayerVida (ya lo tenemos arriba)
        if (playerVida == null)
        {
            Debug.LogError("No se encontró el componente PlayerVida en el jugador");
            Destroy(gameObject);
            return;
        }

        // Obtener referencia al PlayerAtaque (donde está isBarrera)
        playerAtaque = jugador.GetComponent<PlayerAtaque>();
        if (playerAtaque == null)
        {
            Debug.LogError("No se encontró el componente PlayerAtaque en el jugador");
            Destroy(gameObject);
            return;
        }

        // Buscar el punto de la mano (HandPoint) como hijo del jugador
        handPoint = jugador.transform.Find(nombreHandPoint);
        if (handPoint == null)
        {
            Debug.LogError($"No se encontró un GameObject llamado '{nombreHandPoint}' como hijo del jugador");
            Destroy(gameObject);
            return;
        }

        // Desactivar el collider para que no pueda volver a recogerse
        if (objetoCollider != null)
        {
            objetoCollider.enabled = false;
        }

        // Colocar el objeto en la mano del jugador
        transform.SetParent(handPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = escalaOriginal;

        // Guardar referencia en PlayerVida
        playerVida.objetoEscudoActual = this;

        // Activar el escudo (invulnerabilidad)
        ActivarEscudo(true);

        // ACTIVAR PARTÍCULAS DE BARRERA
        ActivarParticulasBarrera();

        // Iniciar la corrutina para desactivar el escudo después de X segundos
        StartCoroutine(DesactivarEscudoDespuesDeTiempo());

        Debug.Log($"¡Escudo recogido! Invulnerable durante {duracionEscudo} segundos");
    }

    private void ActivarEscudo(bool activar)
    {
        if (playerAtaque == null) return;

        // Usar la variable isBarrera del PlayerAtaque que ya tienes implementada
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
            // Instanciar el prefab de partículas en la posición del jugador
            efectoInstanciado = Instantiate(barreraParticlePrefab, jugadorTransform.position + offsetParticulas, jugadorTransform.rotation);

            // Obtener el ParticleSystem
            efectoParticleSystem = efectoInstanciado.GetComponent<ParticleSystem>();
            if (efectoParticleSystem != null)
            {
                // Configurar para que haga loop
                var main = efectoParticleSystem.main;
                main.loop = true;

                // Reproducir las partículas
                efectoParticleSystem.Play();
                Debug.Log("Partículas de barrera activadas");
            }
            else
            {
                // Buscar en los hijos si no está en el root
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
            // Detener la emisión de partículas
            if (efectoParticleSystem != null)
            {
                var emission = efectoParticleSystem.emission;
                emission.enabled = false; // Detener nueva emisión
                efectoParticleSystem.Stop(); // Detener el sistema

                // Destruir después de que las partículas existentes terminen
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
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(duracionEscudo);

        // Desactivar el escudo
        ActivarEscudo(false);

        // DESACTIVAR PARTÍCULAS DE BARRERA
        DesactivarParticulasBarrera();

        // Limpiar referencia en PlayerVida
        if (playerVida != null && playerVida.objetoEscudoActual == this)
        {
            playerVida.objetoEscudoActual = null;
        }

        // Destruir el objeto
        Destroy(gameObject);

        Debug.Log("Escudo desapareció después de " + duracionEscudo + " segundos");
    }

    // Si el objeto se destruye de otra forma (ej: el jugador muere), aseguramos desactivar el escudo
    private void OnDestroy()
    {
        if (recogido && playerAtaque != null)
        {
            ActivarEscudo(false);
        }

        // Asegurarse de desactivar partículas si el objeto se destruye
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