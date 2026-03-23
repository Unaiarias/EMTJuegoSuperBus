using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerVida : MonoBehaviour
{
    [Header("Valores Vida")]
    public float vidaActualPlayer; //Vida actual player
    public float vidaPlayerMaxima = 100f; //Vida maxima que puede tener el player

    [Header("Interfaz Vida")]
    public Image barraSalud; // Referencia a la barra de salud en la UI
    public TextMeshProUGUI vidaText; // Referencia al texto que muestra la vida actual

    [Header("Regeneración de Vida")]
    public float tiempoParaRegenerar = 5f; // Tiempo sin recibir daño para empezar a regenerar
    public float cantidadRegeneracionPorSegundo = 20f; // Cantidad de vida que se regenera por segundo
    private float temporizadorRegeneracion = 0f; // Temporizador para controlar el tiempo sin recibir daño
    private bool estaRegenerando = false; // Bandera para saber si el player está regenerando vida
    private Coroutine corutinaRegeneracion; // Referencia a la corrutina de regeneración para poder detenerla si el player recibe daño nuevamente

    [Header("Combo")]
    public TextMeshProUGUI comboText; // Referencia al texto que muestra el combo actual
    private int comboCount = 0; //Contador de combo del player

    [Header("Sistema de Daño por Combo")]
    [Tooltip("Cada cuántos golpes aumenta el daño")]
    public int golpesPorNivel = 5; // Cada 5 golpes sube de nivel
    [Tooltip("Daño base del jugador")]
    public int danoBase = 30; // Daño base
    public int danoBomba = 100; // Daño bomba
    [Tooltip("Daño adicional por cada nivel de combo")]
    public int danoExtraPorNivel = 10; // +10 de daño por cada nivel de combo
    private int nivelCombo = 0; // Nivel actual de combo (0 = sin bonus)

    [Header("Multiplicadores de Daño")]
    [Tooltip("Multiplicador global de daño (1 = normal, 1.5 = 50% más, etc)")]
    private float multiplicadorDanoGlobal = 1f; // Multiplicador por objetos especiales
    
    [Header("Objetos Especiales")]
    public ObjetoEscudo objetoEscudoActual; // Referencia al escudo actual en la mano
    public ObjetoSable objetoSableActual; // Referencia al objeto actual en la mano

    // Propiedad pública para obtener el daño actual basado en el combo y multiplicadores
    public int DanoActual
    {
        get
        {
            float danoCalculado = danoBase + (nivelCombo * danoExtraPorNivel);
            danoCalculado *= multiplicadorDanoGlobal;
            return Mathf.RoundToInt(danoCalculado); // Redondear para que sea int
        }
    }

    [Header("Tiempo de Combo")]
    public float tiempoMaximoSinAtaque = 5f; // Tiempo máximo sin atacar para reiniciar el combo
    private float temporizadorCombo = 0f; // Temporizador para controlar el tiempo sin atacar
    private bool temporizadorComboActivo = false; // Bandera para saber si el temporizador está activo

    private void Start()
    {
        // Inicializar la vida al máximo al empezar
        vidaActualPlayer = vidaPlayerMaxima;

        // Inicializar el texto del combo
        ActualizarTextoCombo();
    }

    private void Update()
    {
        ActualizarInterfazVida(); // Actualizar la interfaz de vida cada frame
        ActualizarTemporizadorCombo(); // Actualizar el temporizador de combo cada frame
    }

    void ActualizarTemporizadorCombo()
    {
        // Actualizar el temporizador de combo si está activo
        if (temporizadorComboActivo)
        {
            temporizadorCombo += Time.deltaTime;

            // Si se supera el tiempo máximo sin atacar, reiniciar combo
            if (temporizadorCombo >= tiempoMaximoSinAtaque)
            {
                ReiniciarCombo();
            }
        }
    }

    void ActualizarInterfazVida()
    {
        barraSalud.fillAmount = vidaActualPlayer / vidaPlayerMaxima; // Actualizar la barra de salud
        //vidaText.text = vidaActualPlayer.ToString("F0"); // Actualizar el texto de vida
    }

    public void RecibirDanoPlayer(int cantidadDano)
    {
        // Si el escudo está activo, no recibir daño
        PlayerAtaque playerAtaque = GetComponent<PlayerAtaque>();
        if (playerAtaque != null && playerAtaque.isBarrera)
        {
            Debug.Log("¡Daño bloqueado por el escudo!");
            return; // Salir del método sin aplicar daño
        }

        vidaActualPlayer -= cantidadDano; //Se va restando la vida
        ReiniciarCombo(); //Reiniciar el contador de combo del player al recibir daño

        // Detener la regeneración actual si está en curso
        if (corutinaRegeneracion != null)
        {
            StopCoroutine(corutinaRegeneracion); // Detener la corrutina de regeneración si el player recibe daño nuevamente
            estaRegenerando = false; // Reiniciar la bandera de regeneración
        }

        // Reiniciar el temporizador de regeneración
        temporizadorRegeneracion = 0f;

        // Iniciar la corrutina de espera para regeneración
        corutinaRegeneracion = StartCoroutine(EsperarParaRegenerar());

        if (vidaActualPlayer <= 0)
        {
            MorirPlayer(); //Llamar al método de morir del player si la vida llega a 0 o menos
        }
    }

    private IEnumerator EsperarParaRegenerar()
    {
        // Esperar el tiempo especificado sin recibir daño
        while (temporizadorRegeneracion < tiempoParaRegenerar)
        {
            // Si la vida ya está al máximo, no necesitamos regenerar
            if (vidaActualPlayer >= vidaPlayerMaxima)
            {
                yield break;
            }

            temporizadorRegeneracion += Time.deltaTime;
            yield return null;
        }

        // Si llegamos aquí, ha pasado el tiempo sin recibir daño
        if (vidaActualPlayer < vidaPlayerMaxima)
        {
            // Iniciar la regeneración progresiva
            corutinaRegeneracion = StartCoroutine(RegenerarVida());
        }
    }

    private IEnumerator RegenerarVida()
    {
        estaRegenerando = true;

        // Regenerar vida hasta llegar al máximo
        while (vidaActualPlayer < vidaPlayerMaxima)
        {
            // Aumentar la vida progresivamente
            vidaActualPlayer += cantidadRegeneracionPorSegundo * Time.deltaTime;

            // Asegurar que no nos pasamos del máximo
            if (vidaActualPlayer > vidaPlayerMaxima)
            {
                vidaActualPlayer = vidaPlayerMaxima;
            }

            // Actualizar la interfaz
            ActualizarInterfazVida();

            yield return null; // Esperar un frame
        }

        estaRegenerando = false;
        temporizadorRegeneracion = 0f;
    }

    // Método para aumentar el combo del player, se llama desde el script del enemigo al recibir daño
    public void AumentarCombo()
    {
        comboCount++;

        // Calcular el nivel actual de combo (cada X golpes = 1 nivel)
        int nuevoNivel = comboCount / golpesPorNivel;

        // Si el nivel ha cambiado, actualizar
        if (nuevoNivel != nivelCombo)
        {
            nivelCombo = nuevoNivel;
            Debug.Log($"¡Nivel de combo {nivelCombo}! Daño actual: {DanoActual}");
        }

        ActualizarTextoCombo();

        // Reiniciar el temporizador de combo cada vez que se ataca
        ReiniciarTemporizadorCombo();
    }

    // Método para actualizar el texto del combo (solo con el formato "x{comboCount}")
    private void ActualizarTextoCombo()
    {
        if (comboText != null)
        {
            comboText.text = $"x{comboCount}";
        }
    }

    // Método para reiniciar el temporizador de combo
    private void ReiniciarTemporizadorCombo()
    {
        temporizadorCombo = 0f;
        temporizadorComboActivo = true;
    }

    // Método para reiniciar completamente el combo
    private void ReiniciarCombo()
    {
        if (comboCount > 0)
        {
            comboCount = 0;
            nivelCombo = 0;
            ActualizarTextoCombo();
            Debug.Log("Combo reiniciado por tiempo sin atacar o por recibir daño");
        }

        temporizadorCombo = 0f;
        temporizadorComboActivo = false; // Desactivar el temporizador cuando el combo es 0
    }

    // Método para actualizar el multiplicador de daño (llamado desde ObjetoSable)
    public void ActualizarMultiplicadorDano(float nuevoMultiplicador)
    {
        multiplicadorDanoGlobal = nuevoMultiplicador;
        Debug.Log($"Multiplicador de daño actualizado a x{multiplicadorDanoGlobal}. Daño actual: {DanoActual}");
    }

    public void MorirPlayer()
    {
        Destroy(this.gameObject);
        Debug.Log("Player muerto");

        //Cuando se llame al metodo la escena se volverá a cargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}