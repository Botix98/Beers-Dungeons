using UnityEngine;
using UnityEngine.UI;

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

}
