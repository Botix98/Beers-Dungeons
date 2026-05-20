using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CambioObjetos : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefab;
    [SerializeField] private GameObject grid;
    [SerializeField] private TextMeshProUGUI monedas;
    [SerializeField] private GameObject sinMonedas;

    [SerializeField] private int costeCambio = 50;
    [SerializeField] private TextMeshProUGUI costeCambioText;

    [Header("Tablero Info Objeto")]
    [SerializeField] private GameObject infoObjeto;
    [SerializeField] private Image infoSprite;
    [SerializeField] private TextMeshProUGUI infoNombre;
    [SerializeField] private TextMeshProUGUI infoFuncion;
    [SerializeField] private TextMeshProUGUI infoPrecio;

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject objeto = Instantiate(itemPrefab[Random.Range(0, 14)], grid.transform);
            objeto.GetComponent<ObjetoTienda>().infoObjeto = infoObjeto;
            objeto.GetComponent<ObjetoTienda>().infoSprite = infoSprite;
            objeto.GetComponent<ObjetoTienda>().infoNombre = infoNombre;
            objeto.GetComponent<ObjetoTienda>().infoFuncion = infoFuncion;
            objeto.GetComponent<ObjetoTienda>().infoPrecio = infoPrecio;
            objeto.GetComponent<ObjetoTienda>().monedas = monedas;
            objeto.GetComponent<ObjetoTienda>().sinMonedas = sinMonedas;
            objeto.GetComponent<Button>().onClick.AddListener(objeto.GetComponent<ObjetoTienda>().comprarObjeto);
        }
    }

    public void CambiarObjetos()
    {
        if (int.Parse(monedas.text) < costeCambio)
        {
            //Aqui se puede hacer alguna animacion o indicarle al usuario que no tiene dinero
            return;
        }

        monedas.text = (int.Parse(monedas.text) - costeCambio).ToString();
        PlayerPrefs.SetInt("monedas", int.Parse(monedas.text) - costeCambio);
        costeCambio += 50; // Incrementa el coste para la siguiente vez
        costeCambioText.text = costeCambio.ToString() + " Oro";

        for (int i = grid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(grid.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < 4; i++)
        {
            GameObject objeto = Instantiate(itemPrefab[Random.Range(0, 14)], grid.transform);
            objeto.GetComponent<ObjetoTienda>().infoObjeto = infoObjeto;
            objeto.GetComponent<ObjetoTienda>().infoSprite = infoSprite;
            objeto.GetComponent<ObjetoTienda>().infoNombre = infoNombre;
            objeto.GetComponent<ObjetoTienda>().infoFuncion = infoFuncion;
            objeto.GetComponent<ObjetoTienda>().infoPrecio = infoPrecio;
            objeto.GetComponent<ObjetoTienda>().monedas = monedas;
            objeto.GetComponent<ObjetoTienda>().sinMonedas = sinMonedas;
            objeto.GetComponent<Button>().onClick.AddListener(objeto.GetComponent<ObjetoTienda>().comprarObjeto);
        }
    }
}
