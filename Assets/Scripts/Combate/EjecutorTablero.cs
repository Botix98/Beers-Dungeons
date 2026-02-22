using System.Collections.Generic;
using UnityEngine;

public class EjecutorTablero : MonoBehaviour
{
    public static EjecutorTablero Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [Header("Referencias de la UI")]
    public Transform contenedorTablero;
    public Transform contenedorInventario;

    public void EjecutarHechizos()
    {
        // 1. Obtener todos los slots
        List<Transform> listaSlots = new List<Transform>();
        foreach (Transform hijo in contenedorTablero)
        {
            listaSlots.Add(hijo);
        }

        // 2. Ordenar por coordenada (A1, A2, B1...)
        listaSlots.Sort((a, b) =>
        {
            string coordenadaA = a.name.Substring(a.name.IndexOf('_') + 1);
            string coordenadaB = b.name.Substring(b.name.IndexOf('_') + 1);
            return coordenadaA.CompareTo(coordenadaB);
        });

        // 3. Procesar en orden
        foreach (Transform slot in listaSlots)
        {
            if (slot.childCount > 0)
            {
                Transform hechizo = slot.GetChild(0);
                Atributos atributosHechizo = hechizo.GetComponent<Atributos>();

                if (atributosHechizo != null)
                {
                    atributosHechizo.GastarUso();
                }

                // Limpiar la ficha de número (para los que son de atq que tiene números)
                NumSlot cuadroBlanco = hechizo.GetComponentInChildren<NumSlot>();
                if (cuadroBlanco != null)
                {
                    foreach (Transform numero in cuadroBlanco.transform)
                    {
                        Destroy(numero.gameObject);
                    }
                }

                // Devolver el hechizo al inventario
                hechizo.SetParent(contenedorInventario);
            }
        }

        OrdenarInventarioCompleto();

        Debug.Log("Tablero ejecutado. Hechizos devueltos al inventario.");
    }

    public void OrdenarInventarioCompleto()
    {
        // guarda todos los objetos del content en una lista
        List<Transform> todosLosHechizos = new List<Transform>();
        foreach (Transform t in contenedorInventario)
        {
            todosLosHechizos.Add(t);
        }

        // ordena la lista poniendo los que tienen 0 usos al final
        todosLosHechizos.Sort((a, b) =>
        {
            Atributos attrA = a.GetComponent<Atributos>();
            Atributos attrB = b.GetComponent<Atributos>();

            if (attrA == null || attrB == null) return 0;

            int pesoA = (attrA.usosActuales <= 0) ? 1 : 0;
            int pesoB = (attrB.usosActuales <= 0) ? 1 : 0;

            return pesoA.CompareTo(pesoB);
        });

        // aplica el nuevo orden
        for (int i = 0; i < todosLosHechizos.Count; i++)
        {
            todosLosHechizos[i].SetSiblingIndex(i);
        }
    }
}