using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Collections.Generic;

public class TabernaController : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text textoLeyendas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //int vidaMax = RunManager.Instance.mejorasJugador[1].desbloqueada ? 100 + 25 * RunManager.Instance.mejorasJugador[1].nivelActual : 100;
        //PlayerPrefs.SetInt("vidaMax", vidaMax);

        textoLeyendas.text = RunManager.Instance.jugador.monedas.ToString();
    }
}
