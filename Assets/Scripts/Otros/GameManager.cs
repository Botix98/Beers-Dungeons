using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject canvasJuego;
    [SerializeField] private GameObject canvasAjustes;


    public void IniciarNuevaPartida()
    {
        PlayerPrefs.SetInt("PisoActual", 0); // Reiniciamos el piso
        PlayerPrefs.SetInt("monedas", 0); // Dinero inicial para arrancar
        Cargar("Juego");
    }
    public void Cargar(string nombreEscena)
    {
        if (SceneManager.GetActiveScene().name == "Juego")
        {
            if (Bridge.Instance != null) Bridge.Instance.GuardarUsos();
        }

        // Lógica al entrar al combate
        if (nombreEscena == "Juego")
        {
            // Sumamos 1 al piso actual
            int pisoActual = PlayerPrefs.GetInt("PisoActual", 0);
            pisoActual++;
            PlayerPrefs.SetInt("PisoActual", pisoActual);

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
