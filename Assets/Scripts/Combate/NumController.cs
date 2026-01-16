using System.Collections;
using TMPro;
using UnityEngine;

public class NumController : MonoBehaviour
{
    public Transform padre;
    public GameObject numPrefab;
    public Bolsa bolsa;
    public float tiempoEspera = 0.2f;

    [Tooltip("El número de copias, este incluido")]
    public int copias = 5;

    private void Start()
    {
        StartCoroutine(crearConEspera());
    }

    IEnumerator crearConEspera()
    {
        for (int i = 1; i <= copias; i++)
        {
            crear();
            yield return new WaitForSeconds(tiempoEspera);
        }
    }

    public void crear()
    {
        GameObject numero = Instantiate(numPrefab, padre);

        int valor = bolsa.sacar();

        numero.GetComponentInChildren<TMP_Text>(true).text = valor.ToString();
    }
}
