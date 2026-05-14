using UnityEngine;
using System.Collections.Generic;

public class ObjetoBucle : MonoBehaviour
{
    [Header("Configuración")]
    // El número que arrastra al cuadrito pequeño
    public int iteraciones = 1;

    [Header("Forma del Bucle (Estilo Tetris)")]
    [Tooltip("La lista de coordenadas que ocupa. (0,0) es la casilla principal donde se suelta.")]
    public List<Vector2Int> celdasQueOcupa = new List<Vector2Int>();

    // Esta función la llamará el cuadro blanco pequeñito cuando le sueltes un número
    public void EstablecerIteraciones(int cantidad)
    {
        iteraciones = cantidad;
        Debug.Log($"Bucle configurado con {iteraciones} iteraciones.");
    }
}