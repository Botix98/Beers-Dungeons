using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    public void comprarObjeto()
    {
        if(int.Parse(Leyendas.text) >= precio)
        {
            //gameObject.SetActive(false);
            InfoObjeto.SetActive(false);
            Leyendas.text = (int.Parse(Leyendas.text) - precio).ToString();
            //Aqui es donde hay que añadir al inventario el objeto y actualizar la base de datos
            Destroy(gameObject);
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
