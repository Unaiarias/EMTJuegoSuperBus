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

    // Variables privadas
    private int parrafoActual = 0;
    private bool estaEscribiendo = false;
    private bool parrafoCompletado = false;
    private Coroutine corrutinaEscritura;
    private Coroutine corrutinaParpadeo;
    private bool dialogoTerminado = false;

    // Evento que se dispara cuando el diálogo termina
    public System.Action OnDialogoTerminado;

    void Awake()
    {
        // Asegurar que el texto "Enter para continuar" empiece desactivado
        if (textoEnter != null)
        {
            textoEnter.gameObject.SetActive(false);
            textoEnter.alpha = 1f;
        }

        // Limpiar texto al inicio
        if (textoDialogo != null)
            textoDialogo.text = "";
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

        Debug.Log($"DialogoManager inicializado con {parrafos.Length} párrafos. Esperando inicio...");
    }

    // Método para iniciar el diálogo
    public void IniciarDialogo()
    {
        Debug.Log("=== INICIANDO DIÁLOGO ===");

        // Reiniciar todo
        parrafoActual = 0;
        parrafoCompletado = false;
        estaEscribiendo = false;
        dialogoTerminado = false;

        if (textoDialogo != null)
            textoDialogo.text = "";

        OcultarTextoEnter();

        // Empezar con el primer párrafo
        EmpezarParrafo(0);
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
                // Es el último párrafo -> finalizar diálogo
                FinalizarDialogo();
            }
            else
            {
                // No es el último -> pasar al siguiente párrafo
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
        }
    }

    private void SiguienteParrafo()
    {
        if (!parrafoCompletado || estaEscribiendo)
            return;

        // Si es el último párrafo, no hacer nada (se maneja en FinalizarDialogo)
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
        Debug.Log("=== DIÁLOGO FINALIZADO ===");

        // Ocultar el texto "Enter para continuar"
        OcultarTextoEnter();

        // Disparar el evento para que MenuInicioPaneles sepa que terminó
        OnDialogoTerminado?.Invoke();
    }

    // ===== MÉTODOS PARA EL TEXTO "ENTER PARA CONTINUAR" =====

    private void MostrarTextoEnter()
    {
        if (textoEnter != null)
        {
            textoEnter.gameObject.SetActive(true);
            textoEnter.alpha = 1f;

            DetenerParpadeo();
            IniciarParpadeo();

            Debug.Log($"Texto 'Enter para continuar' mostrado");
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
            textoEnter.alpha = 1f;
            yield return new WaitForSeconds(tiempoEspera);
            textoEnter.alpha = 0f;
            yield return new WaitForSeconds(tiempoEspera);
        }
    }

    // Método para reiniciar el diálogo
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

        if (textoDialogo != null)
            textoDialogo.text = "";

        Debug.Log("Diálogo reiniciado");
    }
}