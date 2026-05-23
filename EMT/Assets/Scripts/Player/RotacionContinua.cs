using UnityEngine;

public class RotacionContinua : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [SerializeField] private float velocidadRotacion = 180f; // Grados por segundo
    [SerializeField] private Vector3 ejeRotacion = Vector3.up; // Por defecto rota en Y

    private bool rotacionActiva = true;

    void Update()
    {
        if (rotacionActiva)
        {
            transform.Rotate(ejeRotacion * velocidadRotacion * Time.deltaTime);
        }
    }

    public void DetenerRotacion()
    {
        rotacionActiva = false;
    }

    public void ActivarRotacion()
    {
        rotacionActiva = true;
    }
}