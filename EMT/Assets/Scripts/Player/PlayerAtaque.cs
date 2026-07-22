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
    [SerializeField] private GameObject barreraParticlePrefab;
    [SerializeField] private Transform barreraSpawnPoint;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] public AudioClip sonidoAtaqueNormal;
    [SerializeField] public AudioClip sonidoAtaqueSable;
    [SerializeField] public AudioClip sonidoAtaqueHitNormal;
    [SerializeField] public AudioClip sonidoAtaqueHitSable;
    [SerializeField] public AudioClip sonidoExplosionSuper; // AÑADIDO: Sonido para la explosión

    [Header("Supers Settings")]
    [SerializeField] private GameObject cuboExplosion;
    [SerializeField] private GameObject cuboBarrera;
    [SerializeField] private float duracionSuperExplosion = 0.5f;
    [SerializeField] private float duracionSuperBarrera = 0.5f;
    [SerializeField] private float radioExplosion = 3f;

    [Header("Input")]
    public InputActionReference triggerAtaque;
    public InputActionReference triggerSuperExplosion;
    public InputActionReference triggerSuperBarrera;

    [Header("Timer Habilidades")]
    public TextMeshProUGUI timerText;
    public float timer = 0f;
    public float maxTiempo = 20f;

    // Evento para notificar al cursor cuando se realiza un ataque
    public System.Action<bool> OnAtaqueRealizado; // true = impacto, false = fallo

    private PlayerVida playerVida;
    private bool atacando = false;
    public bool isBarrera = false;
    private Vector3 direccionAtaque;
    private Transform camara;

    public Animator an;

    private GameObject barreraParticleInstance;

    public ObjetoSable ObjetoSable;

    private Quaternion rotacionAtaque;
    private Quaternion rotacionOriginal;
    private bool restaurandoRotacion = false;
    private bool mirandoDireccionAtaque = false;
    private float temporizadorRotacionAtaque = 0f;
    private float duracionRotacionAtaque = 0.3f;

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

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void Update()
    {
        // Si el juego está en pausa, no actualizar la dirección del ataque ni la posición del cubo
        if (MenuPausa.IsGamePaused) return;

        // MANEJO DE LA ROTACIÓN DE ATAQUE
        if (mirandoDireccionAtaque)
        {
            // Mantener la rotación de ataque durante el tiempo establecido
            temporizadorRotacionAtaque -= Time.deltaTime;

            if (temporizadorRotacionAtaque <= 0)
            {
                // Terminó el tiempo de mirar hacia el ataque
                mirandoDireccionAtaque = false;
                Debug.Log("Fin de la rotación de ataque - el movimiento retomará el control");
            }
            else
            {
                // Forzar la rotación de ataque cada frame durante la duración
                transform.rotation = rotacionAtaque;
            }
        }

        // Resto de tu Update original (solo actualizamos la dirección del mouse, no la rotación del personaje)
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

    private bool TieneSableEquipado()
    {
        if (playerVida != null && playerVida.objetoSableActual != null)
        {
            return playerVida.objetoSableActual.estaActivo;
        }
        return false;
    }

    private void OnTriggerPressedAtaque(InputAction.CallbackContext context)
    {
        if (MenuPausa.IsGamePaused) return;
        if (atacando) return;

        StartCoroutine(RealizarAtaque());
    }

    private void OnTriggerPressedSuperExplosion(InputAction.CallbackContext context)
    {
        if (MenuPausa.IsGamePaused) return;
        if (timer >= maxTiempo)
        {
            if (atacando) return;
            StartCoroutine(RealizarExplosion());
            timer = 0f;
        }
    }

    private void OnTriggerPressedSuperBarrera(InputAction.CallbackContext context)
    {
        if (MenuPausa.IsGamePaused) return;
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

        // Guardar la dirección de ataque y activar el modo de mira forzada
        if (direccionAtaque != Vector3.zero)
        {
            rotacionAtaque = Quaternion.LookRotation(direccionAtaque);
            mirandoDireccionAtaque = true;
            temporizadorRotacionAtaque = duracionRotacionAtaque;

            // Aplicar la rotación inmediatamente
            transform.rotation = rotacionAtaque;
        }

        if (playerVida.paleando == true)
        {
            an.SetBool("IsPalo", true);
        }
        else
        {
            an.SetBool("IsAttack", true);
        }

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
        SpawnParticle(ataqueParticlePrefab, ataqueSpawnPoint, false);

        yield return new WaitForSeconds(duracionAtaque);

        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(false);
        }
        an.SetBool("IsPalo", false);
        an.SetBool("IsAttack", false);

        atacando = false;
    }

    private IEnumerator RestaurarRotacionDespuesDeAtaque()
    {
        restaurandoRotacion = true;

        // Esperar un pequeño frame para asegurar que la animación de ataque se vea
        yield return null;

        // Restaurar suavemente la rotación o inmediatamente
        // Si quieres que sea inmediato (como en Brawl Stars):
        //transform.rotation = rotacionOriginal;

        // Si prefieres que sea suave (descomenta las siguientes líneas y comenta la de arriba):
        
        float tiempoRestauracion = 0.1f;
        float elapsedTime = 0;
        Quaternion startRot = transform.rotation;

        while (elapsedTime < tiempoRestauracion)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / tiempoRestauracion;
            transform.rotation = Quaternion.Slerp(startRot, rotacionOriginal, t);
            yield return null;
        }
        transform.rotation = rotacionOriginal;
        

        restaurandoRotacion = false;
        atacando = false;
    }

    public bool EstaMirandoDireccionAtaque()
    {
        return mirandoDireccionAtaque;
    }

    private void EjecutarAtaque()
    {
        Vector3 centroAtaque = puntoAtaque.position + direccionAtaque * (rangoAtaque / 2);
        Collider[] enemigosGolpeados = Physics.OverlapSphere(centroAtaque, radioAtaque, capaEnemigos);

        Debug.Log($"Ataque detectó {enemigosGolpeados.Length} enemigos");

        bool impactoRealizado = false;

        foreach (Collider enemigoCollider in enemigosGolpeados)
        {
            Enemigo enemigo = enemigoCollider.GetComponent<Enemigo>();
            if (enemigo != null && playerVida != null)
            {
                int dañoAplicado = playerVida.DanoActual;
                enemigo.RecibirDanoEnemigo(dañoAplicado);
                Debug.Log($"Daño aplicado: {dañoAplicado} a {enemigo.name}");
                impactoRealizado = true;
            }
        }

        if (impactoRealizado)
        {
            ReproducirSonidoAtaqueHit();
        }

        // ===== DISPARAR EVENTO PARA EL CURSOR =====
        OnAtaqueRealizado?.Invoke(impactoRealizado);
        Debug.Log($"Evento de ataque disparado: {(impactoRealizado ? "IMPACTO" : "FALLO")}");
    }

    private void ReproducirSonidoAtaque()
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = null;

        if (TieneSableEquipado() && sonidoAtaqueSable != null)
        {
            clipToPlay = sonidoAtaqueSable;
            Debug.Log("?? Reproduciendo sonido de ataque con SABLE");
        }
        else if (sonidoAtaqueNormal != null)
        {
            clipToPlay = sonidoAtaqueNormal;
            Debug.Log("?? Reproduciendo sonido de ataque NORMAL");
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el clip de sonido de ataque correspondiente");
        }
    }

    private void ReproducirSonidoAtaqueHit()
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = null;

        if (TieneSableEquipado() && sonidoAtaqueHitSable != null)
        {
            clipToPlay = sonidoAtaqueHitSable;
            Debug.Log("?? Reproduciendo sonido de impacto con SABLE");
        }
        else if (sonidoAtaqueHitNormal != null)
        {
            clipToPlay = sonidoAtaqueHitNormal;
            Debug.Log("?? Reproduciendo sonido de impacto NORMAL");
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el clip de sonido de impacto correspondiente");
        }
    }

    // AÑADIDO: Método para reproducir sonido de explosión
    private void ReproducirSonidoExplosionSuper()
    {
        if (audioSource == null) return;

        if (sonidoExplosionSuper != null)
        {
            audioSource.PlayOneShot(sonidoExplosionSuper);
            Debug.Log("?? Reproduciendo sonido de explosión SUPER");
        }
        else
        {
            Debug.LogWarning("No se ha asignado el clip de sonido de explosión super");
        }
    }

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

        // AÑADIDO: Reproducir sonido de explosión
        ReproducirSonidoExplosionSuper();

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

        yield return new WaitForSeconds(duracionSuperExplosion);

        if (cuboExplosion != null)
        {
            cuboExplosion.SetActive(false);
        }

        atacando = false;
    }

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

        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(true);
            Debug.Log("¡BARRERA ACTIVADA!");
        }

        StartBarreraParticles();

        yield return new WaitForSeconds(duracionSuperBarrera);

        StopBarreraParticles();

        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(false);
        }

        isBarrera = false;
    }

    private void StartBarreraParticles()
    {
        if (barreraParticlePrefab != null && barreraSpawnPoint != null)
        {
            barreraParticleInstance = Instantiate(barreraParticlePrefab, barreraSpawnPoint.position, barreraSpawnPoint.rotation);

            ParticleSystem ps = barreraParticleInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.loop = true;
                ps.Play();
                Debug.Log("Partículas de barrera iniciadas en loop");
            }
            else
            {
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

    private void StopBarreraParticles()
    {
        if (barreraParticleInstance != null)
        {
            ParticleSystem ps = barreraParticleInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.enabled = false;
                ps.Stop();
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

            Destroy(barreraParticleInstance, 2f);
            barreraParticleInstance = null;
            Debug.Log("Partículas de barrera detenidas");
        }
    }

    private void SpawnParticle(GameObject particlePrefab, Transform spawnPoint, bool loop = false)
    {
        if (particlePrefab != null && spawnPoint != null)
        {
            GameObject particleInstance = Instantiate(particlePrefab, spawnPoint.position, spawnPoint.rotation);

            if (loop)
            {
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

    // Método para forzar la desactivación del cubo de ataque (llamado desde MenuPausa)
    public void ForzarDesactivarCuboAtaque()
    {
        if (cuboAtaque != null && cuboAtaque.activeSelf)
        {
            StopAllCoroutines(); // Detener cualquier ataque en curso
            cuboAtaque.SetActive(false);
            atacando = false;

            // Restaurar rotación si es necesario
            if (restaurandoRotacion)
            {
                transform.rotation = rotacionOriginal;
                restaurandoRotacion = false;
            }

            Debug.Log("Cubo de ataque desactivado por pausa");
        }
    }

    public float GetDuracionSuperExplosion()
    {
        return duracionSuperExplosion;
    }

    public float GetDuracionSuperBarrera()
    {
        return duracionSuperBarrera;
    }

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