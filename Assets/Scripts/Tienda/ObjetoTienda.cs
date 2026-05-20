using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjetoTienda : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public TextMeshProUGUI monedas;
    [SerializeField] public GameObject sinMonedas;

    [Header("Tablero Info Objeto")]
    [SerializeField] public GameObject infoObjeto;
    [SerializeField] public Image infoSprite;
    [SerializeField] public TextMeshProUGUI infoNombre;
    [SerializeField] public TextMeshProUGUI infoFuncion;
    [SerializeField] public TextMeshProUGUI infoPrecio;

    [Header("Objeto")]
    [SerializeField] private string nombre;
    [SerializeField] private int precio;
    [SerializeField] private string descripcion;

    public void comprarObjeto()
    {
        if (int.Parse(monedas.text) >= precio)
        {
            gameObject.SetActive(false);
            infoObjeto.SetActive(false);
            Bridge.Instance.inventarioUsos[this.ToString().Split('(')[0]] = Bridge.Instance.inventarioUsos[this.ToString().Split('(')[0]] + 1;
            monedas.text = (int.Parse(monedas.text) - precio).ToString();
            PlayerPrefs.SetInt("monedas", int.Parse(monedas.text) - precio);
            Debug.Log($"Objeto comprado: {nombre}");
        }
        else
        {
            sinMonedas.SetActive(true);
            //Aqui se puede hacer alguna animacion o indicarle al usuario que no tiene dinero
        }
    }

    public void RecuperarVidaActual()
    {
        if (int.Parse(monedas.text) >= precio)
        {
            monedas.text = (int.Parse(monedas.text) - precio).ToString();
            PlayerPrefs.SetInt("monedas", int.Parse(monedas.text) - precio);
            PlayerPrefs.SetInt("vidaActual", PlayerPrefs.GetInt("vidaMax"));

            GameObject.Find("NumeroVida").GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("vidaActual").ToString() + "/" + PlayerPrefs.GetInt("vidaMax").ToString();

            this.gameObject.SetActive(false);

            Debug.Log("Vida recuperada al maximo");
        }
        else
        {
            sinMonedas.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Aqui hay que hacer que se actualice la informacion de los hijos de infoObjeto antes de que se muestre con la info
        //del objeto seleccionado
        infoSprite.sprite = this.gameObject.GetComponent<Image>().sprite;
        infoNombre.text = nombre;
        infoPrecio.text = precio.ToString();
        infoFuncion.text = descripcion;
        infoObjeto.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoObjeto.SetActive(false);
    }
}
