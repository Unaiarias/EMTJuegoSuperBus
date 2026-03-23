using UnityEngine;
using System.Collections;

public class SpawnManagerObjetos : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    [SerializeField] private GameObject[] objetosEspeciales; // Array de objetos que pueden spawnear (sables, escudos, etc)
    [SerializeField] private float[] probabilidadesObjetos; // Probabilidades de spawn para cada objeto (deben sumar 100)
    [SerializeField] private Transform[] puntosDeSpawn; // Array de puntos donde pueden spawnear los objetos
    [SerializeField] private float intervaloSpawn = 15f; // Tiempo en segundos entre cada spawn
    [SerializeField] private float tiempoVidaObjeto = 7f; // Tiempo en segundos que el objeto permanece en la escena antes de ser destruido si no es recogido

    [Header("Configuración de Parpadeo")]
    [SerializeField] private float tiempoParpadeo = 3f; // Tiempo en segundos que el objeto parpadea antes de ser destruido
    [SerializeField] private float velocidadParpadeo = 0.2f; // Intervalo en segundos entre cada cambio de visibilidad durante el parpadeo

    [Header("Estado Actual")]
    private GameObject objetoActualEnEscena; // Referencia al objeto actualmente spawneado en la escena (si hay alguno)
    private bool spawnActivo = true; // Controla si el spawn está activo o no

    private void Start()
    {
        // Validar que las probabilidades sumen 100
        StartCoroutine(CicloDeSpawn());
    }

    private IEnumerator CicloDeSpawn()
    {
        while (spawnActivo)
        {
            yield return new WaitForSeconds(intervaloSpawn);
            SpawnearObjeto();
        }
    }
    
    private void SpawnearObjeto()
    {
        GameObject objetoSeleccionado = SeleccionarObjetoPorProbabilidad();
        Transform puntoSpawn = puntosDeSpawn[Random.Range(0, puntosDeSpawn.Length)];

        GameObject nuevoObjeto = Instantiate(objetoSeleccionado, puntoSpawn.position, Quaternion.identity);
        objetoActualEnEscena = nuevoObjeto;

        StartCoroutine(VerificarDestruccionObjeto(nuevoObjeto));
    }

    private GameObject SeleccionarObjetoPorProbabilidad()
    {
        float randomValue = Random.Range(0f, 100f);
        float acumulador = 0f;

        for (int i = 0; i < probabilidadesObjetos.Length; i++)
        {
            acumulador += probabilidadesObjetos[i];
            if (randomValue <= acumulador)
                return objetosEspeciales[i];
        }

        return objetosEspeciales[objetosEspeciales.Length - 1];
    }

    private IEnumerator VerificarDestruccionObjeto(GameObject objeto)
    {
        float tiempoParpadeoReal = Mathf.Min(tiempoParpadeo, tiempoVidaObjeto);
        float tiempoAntesDeParpadear = tiempoVidaObjeto - tiempoParpadeoReal;

        yield return new WaitForSeconds(tiempoAntesDeParpadear);

        if (objeto != null && !VerificarSiFueRecogido(objeto))
        {
            // Iniciar parpadeo
            Coroutine parpadeo = StartCoroutine(ParpadearObjeto(objeto, tiempoParpadeoReal));

            yield return new WaitForSeconds(tiempoParpadeoReal);

            if (objeto != null && !VerificarSiFueRecogido(objeto))
            {
                Destroy(objeto);
                if (objetoActualEnEscena == objeto)
                    objetoActualEnEscena = null;
            }

            StopCoroutine(parpadeo);
        }
    }

    private IEnumerator ParpadearObjeto(GameObject objeto, float duracion)
    {
        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>();
        float tiempoInicio = Time.time;

        while (Time.time < tiempoInicio + duracion)
        {
            if (VerificarSiFueRecogido(objeto))
                yield break;

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = !renderer.enabled;
            }

            yield return new WaitForSeconds(velocidadParpadeo);
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }
    }

    private bool VerificarSiFueRecogido(GameObject objeto)
    {
        if (objeto == null) return true;

        ObjetoSable objetoSable = objeto.GetComponent<ObjetoSable>();
        if (objetoSable != null)
            return objetoSable.EstaRecogido();

        ObjetoEscudo objetoEscudo = objeto.GetComponent<ObjetoEscudo>();
        if (objetoEscudo != null)
        {
            Collider collider = objeto.GetComponent<Collider>();
            return collider != null && !collider.enabled;
        }

        return false;
    }

    // Métodos públicos
    public void SpawnearObjetoManual() => SpawnearObjeto();
    public void DetenerSpawn() => spawnActivo = false;
    public void ReanudarSpawn()
    {
        if (!spawnActivo)
        {
            spawnActivo = true;
            StartCoroutine(CicloDeSpawn());
        }
    }

    public void LimpiarObjetoActual()
    {
        if (objetoActualEnEscena != null)
        {
            Destroy(objetoActualEnEscena);
            objetoActualEnEscena = null;
        }
    }

    // Dibujar gizmos para visualizar los puntos de spawn
    private void OnDrawGizmosSelected()
    {
        if (puntosDeSpawn != null)
        {
            Gizmos.color = Color.green;

            foreach (Transform punto in puntosDeSpawn)
            {
                if (punto != null)
                {
                    Gizmos.DrawSphere(punto.position, 0.5f);
                    Gizmos.DrawIcon(punto.position, "GameObject Icon", true);
                }
            }
        }
    }
}