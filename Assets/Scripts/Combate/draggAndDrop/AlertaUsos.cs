using UnityEngine;

public class AlertaUsos : MonoBehaviour
{
    public static AlertaUsos Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        gameObject.SetActive(false);
    }

    public void Mostrar()
    {
        gameObject.SetActive(true);
    }

    public void Ocultar()
    {
        gameObject.SetActive(false);
    }
}