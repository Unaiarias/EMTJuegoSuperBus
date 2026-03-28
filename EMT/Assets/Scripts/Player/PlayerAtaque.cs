using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;  

public class PlayerAtaque : MonoBehaviour
{
    [Header("Ataque Settings")]
    [SerializeField] private float rangoAtaque = 2f;
    [SerializeField] private float radioAtaque = 1.2f; // Radio del área de ataque
    [SerializeField] private float duracionAtaque = 0.2f;
    [SerializeField] private LayerMask capaEnemigos;
    [SerializeField] private Transform puntoAtaque;

    [Header("Visuales")]
    [SerializeField] private GameObject cuboAtaque; 
    //[SerializeField] private GameObject efectoAtaque; 

    [Header("Supers Settings")]
    [SerializeField] private GameObject cuboExplosion;
    [SerializeField] private GameObject cuboBarrera;
    [SerializeField] private float duracionSuper = 0.5f;
    [SerializeField] private float radioExplosion = 3f; // Radio de la explosión

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

    private void Awake()
    {
        playerVida = GetComponent<PlayerVida>();

        // Configurar punto de ataque
        if (puntoAtaque == null)
            puntoAtaque = transform;

        // Obtener cámara
        if (Camera.main != null)
            camara = Camera.main.transform;

        // Configurar visuales
        if (cuboAtaque != null)
            cuboAtaque.SetActive(false);

        if (cuboExplosion != null)
            cuboExplosion.SetActive(false);

        if (cuboBarrera != null)
            cuboBarrera.SetActive(false);
    }

    private void Update()
    {
        if (timer < maxTiempo)
        {
            timer += Time.deltaTime;
            if (timer > maxTiempo)
                timer = maxTiempo;
        }

        // Actualizar texto del timer
        if (timerText != null)
            timerText.text = "Tiempo Habilidad: " + timer.ToString("F0");
    

        // Obtener dirección de ataque hacia el mouse (para PC)
        if (Mouse.current != null && camara != null)
        {
            Ray ray = camara.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plano = new Plane(Vector3.up, transform.position);

            if (plano.Raycast(ray, out float distancia))
            {
                Vector3 puntoMundo = ray.GetPoint(distancia);
                direccionAtaque = (puntoMundo - transform.position).normalized;
                direccionAtaque.y = 0;

                // Si la dirección es válida, actualizar la rotación visual del cubo
                if (direccionAtaque != Vector3.zero && cuboAtaque != null && cuboAtaque.activeSelf)
                {
                    cuboAtaque.transform.position = puntoAtaque.position + direccionAtaque * (rangoAtaque / 2);
                    cuboAtaque.transform.rotation = Quaternion.LookRotation(direccionAtaque);
                }
            }
        }

        // Si no hay dirección válida, usar la dirección forward del jugador
        if (direccionAtaque == Vector3.zero)
        {
            direccionAtaque = transform.forward;
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
        // AHORA USA timer Y maxTiempo DEL PROPIO SCRIPT
        if (timer >= maxTiempo)
        {
            if (atacando) return;
            StartCoroutine(RealizarExplosion());
            timer = 0f;  // Reiniciar timer
        }
    }

    private void OnTriggerPressedSuperBarrera(InputAction.CallbackContext context)
    {
        // AHORA USA timer Y maxTiempo DEL PROPIO SCRIPT
        if (timer >= maxTiempo)
        {
            if (isBarrera) return;
            StartCoroutine(ActivarBarrera());
            timer = 0f;  // Reiniciar timer
        }
    }

    private IEnumerator RealizarAtaque()
    {
        atacando = true;

        // Activar visual del cubo
        if (cuboAtaque != null)
        {
            // Posicionar el cubo en la dirección del ataque
            cuboAtaque.transform.position = puntoAtaque.position + direccionAtaque * (rangoAtaque / 2);
            cuboAtaque.transform.rotation = Quaternion.LookRotation(direccionAtaque);
            cuboAtaque.transform.localScale = new Vector3(radioAtaque * 2, 1, rangoAtaque);
            cuboAtaque.SetActive(true);
            Debug.Log("Ataque en dirección: " + direccionAtaque);
        }

        // Ejecutar el daño instantáneamente
        EjecutarAtaque();

        // Esperar duración del ataque
        yield return new WaitForSeconds(duracionAtaque);

        // Desactivar visual
        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(false);
        }

        atacando = false;
    }

