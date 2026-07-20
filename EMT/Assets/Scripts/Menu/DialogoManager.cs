using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogoManager : MonoBehaviour
{
    [Header("Configuración de Texto")]
    [TextArea(3, 10)]
    [SerializeField] private string[] parrafos;
    [SerializeField] private TextMeshProUGUI textoDialogo;
    [SerializeField] private float velocidadEscritura = 0.05f;

    [Header("Input - Asignar desde Input Actions")]
    [SerializeField] private InputActionReference inputContinuar;

    [Header("Texto Enter para continuar")]
    [SerializeField] private TextMeshProUGUI textoEnter;
    [SerializeField] private float velocidadParpadeo = 0.5f;

    [Header("Botón Continuar")]
    [SerializeField] private GameObject botonContinuar;

    // Variables privadas
    private int parrafoActual = 0;
    private bool estaEscribiendo = false;
    private bool parrafoCompletado = false;
    private Coroutine corrutinaEscritura;
    private Coroutine corrutinaParpadeo;
    private bool dialogoTerminado = false;

    void Awake()
    {
        // Asegurar que el texto "Enter para continuar" empiece desactivado
        if (textoEnter != null)
        {
            textoEnter.gameObject.SetActive(false);
            textoEnter.alpha = 1f;
        }

        // El botón continuar empieza oculto
        if (botonContinuar != null)
            botonContinuar.SetActive(false);
    }

    void Start()
    {
        // Verificar que hay párrafos
        if (parrafos == null || parrafos.Length == 0)
        {
            Debug.LogError("No hay párrafos asignados en DialogoManager");
            return;
        }

        if (textoDialogo == null)
        {
            Debug.LogError("Texto Diálogo no asignado en DialogoManager");
            return;
        }

        // Limpiar texto
        textoDialogo.text = "";
        dialogoTerminado = false;

        // Empezar con el primer párrafo
        EmpezarParrafo(0);

        Debug.Log($"DialogoManager iniciado con {parrafos.Length} párrafos");
    }

    private void OnEnable()
    {
        // Suscribirse al evento del Input Action
        if (inputContinuar != null)
        {
            inputContinuar.action.performed += OnInputPerformed;
            inputContinuar.action.Enable();
            Debug.Log("Input Continuar habilitado");
        }
        else
        {
            Debug.LogWarning("Input Continuar no asignado en DialogoManager");
        }
    }

    private void OnDisable()
    {
        // Desuscribirse del evento del Input Action
        if (inputContinuar != null)
        {
            inputContinuar.action.performed -= OnInputPerformed;
            inputContinuar.action.Disable();
        }
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Input Continuar presionado");

        if (estaEscribiendo)
        {
            // Si está escribiendo, completar el párrafo instantáneamente
            MostrarParrafoCompleto();
        }
        else if (parrafoCompletado)
        {
            // Si el párrafo está completo
            if (parrafoActual >= parrafos.Length - 1)
            {
                // Es el último párrafo ? ejecutar la función de continuar
                FinalizarDialogo();
            }
            else
            {
                // No es el último ? pasar al siguiente párrafo
                SiguienteParrafo();
            }
        }
    }

    private void EmpezarParrafo(int indice)
    {
        if (indice >= parrafos.Length)
        {
            return;
        }

        parrafoActual = indice;
        parrafoCompletado = false;
        dialogoTerminado = false;
        textoDialogo.text = "";

        // Ocultar botón continuar mientras se escribe
        if (botonContinuar != null)
            botonContinuar.SetActive(false);

        // Ocultar texto "Enter para continuar" mientras se escribe
        OcultarTextoEnter();

        // Empezar la corrutina de escritura
        if (corrutinaEscritura != null)
            StopCoroutine(corrutinaEscritura);

        corrutinaEscritura = StartCoroutine(EscribirParrafo(parrafos[indice]));

        Debug.Log($"Empezando párrafo {indice + 1}/{parrafos.Length}");
    }

    private IEnumerator EscribirParrafo(string texto)
    {
        estaEscribiendo = true;
        textoDialogo.text = "";

        // Escribir carácter por carácter
        for (int i = 0; i < texto.Length; i++)
        {
            textoDialogo.text += texto[i];
            yield return new WaitForSeconds(velocidadEscritura);
        }

        // Terminó de escribir
        estaEscribiendo = false;
        parrafoCompletado = true;

        Debug.Log($"Párrafo {parrafoActual + 1} completado");

        // ===== Mostrar el texto "Enter para continuar" =====
        MostrarTextoEnter();

        // ===== Si es el último párrafo, mostrar el botón continuar =====
        if (parrafoActual >= parrafos.Length - 1)
        {
            if (botonContinuar != null)
            {
                botonContinuar.SetActive(true);
                Debug.Log("Botón Continuar mostrado (último párrafo)");
            }
        }
    }

    private void MostrarParrafoCompleto()
    {
        if (estaEscribiendo && corrutinaEscritura != null)
        {
            StopCoroutine(corrutinaEscritura);
            corrutinaEscritura = null;

            // Mostrar el texto completo del párrafo actual
            textoDialogo.text = parrafos[parrafoActual];

            estaEscribiendo = false;
            parrafoCompletado = true;

            Debug.Log($"Párrafo {parrafoActual + 1} completado (skip)");

            // ===== Mostrar el texto "Enter para continuar" =====
            MostrarTextoEnter();

            // ===== Si es el último párrafo, mostrar el botón continuar =====
            if (parrafoActual >= parrafos.Length - 1)
            {
                if (botonContinuar != null)
                {
                    botonContinuar.SetActive(true);
                    Debug.Log("Botón Continuar mostrado (último párrafo - skip)");
                }
            }
        }
    }

    private void SiguienteParrafo()
    {
        if (!parrafoCompletado || estaEscribiendo)
            return;

        // Si es el último párrafo, no hacer nada
        if (parrafoActual >= parrafos.Length - 1)
            return;

        Debug.Log($"Pasando al párrafo {parrafoActual + 2}");

        // Pasar al siguiente párrafo
        EmpezarParrafo(parrafoActual + 1);
    }

    private void FinalizarDialogo()
    {
        if (!parrafoCompletado || estaEscribiendo)
            return;

        if (dialogoTerminado)
            return;

        dialogoTerminado = true;
        Debug.Log("Finalizando diálogo - Ejecutando continuar");

        // Si tienes un botón continuar con su función, ejecutamos su OnClick
        if (botonContinuar != null && botonContinuar.activeInHierarchy)
        {
            var button = botonContinuar.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                Debug.Log("Función del botón Continuar ejecutada");
            }
            else
            {
                Debug.LogWarning("El botón continuar no tiene componente Button");
            }
        }
        else
        {
            Debug.LogWarning("El botón continuar no está disponible o activo");
        }
    }

    // ===== MÉTODOS PARA EL TEXTO "ENTER PARA CONTINUAR" =====

    private void MostrarTextoEnter()
    {
        if (textoEnter != null)
        {
            textoEnter.gameObject.SetActive(true);
            textoEnter.alpha = 1f;

            // Detener parpadeo anterior si existe
            DetenerParpadeo();

            // Iniciar nuevo parpadeo
            IniciarParpadeo();

            Debug.Log($"Texto 'Enter para continuar' mostrado - Activo: {textoEnter.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("textoEnter es null en MostrarTextoEnter");
        }
    }

    private void OcultarTextoEnter()
    {
        if (textoEnter != null)
        {
            DetenerParpadeo();
            textoEnter.gameObject.SetActive(false);
            textoEnter.alpha = 1f;
            Debug.Log("Texto 'Enter para continuar' ocultado");
        }
    }

    private void IniciarParpadeo()
    {
        if (textoEnter != null && textoEnter.gameObject.activeInHierarchy)
        {
            if (corrutinaParpadeo != null)
            {
                StopCoroutine(corrutinaParpadeo);
                corrutinaParpadeo = null;
            }

            corrutinaParpadeo = StartCoroutine(ParpadearTexto());
            Debug.Log("Parpadeo iniciado");
        }
        else
        {
            Debug.LogWarning("No se puede iniciar parpadeo: textoEnter no está activo o es null");
        }
    }

    private void DetenerParpadeo()
    {
        if (corrutinaParpadeo != null)
        {
            StopCoroutine(corrutinaParpadeo);
            corrutinaParpadeo = null;
        }
    }

    private IEnumerator ParpadearTexto()
    {
        float tiempoEspera = velocidadParpadeo;

        while (true)
        {
            // Aparecer
            textoEnter.alpha = 1f;
            yield return new WaitForSeconds(tiempoEspera);

            // Desaparecer
            textoEnter.alpha = 0f;
            yield return new WaitForSeconds(tiempoEspera);
        }
    }

    // Método para reiniciar el diálogo (opcional)
    public void ReiniciarDialogo()
    {
        parrafoActual = 0;
        parrafoCompletado = false;
        estaEscribiendo = false;
        dialogoTerminado = false;

        if (corrutinaEscritura != null)
        {
            StopCoroutine(corrutinaEscritura);
            corrutinaEscritura = null;
        }

        OcultarTextoEnter();

        if (botonContinuar != null)
            botonContinuar.SetActive(false);

        if (textoDialogo != null)
            textoDialogo.text = "";

        EmpezarParrafo(0);
        Debug.Log("Diálogo reiniciado");
    }

    // Método de prueba para forzar mostrar el texto
    public void ForzarMostrarEnter()
    {
        MostrarTextoEnter();
        Debug.Log("Texto Enter forzado a mostrar");
    }
}