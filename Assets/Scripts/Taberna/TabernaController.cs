using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TabernaController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Cuando esté el enlace con la base de datos se tendrá que cambiar el 100 por el valor de la base de datos
        PlayerPrefs.SetInt("vidaMax", 100);
    }
}
