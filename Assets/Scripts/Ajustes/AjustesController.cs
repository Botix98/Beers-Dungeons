using System;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AjustesController : MonoBehaviour
{
    [SerializeField] private Slider sliderVolumen;

    private void OnEnable()
    {
        
    }

    public void EliminarCuenta()
    {
        RunManager.Instance.EliminarCuenta();
        Destroy(RunManager.Instance.gameObject);
    }

    public void CerrarSesion()
    {
        RunManager.Instance.CerrarSesion();
        Destroy(RunManager.Instance.gameObject);
    }

    public void CambiarVolumen(float volumen)
    {
        AudioListener.volume = volumen;
        PlayerPrefs.SetFloat("volumen", volumen);
    }

    public void Tutorial()
    {
        
    }

    public void Creditos()
    {
        
    }
}