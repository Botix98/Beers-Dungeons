using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableBucle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentOriginal;
    private ObjetoBucle objetoBucle;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        objetoBucle = GetComponent<ObjetoBucle>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Guardamos de dónde venimos (Inventario) por si lo soltamos en un sitio no válido
        if (transform.parent.name != "ImgCuadri") // Si no estábamos ya en el tablero
        {
            parentOriginal = transform.parent;
        }

        // Lo ponemos en el canvas principal para que se vea por encima de todo al arrastrar
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Desactivamos los raycasts para que el ratón pueda "atravesar" la pieza y detectar las casillas del tablero
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Lanzamos un "rayo" desde el ratón para ver qué hay debajo
        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        SpellSlot casillaBase = null;
        foreach (RaycastResult res in resultados)
        {
            casillaBase = res.gameObject.GetComponent<SpellSlot>();
            if (casillaBase != null) break; // Encontramos una casilla
        }

        // Si soltamos la esquina superior izquierda del ratón encima de una casilla...
        if (casillaBase != null)
        {
            if (ComprobarLimitesTablero(casillaBase))
            {
                // ¡Encaja! Lo hacemos hijo del fondo del tablero (ImgCuadri)
                Transform contenedorTablero = casillaBase.transform.parent;
                transform.SetParent(contenedorTablero);
                transform.SetAsLastSibling(); // Para que se dibuje por encima de los hechizos

                AlinearConCasilla(casillaBase);
                return; // Terminamos con éxito
            }
            else
            {
                Debug.Log("Tetris: El bucle choca con la pared o se sale del tablero.");
            }
        }

        // Si no soltamos en el tablero o se sale de los límites, vuelve a su sitio
        if (parentOriginal != null)
        {
            transform.SetParent(parentOriginal);
        }
    }

    private bool ComprobarLimitesTablero(SpellSlot casillaBase)
    {
        // Tu casilla se llama "SlotG_A1". Sacamos la "A" y el "1".
        string nombreCasilla = casillaBase.gameObject.name;
        string coordenada = nombreCasilla.Substring(nombreCasilla.LastIndexOf('_') + 1);

        char filaBase = coordenada[0]; // 'A'
        int colBase = int.Parse(coordenada.Substring(1)); // 1

        Transform contenedorTablero = casillaBase.transform.parent;

        // Comprobamos cada trozo de la forma del Tetris leyendo la lista de ObjetoBucle
        foreach (Vector2Int offset in objetoBucle.celdasQueOcupa)
        {
            char targetFila = (char)(filaBase + offset.y); // Si offset Y es 1, 'A' pasa a ser 'B'
            int targetCol = colBase + offset.x; // Si offset X es 1, 1 pasa a ser 2

            string sufijoBuscado = "_" + targetFila + targetCol; // Se convierte en "_A2"

            // Buscamos si existe una casilla que termine con ese nombre en tu ImgCuadri
            bool existe = false;
            foreach (Transform hijo in contenedorTablero)
            {
                if (hijo.name.EndsWith(sufijoBuscado))
                {
                    existe = true;
                    break;
                }
            }

            // Si el código no encuentra la casilla "_A7", significa que te sales del tablero
            if (!existe) return false;
        }

        return true;
    }

    private void AlinearConCasilla(SpellSlot casillaBase)
    {
        // Truco matemático para alinear perfectamente la esquina Arriba-Izquierda 
        // del marco negro con la esquina Arriba-Izquierda de la casilla, sin importar tus configuraciones de Pivot.
        RectTransform slotRect = casillaBase.GetComponent<RectTransform>();

        Vector3[] slotEsquinas = new Vector3[4];
        slotRect.GetWorldCorners(slotEsquinas);
        Vector3 esquinaArribaIzquierdaCasilla = slotEsquinas[1];

        Vector3[] marcoEsquinas = new Vector3[4];
        rectTransform.GetWorldCorners(marcoEsquinas);
        Vector3 esquinaArribaIzquierdaMarco = marcoEsquinas[1];

        Vector3 diferencia = esquinaArribaIzquierdaCasilla - esquinaArribaIzquierdaMarco;
        transform.position += diferencia;
    }
}