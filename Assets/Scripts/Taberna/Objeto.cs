using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class Objeto : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI leyendas;

    [Header("Tablero Info Objeto")]
    [SerializeField] private GameObject infoObjeto;
    [SerializeField] private Image infoSprite;
    [SerializeField] private TextMeshProUGUI infoNombre;
    [SerializeField] private TextMeshProUGUI infoFuncion;
    [SerializeField] private TextMeshProUGUI infoPrecio;

    public GameObject InfoObjeto { get => infoObjeto; set => infoObjeto = value; }
    public Image InfoSprite { get => infoSprite; set => infoSprite = value; }
    public TextMeshProUGUI InfoNombre { get => infoNombre; set => infoNombre = value; }
    public TextMeshProUGUI InfoFuncion { get => infoFuncion; set => infoFuncion = value; }
    public TextMeshProUGUI InfoPrecio { get => infoPrecio; set => infoPrecio = value; }
    public TextMeshProUGUI Leyendas { get => leyendas; set => leyendas = value; }

    [Header("Objeto")]
    [SerializeField] private string nombre;
    [SerializeField] private int precio;
    [SerializeField] private string descripcion;
    [SerializeField] private int idMejora;

    public void comprarObjeto()
    {
        int leyendasActuales = int.Parse(Leyendas.text);

        if (leyendasActuales >= precio)
        {
            try 
            {
                Leyendas.text = (leyendasActuales - precio).ToString();

                RunManager.Instance.jugador.monedas -= precio;
                if (RunManager.Instance.mejorasJugador[idMejora - 1].desbloqueada)
                    RunManager.Instance.mejorasJugador[idMejora - 1].nivelActual += 1;
                else
                    RunManager.Instance.mejorasJugador[idMejora - 1].desbloqueada = true;

                RunManager.Instance.ActualizarMonedasJugador();
                RunManager.Instance.ActualizarMejorasJugador();

                Debug.Log($"Mejora comprada");

                InfoObjeto.SetActive(false);
                Destroy(gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al comprar objeto: " + e.Message);
                // Revertimos el cambio en la UI para que el jugador no pierda sus monedas
                RunManager.Instance.jugador.monedas = leyendasActuales;
                Leyendas.text = leyendasActuales.ToString();
                RunManager.Instance.ActualizarMonedasJugador();
            }
        }
        else
        {
            Debug.Log("No tienes suficiente dinero");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Aqui hay que hacer que se actualice la informacion de los hijos de infoObjeto antes de que se muestre con la info
        //del objeto seleccionado
        InfoSprite.sprite = this.gameObject.GetComponent<Image>().sprite;
        InfoNombre.text = nombre;
        InfoPrecio.text = precio.ToString();
        InfoFuncion.text = descripcion;
        InfoObjeto.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InfoObjeto.SetActive(false);
    }
}
