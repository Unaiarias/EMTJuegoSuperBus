using UnityEngine;
using System.Collections;

public class SistemaOleadas : MonoBehaviour
{
    [Header("Config")]
    public GameObject[] enemigos;
    public Collider[] zonaSpawn;

    public int maxEnemigosEnPantalla = 5;
    public int maximoEnemigoNivel = 20;
    public float tiempoEntreSpawns = 0.5f;

    [Header("Info (solo lectura)")]
    public int enemigosVivos = 0;
    public int enemigosGenerados = 0;

    bool estaSpawneando = false;

    void Update()
    {
        // Si aún podemos generar más en el nivel y caben más en pantalla, intenta spawn
        if (!estaSpawneando &&
            enemigosGenerados < maximoEnemigoNivel &&
            enemigosVivos < maxEnemigosEnPantalla)
        {
            StartCoroutine(GenerarEnemigo());
        }
    }

    IEnumerator GenerarEnemigo()
    {
        estaSpawneando = true;

        // Seleccionar enemigo aleatorio
        GameObject enemigoSeleccionado = enemigos[Random.Range(0, enemigos.Length)];
        // Seleccionar zona de spawn aleatoria
        Collider zonaSeleccionada = zonaSpawn[Random.Range(0, zonaSpawn.Length)];

        // Posición aleatoria dentro de la zona
        Vector3 posicionSpawn = new Vector3(
            Random.Range(zonaSeleccionada.bounds.min.x, zonaSeleccionada.bounds.max.x),
            zonaSeleccionada.bounds.center.y,
            Random.Range(zonaSeleccionada.bounds.min.z, zonaSeleccionada.bounds.max.z)
        );

        // Instanciar
        GameObject enemigo = Instantiate(enemigoSeleccionado, posicionSpawn, Quaternion.identity);

        // Avisarle al enemigo quién es el spawner para que notifique al morir
        Enemigo notificador = enemigo.GetComponent<Enemigo>();
        if (notificador != null)
        {
            notificador.spawner = this;
        }

        enemigosGenerados++;
        enemigosVivos++;

        yield return new WaitForSeconds(tiempoEntreSpawns);
        estaSpawneando = false;
    }

    // Llamado por los enemigos cuando mueren
    public void NotificarMuerteEnemigo()
    {
        enemigosVivos = Mathf.Max(0, enemigosVivos - 1);
    }
}
