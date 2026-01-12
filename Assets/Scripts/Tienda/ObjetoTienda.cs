using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjetoTienda : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI monedas;

    [Header("Tablero Info Objeto")]
    [SerializeField] private GameObject infoObjeto;
    [SerializeField] private Image infoSprite;
    [SerializeField] private TextMeshProUGUI infoNombre;
    [SerializeField] private TextMeshProUGUI infoFuncion;
    [SerializeField] private TextMeshProUGUI infoPrecio;

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
            monedas.text = (int.Parse(monedas.text) - precio).ToString();
            //Aqui es donde hay que añadir al inventario el objeto y actualizar la base de datos
        }
        else
        {
            //Aqui se puede hacer alguna animacion o indicarle al usuario que no tiene dinero
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
