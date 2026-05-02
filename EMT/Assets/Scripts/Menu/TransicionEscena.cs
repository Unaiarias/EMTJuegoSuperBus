using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransicionEscena : MonoBehaviour
{
    public Canvas canvasTransicion;
    public Image imagenFondoNegro;
    public float duracionFundido = 1f;

    void Start()
    {
        // Asegurar que el canvas está activo y la imagen visible
        canvasTransicion.gameObject.SetActive(true);
        Color color = imagenFondoNegro.color;
        color.a = 1f; // Totalmente opaco (negro visible)
        imagenFondoNegro.color = color;

        // Iniciar la animación para que desaparezca
        StartCoroutine(FundidoEntrada());
    }

    IEnumerator FundidoEntrada()
    {
        float tiempoTranscurrido = 0f;
        Color color = imagenFondoNegro.color;

        while (tiempoTranscurrido < duracionFundido)
        {
            tiempoTranscurrido += Time.deltaTime;
            float alpha = 1f - (tiempoTranscurrido / duracionFundido);
            color.a = alpha;
            imagenFondoNegro.color = color;
            yield return null;
        }

        // Cuando termina el fundido:
        color.a = 0f;
        imagenFondoNegro.color = color;
        canvasTransicion.gameObject.SetActive(false); // Desactivar el canvas
    }
}