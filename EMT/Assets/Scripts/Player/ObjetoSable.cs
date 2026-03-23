using UnityEngine;
using System.Collections;

public class ObjetoSable : MonoBehaviour
{
    [Header("Configuración del Objeto")]
    [SerializeField] private float multiplicadorDano = 1.5f; // Multiplicador de daño (1.5 = 50% más daño)
    [SerializeField] private float duracionObjeto = 15f; // Duración en segundos del objeto en la mano

    [Header("Referencias")]
    [SerializeField] private string tagJugador = "Player"; // Tag del jugador para detectar la colisión
    [SerializeField] private string nombreHandPoint = "HandPointSable"; // Nombre del GameObject vacío en la mano

    private bool recogido = false; // Para evitar que se recoja múltiples veces
    private Transform handPoint; // Referencia al punto de la mano del jugador
    private PlayerVida playerVida; // Referencia al script PlayerVida
    private Vector3 escalaOriginal; // Escala original del objeto
    private Collider objetoCollider; // Collider del objeto

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
        // Verificar si ya hay un objeto sable en la mano
        playerVida = jugador.GetComponent<PlayerVida>();
        if (playerVida != null && playerVida.objetoSableActual != null)
        {
            Debug.Log("Ya tienes un objeto en la mano. No puedes recoger otro.");
            recogido = false; // No marcar como recogido
            return; // Salir del método sin recoger el objeto
        }

        recogido = true;

        // Obtener referencia al PlayerVida (ya lo tenemos arriba)
        if (playerVida == null)
        {
            Debug.LogError("No se encontró el componente PlayerVida en el jugador");
            Destroy(gameObject); // Destruir el objeto si hay error
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
        transform.SetParent(handPoint); // Hacer que el objeto sea hijo del HandPoint
        transform.localPosition = Vector3.zero; // Posición local (0,0,0) respecto al HandPoint
        transform.localRotation = Quaternion.identity; // Rotación local cero
        transform.localScale = escalaOriginal; // Mantener la escala original

        // Aplicar el multiplicador de daño
        AplicarMultiplicadorDano(true);

        // Iniciar la corrutina para destruir el objeto después de X segundos
        StartCoroutine(DestruirDespuesDeTiempo());

        Debug.Log($"¡Objeto recogido! Daño multiplicado x{multiplicadorDano} durante {duracionObjeto} segundos");
    }

    private void AplicarMultiplicadorDano(bool activar)
    {
        if (playerVida == null) return;

        if (activar)
        {
            // Guardar referencia del objeto actual y aplicar multiplicador
            playerVida.objetoSableActual = this;
            playerVida.ActualizarMultiplicadorDano(multiplicadorDano);
        }
        else
        {
            // Quitar referencia y restaurar multiplicador a 1
            if (playerVida.objetoSableActual == this)
            {
                playerVida.objetoSableActual = null;
                playerVida.ActualizarMultiplicadorDano(1f);
            }
        }
    }

    private IEnumerator DestruirDespuesDeTiempo()
    {
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(duracionObjeto);

        // Quitar el multiplicador de daño antes de destruir
        AplicarMultiplicadorDano(false);

        // Destruir el objeto
        Destroy(gameObject);

        Debug.Log("Objeto de daño desapareció después de " + duracionObjeto + " segundos");
    }

    // Método público para obtener el multiplicador (lo usará PlayerVida)
    public float GetMultiplicadorDano()
    {
        return recogido ? multiplicadorDano : 1f;
    }

    // Método para saber si el objeto está activo
    public bool EstaRecogido()
    {
        return recogido;
    }

    // Si el objeto se destruye de otra forma (ej: el jugador muere), aseguramos quitar el multiplicador
    private void OnDestroy()
    {
        if (recogido && playerVida != null)
        {
            // Asegurarse de quitar el multiplicador si este objeto era el activo
            if (playerVida.objetoSableActual == this)
            {
                playerVida.objetoSableActual = null;
                playerVida.ActualizarMultiplicadorDano(1f);
            }
        }
    }
}