using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum elementos
{
    Fisico,
    Cortante,
    Calor,
    Frio,
    Toxina,
    Electrico
}

public class Atributos : MonoBehaviour
{
    [Header("Comunes")]
    //public string nombre;
    public int id;
    public int espacios;

    [Header("Usos del Hechizo")]
    public int usosIniciales = 3;
    [HideInInspector]
    public int usosActuales;
    public TMP_Text textoUsos;

    //protected virtual void Start()
    //{
    //    ConfigurarUsos();
    //}

    public void ConfigurarUsos(int usosGuardados = -1)
    {
        if (usosGuardados < 0)
        {
            // Es un hechizo nuevo
            usosActuales = usosIniciales;
        }
        else
        {
            // Es un hechizo que viene del inventario/tienda
            usosActuales = usosGuardados;
        }

        ActualizarUI();
    }
    public void ActualizarUI()
    {
        if (textoUsos != null)
        {
            textoUsos.text = usosActuales.ToString();
        }
        else
        {
            Debug.LogWarning("Falta asignar el texto de usos en el prefab " + gameObject.name);
        }
    }
    public void GastarUso()
    {
        if (usosActuales > 0)
        {
            usosActuales--;
            ActualizarUI();

            if (usosActuales <= 0)
            {
                Debug.Log("Hechizo agotado: " + gameObject.name + "!");
                // Aqu� avisaremos al inventario de que este hechizo debe destruirse
            }
        }
    }

    public void AnadirUsos(int cantidad)
    {
        usosActuales += cantidad;
        ActualizarUI();
    }
}
[System.Serializable]
public class EstadoAlterado
{
    public elementos tipo;
    public int turnosRestantes; // Si es -1, dura infinito
    public int turnosActivo; // Para llevar la cuenta de la Toxina

    public EstadoAlterado(elementos tipo, int turnosRestantes)
    {
        this.tipo = tipo;
        this.turnosRestantes = turnosRestantes;
        this.turnosActivo = 0; // Empieza en 0
    }
}
