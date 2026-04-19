using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PlayerAtaque : MonoBehaviour
{
    [Header("Ataque Settings")]
    [SerializeField] private float rangoAtaque = 2f;
    [SerializeField] private float radioAtaque = 1.2f;
    [SerializeField] private float duracionAtaque = 0.2f;
    [SerializeField] private LayerMask capaEnemigos;
    [SerializeField] private Transform puntoAtaque;

    [Header("Visuales")]
    [SerializeField] private GameObject cuboAtaque;

    [Header("Particle Effects")]
    [SerializeField] private GameObject ataqueParticlePrefab;
    [SerializeField] private Transform ataqueSpawnPoint;
    [SerializeField] private GameObject explosionParticlePrefab;
    [SerializeField] private Transform explosionSpawnPoint;
    [SerializeField] private GameObject barreraParticlePrefab; // Prefab de partículas para la barrera (debe tener loop activado)
    [SerializeField] private Transform barreraSpawnPoint;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource; // Fuente de audio para reproducir sonidos
    [SerializeField] public AudioClip sonidoAtaque;
    [SerializeField] public AudioClip sonidoAtaqueHit;

    [Header("Supers Settings")]
    [SerializeField] private GameObject cuboExplosion;
    [SerializeField] private GameObject cuboBarrera;
    [SerializeField] private float duracionSuperExplosion = 0.5f; // Duración específica para la explosión
    [SerializeField] private float duracionSuperBarrera = 0.5f;   // Duración específica para la barrera
    [SerializeField] private float radioExplosion = 3f;

    [Header("Input")]
    public InputActionReference triggerAtaque;
    public InputActionReference triggerSuperExplosion;
    public InputActionReference triggerSuperBarrera;

    [Header("Timer Habilidades")]
    public TextMeshProUGUI timerText;
    public float timer = 0f;
    public float maxTiempo = 20f;

    private PlayerVida playerVida;
    private bool atacando = false;
    public bool isBarrera = false;
    private Vector3 direccionAtaque;
    private Transform camara;

    // Variables para la barrera
    private GameObject barreraParticleInstance; // Instancia activa de las partículas de la barrera

    private void Awake()
    {
        playerVida = GetComponent<PlayerVida>();

        if (puntoAtaque == null)
            puntoAtaque = transform;

        if (ataqueSpawnPoint == null)
            ataqueSpawnPoint = transform;
        if (explosionSpawnPoint == null)
            explosionSpawnPoint = transform;
        if (barreraSpawnPoint == null)
            barreraSpawnPoint = transform;

        if (Camera.main != null)
            camara = Camera.main.transform;

        if (cuboAtaque != null)
            cuboAtaque.SetActive(false);

        if (cuboExplosion != null)
            cuboExplosion.SetActive(false);

        if (cuboBarrera != null)
            cuboBarrera.SetActive(false);

        // Si no hay AudioSource asignado, intentar obtenerlo del mismo GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Si no existe, agregar uno
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void Update()
    {
        if (timer < maxTiempo)
        {
            timer += Time.deltaTime;
            if (timer > maxTiempo)
                timer = maxTiempo;
        }

        if (timerText != null)
            timerText.text = timer.ToString("F0");

        if (Mouse.current != null && camara != null)
        {
            Ray ray = camara.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plano = new Plane(Vector3.up, transform.position);

            if (plano.Raycast(ray, out float distancia))
            {
                Vector3 puntoMundo = ray.GetPoint(distancia);
                direccionAtaque = (puntoMundo - transform.position).normalized;
                direccionAtaque.y = 0;

                if (direccionAtaque != Vector3.zero && cuboAtaque != null && cuboAtaque.activeSelf)
                {
                    cuboAtaque.transform.position = puntoAtaque.position + direccionAtaque * (rangoAtaque / 2);
                    cuboAtaque.transform.rotation = Quaternion.LookRotation(direccionAtaque);
                }
            }
        }

        if (direccionAtaque == Vector3.zero)
        {
            direccionAtaque = transform.forward;
        }

        // Actualizar la posición de las partículas de la barrera si están activas
        if (isBarrera && barreraParticleInstance != null && barreraSpawnPoint != null)
        {
            barreraParticleInstance.transform.position = barreraSpawnPoint.position;
            barreraParticleInstance.transform.rotation = barreraSpawnPoint.rotation;
        }
    }

    private void OnEnable()
    {
        triggerAtaque.action.performed += OnTriggerPressedAtaque;
        triggerAtaque.action.Enable();

        triggerSuperExplosion.action.performed += OnTriggerPressedSuperExplosion;
        triggerSuperExplosion.action.Enable();

        triggerSuperBarrera.action.performed += OnTriggerPressedSuperBarrera;
        triggerSuperBarrera.action.Enable();
    }

    private void OnDisable()
    {
        triggerAtaque.action.performed -= OnTriggerPressedAtaque;
        triggerAtaque.action.Disable();

        triggerSuperExplosion.action.performed -= OnTriggerPressedSuperExplosion;
        triggerSuperExplosion.action.Disable();

        triggerSuperBarrera.action.performed -= OnTriggerPressedSuperBarrera;
        triggerSuperBarrera.action.Disable();
    }

    private void OnTriggerPressedAtaque(InputAction.CallbackContext context)
    {
        if (atacando) return;
        StartCoroutine(RealizarAtaque());
    }

    private void OnTriggerPressedSuperExplosion(InputAction.CallbackContext context)
    {
        if (timer >= maxTiempo)
        {
            if (atacando) return;
            StartCoroutine(RealizarExplosion());
            timer = 0f;
        }
    }

    private void OnTriggerPressedSuperBarrera(InputAction.CallbackContext context)
    {
        if (timer >= maxTiempo)
        {
            if (isBarrera) return;
            StartCoroutine(ActivarBarrera());
            timer = 0f;
        }
    }

    private IEnumerator RealizarAtaque()
    {
        atacando = true;

        // Reproducir sonido de ataque
        ReproducirSonidoAtaque();

        if (cuboAtaque != null)
        {
            cuboAtaque.transform.position = puntoAtaque.position + direccionAtaque * (rangoAtaque / 2);
            cuboAtaque.transform.rotation = Quaternion.LookRotation(direccionAtaque);
            cuboAtaque.transform.localScale = new Vector3(radioAtaque * 2, 1, rangoAtaque);
            cuboAtaque.SetActive(true);
            Debug.Log("Ataque en dirección: " + direccionAtaque);
        }

        EjecutarAtaque();
        SpawnParticle(ataqueParticlePrefab, ataqueSpawnPoint, false); // false = no loop

        yield return new WaitForSeconds(duracionAtaque);

        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(false);
        }

        atacando = false;
    }

    private void EjecutarAtaque()
    {
        Vector3 centroAtaque = puntoAtaque.position + direccionAtaque * (rangoAtaque / 2);
        Collider[] enemigosGolpeados = Physics.OverlapSphere(centroAtaque, radioAtaque, capaEnemigos);

        Debug.Log($"Ataque detectó {enemigosGolpeados.Length} enemigos");

        bool impactoRealizado = false; // Variable para saber si hubo al menos un impacto

        foreach (Collider enemigoCollider in enemigosGolpeados)
        {
            Enemigo enemigo = enemigoCollider.GetComponent<Enemigo>();
            if (enemigo != null && playerVida != null)
            {
                int dañoAplicado = playerVida.DanoActual;
                enemigo.RecibirDanoEnemigo(dañoAplicado);
                playerVida.AumentarCombo();
                Debug.Log($"Daño aplicado: {dañoAplicado} a {enemigo.name}");
                impactoRealizado = true; // Marcamos que hubo impacto
            }
        }

        // Reproducir sonido de impacto si golpeó al menos a un enemigo
        if (impactoRealizado)
        {
            ReproducirSonidoAtaqueHit();
        }
    }

    // Método para reproducir el sonido de ataque
    private void ReproducirSonidoAtaque()
    {
        if (sonidoAtaque != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoAtaque);
            Debug.Log("Reproduciendo sonido de ataque");
        }
        else if (sonidoAtaque == null)
        {
            Debug.LogWarning("No se ha asignado el clip de sonido de ataque");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("No se ha asignado el AudioSource");
        }
    }

    // Método para reproducir el sonido de impacto al golpear enemigos
    private void ReproducirSonidoAtaqueHit()
    {
        if (sonidoAtaqueHit != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoAtaqueHit);
            Debug.Log("Reproduciendo sonido de impacto al enemigo");
        }
        else if (sonidoAtaqueHit == null)
        {
            Debug.LogWarning("No se ha asignado el clip de sonido de impacto (sonidoAtaqueHit)");
        }
    }

    //Explosion Habilidad

    public void BotonExplosionHabilidad()
    {
        if (timer >= maxTiempo)
        {
            Debug.Log("Botón de explosión presionado");
            if (atacando) return;
            StartCoroutine(RealizarExplosion());
            timer = 0f;
        }
    }

    public IEnumerator RealizarExplosion()
    {
        atacando = true;

        if (cuboExplosion != null)
        {
            cuboExplosion.transform.localScale = Vector3.one * radioExplosion;
            cuboExplosion.SetActive(true);
            Debug.Log("¡SUPER EXPLOSIÓN ACTIVADA!");
        }

        SpawnParticle(explosionParticlePrefab, explosionSpawnPoint, false);

        Collider[] enemigosGolpeados = Physics.OverlapSphere(transform.position, radioExplosion, capaEnemigos);

        foreach (Collider enemigoCollider in enemigosGolpeados)
        {
            Enemigo enemigo = enemigoCollider.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                enemigo.RecibirDanoEnemigo(100);
                Debug.Log($"Explosión golpeó a {enemigo.name}");
            }
        }

        Debug.Log($"Explosión golpeó {enemigosGolpeados.Length} enemigos");

        // Usar la duración específica de la explosión
        yield return new WaitForSeconds(duracionSuperExplosion);

        if (cuboExplosion != null)
        {
            cuboExplosion.SetActive(false);
        }

        atacando = false;
    }

    //Barrera Habilidad

    public void BotonBarreraHabilidad()
    {
        if (timer >= maxTiempo)
        {
            Debug.Log("Botón de Barrera presionado");
            if (isBarrera) return;
            StartCoroutine(ActivarBarrera());
            timer = 0f;
        }
    }

    public IEnumerator ActivarBarrera()
    {
        isBarrera = true;

        // Activar visual de cubo (opcional)
        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(true);
            Debug.Log("¡BARRERA ACTIVADA!");
        }

        // Instanciar partículas de barrera en loop
        StartBarreraParticles();

        // Usar la duración específica de la barrera
        yield return new WaitForSeconds(duracionSuperBarrera);

        // Detener y destruir las partículas de la barrera
        StopBarreraParticles();

        // Desactivar visual
        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(false);
        }

        isBarrera = false;
    }

    // Método para iniciar las partículas de la barrera (con loop)
    private void StartBarreraParticles()
    {
        if (barreraParticlePrefab != null && barreraSpawnPoint != null)
        {
            // Instanciar el prefab
            barreraParticleInstance = Instantiate(barreraParticlePrefab, barreraSpawnPoint.position, barreraSpawnPoint.rotation);

            // Asegurar que el Particle System esté en loop
            ParticleSystem ps = barreraParticleInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.loop = true; // Forzar loop activado
                ps.Play(); // Reproducir partículas
                Debug.Log("Partículas de barrera iniciadas en loop");
            }
            else
            {
                // Si no tiene ParticleSystem, buscar en los hijos
                ps = barreraParticleInstance.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.loop = true;
                    ps.Play();
                    Debug.Log("Partículas de barrera (hijo) iniciadas en loop");
                }
            }
        }
    }

    // Método para detener las partículas de la barrera
    private void StopBarreraParticles()
    {
        if (barreraParticleInstance != null)
        {
            // Detener la emisión de partículas
            ParticleSystem ps = barreraParticleInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.enabled = false; // Detener nueva emisión
                ps.Stop(); // Detener el sistema
            }
            else
            {
                ps = barreraParticleInstance.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                    ps.Stop();
                }
            }

            // Destruir el objeto después de que las partículas existentes desaparezcan
            Destroy(barreraParticleInstance, 2f);
            barreraParticleInstance = null;
            Debug.Log("Partículas de barrera detenidas");
        }
    }

    // Método genérico para instanciar partículas (para ataques normales y explosión)
    private void SpawnParticle(GameObject particlePrefab, Transform spawnPoint, bool loop = false)
    {
        if (particlePrefab != null && spawnPoint != null)
        {
            GameObject particleInstance = Instantiate(particlePrefab, spawnPoint.position, spawnPoint.rotation);

            if (loop)
            {
                // Para partículas con loop (como la barrera, pero usamos el método específico)
                ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.loop = true;
                    ps.Play();
                }
            }
            else
            {
                // Auto-destruir el efecto después de que termine (para ataques normales)
                ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    float duration = particleSystem.main.duration;
                    Destroy(particleInstance, duration + 0.5f);
                }
                else
                {
                    Destroy(particleInstance, 2f);
                }
            }
        }
    }

    // Métodos públicos para acceder a las duraciones de las habilidades
    public float GetDuracionSuperExplosion()
    {
        return duracionSuperExplosion;
    }

    public float GetDuracionSuperBarrera()
    {
        return duracionSuperBarrera;
    }

    // Métodos para modificar las duraciones en tiempo de ejecución
    public void SetDuracionSuperExplosion(float newDuration)
    {
        duracionSuperExplosion = Mathf.Max(0f, newDuration);
    }

    public void SetDuracionSuperBarrera(float newDuration)
    {
        duracionSuperBarrera = Mathf.Max(0f, newDuration);
    }

    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque != null)
        {
            Gizmos.color = Color.red;
            Vector3 centroAtaque = puntoAtaque.position + (Application.isPlaying ? direccionAtaque : transform.forward) * (rangoAtaque / 2);
            Gizmos.DrawWireSphere(centroAtaque, radioAtaque);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radioExplosion);
        }
    }
}