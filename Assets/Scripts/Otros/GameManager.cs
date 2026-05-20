using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject canvasJuego;
    [SerializeField] private GameObject canvasAjustes;

    public void Cargar(string nombreEscena)
    {

        // Solo guarda los usos si estas saliendo de la escena de Juego/Combate
        if (SceneManager.GetActiveScene().name == "Juego")
        {
            if (Bridge.Instance != null)
            {
                Bridge.Instance.GuardarUsos();
            }
        }

        // Esto habra que quitarlo
        PlayerPrefs.SetInt("monedas", 1500);

        if (nombreEscena == "Juego")
        {
            int vidaMaxima = RunManager.Instance.mejorasJugador[1].desbloqueada ? (100 + (25 * RunManager.Instance.mejorasJugador[1].nivelActual)) : 100;
            PlayerPrefs.SetInt("vidaMax", vidaMaxima);
            PlayerPrefs.SetInt("vidaActual", vidaMaxima);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }
    public void Salir()
    {
        Application.Quit();
    }

    public void IrAjustes()
    {
        canvasAjustes.SetActive(true);
        canvasJuego.SetActive(false);
    }

    public void SalirAjustes()
    {
        canvasJuego.SetActive(true);
        canvasAjustes.SetActive(false);
    }
}
