using System.Collections.Generic;
using UnityEngine;

public class Bolsa : MonoBehaviour
{
    public List<int> bolsa = new List<int>();
    public List<int> descartes = new List<int>();
    public int Iniciales = 10;

    private void Awake()
    {
        for (int i = 1; i < Iniciales; i++)
        {
            bolsa.Add(i);
        }
    }

    public int sacar()
    {
        if (bolsa.Count == 0)
        {
            bolsa.AddRange(descartes);
            descartes.Clear();
        }

        int valor = Random.Range(0, bolsa.Count);
        int elegido = bolsa[valor];

        descartes.Add(elegido);
        bolsa.RemoveAt(valor);

        return elegido;
    }

    public void añadir(int i)
    {
        bolsa.Add(i);
    }
}