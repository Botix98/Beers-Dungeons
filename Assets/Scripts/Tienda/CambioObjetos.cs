using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CambioObjetos : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefab;
    [SerializeField] private GameObject grid;
    [SerializeField] private TextMeshProUGUI monedas;

    [Header("Tablero Info Objeto")]
    [SerializeField] private GameObject infoObjeto;
    [SerializeField] private Image infoSprite;
    [SerializeField] private TextMeshProUGUI infoNombre;
    [SerializeField] private TextMeshProUGUI infoFuncion;
    [SerializeField] private TextMeshProUGUI infoPrecio;

    public void CambiarObjetos()
    {
        //FALTA HACER QUE COMPRUEBE SI TIENES DINERO SUFICIENTE PARA HACER EL CAMBIO Y SI NO LO TIENES QUE NO TE DEJE HACERLO
        for (int i = grid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(grid.transform.GetChild(i).gameObject);
        }

        Debug.Log("Ha salido supongo ns");
        for (int i = 0; i < 4; i++)
        {
            Debug.Log("Y aqui entra?");
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