    private void EjecutarAtaque()
    {
        // Calcular el centro del área de ataque
        Vector3 centroAtaque = puntoAtaque.position + direccionAtaque * (rangoAtaque / 2);

        // Detectar enemigos en el área circular
        Collider[] enemigosGolpeados = Physics.OverlapSphere(centroAtaque, radioAtaque, capaEnemigos);

        Debug.Log($"Ataque detectó {enemigosGolpeados.Length} enemigos");

        // Aplicar daño a cada enemigo
        foreach (Collider enemigoCollider in enemigosGolpeados)
        {
            Enemigo enemigo = enemigoCollider.GetComponent<Enemigo>();
            if (enemigo != null && playerVida != null)
            {
                int dañoAplicado = playerVida.DanoActual;
                enemigo.RecibirDanoEnemigo(dañoAplicado);
                playerVida.AumentarCombo();

                Debug.Log($"Daño aplicado: {dañoAplicado} a {enemigo.name}");

                // Efecto de impacto
                //if (efectoAtaque != null)
                //{
                //    Instantiate(efectoAtaque, enemigoCollider.transform.position, Quaternion.identity);
                //}
            }
        }

        // Si se golpeó al menos un enemigo, activar efecto visual
        //if (enemigosGolpeados.Length > 0 && efectoAtaque != null)
        //{
        //    Instantiate(efectoAtaque, centroAtaque, Quaternion.identity);
        //}
    }

    private IEnumerator RealizarExplosion()
    {
        atacando = true;

        // Activar visual de explosión
        if (cuboExplosion != null)
        {
            cuboExplosion.transform.localScale = Vector3.one * radioExplosion;
            cuboExplosion.SetActive(true);
            Debug.Log("¡SUPER EXPLOSIÓN ACTIVADA!");
        }

        // Detectar todos los enemigos en el radio de explosión
        Collider[] enemigosGolpeados = Physics.OverlapSphere(transform.position, radioExplosion, capaEnemigos);

        foreach (Collider enemigoCollider in enemigosGolpeados)
        {
            Enemigo enemigo = enemigoCollider.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                // Daño fijo de la bomba
                enemigo.RecibirDanoEnemigo(100);
                Debug.Log($"Explosión golpeó a {enemigo.name}");
            }
        }

        Debug.Log($"Explosión golpeó {enemigosGolpeados.Length} enemigos");

        // Esperar duración
        yield return new WaitForSeconds(duracionSuper);

        // Desactivar visual
        if (cuboExplosion != null)
        {
            cuboExplosion.SetActive(false);
        }

        atacando = false;
    }

    private IEnumerator ActivarBarrera()
    {
        isBarrera = true;

        // Activar visual de barrera
        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(true);
            Debug.Log("¡BARRERA ACTIVADA!");
        }

        // Esperar duración
        yield return new WaitForSeconds(duracionSuper);

        // Desactivar visual
        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(false);
        }

        isBarrera = false;
    }

    // Método para dibujar gizmos en el editor (útil para ajustar rangos)
    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque != null)
        {
            // Dibujar área de ataque
            Gizmos.color = Color.red;
            Vector3 centroAtaque = puntoAtaque.position + (Application.isPlaying ? direccionAtaque : transform.forward) * (rangoAtaque / 2);
            Gizmos.DrawWireSphere(centroAtaque, radioAtaque);

            // Dibujar área de explosión
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radioExplosion);
        }
    }
}