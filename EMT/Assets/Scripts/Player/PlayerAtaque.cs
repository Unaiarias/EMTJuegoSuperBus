using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerAtaque : MonoBehaviour
{
    [Header("Ataque Settings")]
    [SerializeField] private GameObject cuboAtaque; // El cubo que está dentro del player
    [SerializeField] private float duracionAtaque = 0.2f; // Tiempo que permanece activo

    [Header("Supers Settings")]
    [SerializeField] private GameObject cuboExplosion;
    [SerializeField] private GameObject cuboBarrera;
    [SerializeField] private float duracionSuper= 0.5f;

    public InputActionReference triggerAtaque;
    public InputActionReference triggerSuperExplosion;
    public InputActionReference triggerSuperBarrera;

    private bool atacando = false;
    public bool isBarrera = false;

    public TextMeshProUGUI timerText; // Referencia al TextMeshPro para mostrar el tiempo
    private float timer = 0f;
    public float maxTiempo= 20f;

    private void Awake()
    {
        // Asegurarse de que el cubo empieza desactivado
        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(false);
        }
        else
        {
            Debug.LogError("No se ha asignado el cubo de ataque en el inspector");
        }
    }

    public void FixedUpdate()
    {
        // Solo incrementar el timer si aún no ha alcanzado el máximo
        if (timer < maxTiempo)
        {
            timer += Time.fixedDeltaTime;

            // Asegurar que no nos pasamos del máximo
            if (timer > maxTiempo)
            {
                timer = maxTiempo;
            }
        }

        // Actualizar el texto del timer
        timerText.text = "Tiempo Habilidad: " + timer.ToString("F0");

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
        // Prevenir múltiples ataques a la vez
        if (atacando) return;

        Debug.Log("Ataque presionado");
        StartCoroutine(MostrarCuboAtaque());
    }

    private void OnTriggerPressedSuperExplosion(InputAction.CallbackContext context)
    {
        if (timer >= maxTiempo)
        {
            if (atacando) return;
            Debug.Log("Super Explosion presionado");
            StartCoroutine(MostrarCuboExplosion());
            timer = 0f;   
        }
        
    }

    private void OnTriggerPressedSuperBarrera(InputAction.CallbackContext context)
    { 
        if (timer>= maxTiempo)
        {
            if (isBarrera) return;
            Debug.Log("Super Barrera presionado");
            StartCoroutine(MostrarCuboBarrera());
            timer = 0f;
        }
    }


    private System.Collections.IEnumerator MostrarCuboAtaque()
    {
        atacando = true;

        // Activar el cubo
        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(true);
            Debug.Log("Cubo de ataque ACTIVADO");
        }

        // Esperar el tiempo de duración
        yield return new WaitForSeconds(duracionAtaque);

        // Desactivar el cubo
        if (cuboAtaque != null)
        {
            cuboAtaque.SetActive(false);
            Debug.Log("Cubo de ataque DESACTIVADO");
        }

        atacando = false;
    }

    private System.Collections.IEnumerator MostrarCuboExplosion()
    {
        atacando = true;

        // Activar el cubo
        if (cuboExplosion != null)
        {
            cuboExplosion.SetActive(true);
            Debug.Log("Cubo de expl ACTIVADO");
        }

        // Esperar el tiempo de duración
        yield return new WaitForSeconds(duracionAtaque);

        // Desactivar el cubo
        if (cuboExplosion != null)
        {
            cuboExplosion.SetActive(false);
            Debug.Log("Cubo de expl DESACTIVADO");
        }

        atacando = false;
    }

    private System.Collections.IEnumerator MostrarCuboBarrera()
    {
        isBarrera = true;

        // Activar el cubo
        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(true);
            Debug.Log("Cubo de barrera ACTIVADO");
        }

        // Esperar el tiempo de duración
        yield return new WaitForSeconds(duracionSuper);

        // Desactivar el cubo
        if (cuboBarrera != null)
        {
            cuboBarrera.SetActive(false);
            Debug.Log("Cubo de barrera DESACTIVADO");
        }

        isBarrera = false;
    }
}