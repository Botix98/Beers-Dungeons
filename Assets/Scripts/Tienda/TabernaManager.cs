using UnityEngine;
using TMPro;

public class TabernaManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI numeroVida;
    [SerializeField] private TextMeshProUGUI numeroMonedas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numeroVida.text = PlayerPrefs.GetInt("vidaActual").ToString() + "/" + PlayerPrefs.GetInt("vidaMax").ToString();
        numeroMonedas.text = PlayerPrefs.GetInt("monedas").ToString();
    }
}
