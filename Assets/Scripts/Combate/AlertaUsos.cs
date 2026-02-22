using UnityEngine;

public class AlertaUsos : MonoBehaviour
{
    // Singleton para que los prefabs puedan encontrar este script
    public static AlertaUsos Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    // Activa la ventana
    public void Mostrar()
    {
        gameObject.SetActive(true);
    }

    // Desactiva la ventana
    public void Ocultar()
    {
        gameObject.SetActive(false);
    }
}