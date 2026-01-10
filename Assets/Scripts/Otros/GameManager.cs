using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject canvasJuego;
    [SerializeField] private GameObject canvasAjustes;

    public void Cargar(string nombreEscena)
    {
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
