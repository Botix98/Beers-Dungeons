using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CambioObjetos : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefab;
    [SerializeField] private GameObject grid;
    [SerializeField] private TextMeshProUGUI monedas;

    [SerializeField] private int costeCambio = 50;
    [SerializeField] private TextMeshProUGUI costeCambioText;

    [Header("Tablero Info Objeto")]
    [SerializeField] private GameObject infoObjeto;
    [SerializeField] private Image infoSprite;
    [SerializeField] private TextMeshProUGUI infoNombre;
    [SerializeField] private TextMeshProUGUI infoFuncion;
    [SerializeField] private TextMeshProUGUI infoPrecio;

    public void CambiarObjetos()
    {
        if (int.Parse(monedas.text) < costeCambio)
        {
            //Aqui se puede hacer alguna animacion o indicarle al usuario que no tiene dinero
            return;
        }

        monedas.text = (int.Parse(monedas.text) - costeCambio).ToString();
        costeCambio += 50; // Incrementa el coste para la siguiente vez
        costeCambioText.text = costeCambio.ToString() + " Oro";

        for (int i = grid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(grid.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < 4; i++)
        {
            GameObject objeto = Instantiate(itemPrefab[Random.Range(0, 14)], grid.transform);
            objeto.GetComponent<Objeto>().InfoObjeto = infoObjeto;
            objeto.GetComponent<Objeto>().InfoSprite = infoSprite;
            objeto.GetComponent<Objeto>().InfoNombre = infoNombre;
            objeto.GetComponent<Objeto>().InfoFuncion = infoFuncion;
            objeto.GetComponent<Objeto>().InfoPrecio = infoPrecio;
            objeto.GetComponent<Objeto>().Leyendas = monedas;
        }
    }
}
