using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableBucle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, ICanvasRaycastFilter
{
    [HideInInspector] public Transform parentOriginal;
    [HideInInspector] public Vector3 posicionOriginal;

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
        parentOriginal = transform.parent;
        posicionOriginal = transform.position;

        Canvas canvasPrincipal = GetComponentInParent<Canvas>();
        if (canvasPrincipal == null) canvasPrincipal = FindObjectOfType<Canvas>();

        transform.SetParent(canvasPrincipal.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;

        AjustarTamanoReal();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            GetComponentInParent<Canvas>().transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint
        );

        worldPoint.z = 0;
        transform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        SpellSlot casillaBase = null;
        foreach (RaycastResult res in resultados)
        {
            SpellSlot slotEncontrado = res.gameObject.GetComponent<SpellSlot>();
            if (slotEncontrado != null && slotEncontrado.transform.parent.name == "ImgCuadri")
            {
                casillaBase = slotEncontrado;
                break;
            }
        }

        if (casillaBase != null && ComprobarLimitesTablero(casillaBase) && !HayOtroBucleEnCamino(casillaBase))
        {
            Transform contenedorTablero = casillaBase.transform.parent;
            Transform padreSinGrid = contenedorTablero.parent;

            transform.SetParent(padreSinGrid);
            transform.SetAsLastSibling();

            AlinearConCasilla(casillaBase);
        }
        else
        {
            transform.SetParent(parentOriginal);
            transform.position = posicionOriginal;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
        DraggableNum draggableNum = dropped.GetComponent<DraggableNum>();

        if (draggableItem != null || draggableNum != null)
        {
            canvasGroup.blocksRaycasts = false;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> resultados = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, resultados);

            foreach (RaycastResult res in resultados)
            {
                // Si lo que soltaron fue un hechizo
                if (draggableItem != null)
                {
                    SpellSlot slot = res.gameObject.GetComponent<SpellSlot>();
                    if (slot != null && slot.transform.childCount == 0 && slot.transform.parent.name == "ImgCuadri")
                    {
                        draggableItem.parentAfterDrag = slot.transform;
                        break;
                    }
                }
                // Si lo que soltaron fue un numero
                else if (draggableNum != null)
                {
                    NumSlot numSlotDestino = res.gameObject.GetComponent<NumSlot>();
                    if (numSlotDestino != null)
                    {
                        // Le pasa el número al hechizo que hay debajo
                        numSlotDestino.OnDrop(eventData);
                        break;
                    }
                }
            }

            canvasGroup.blocksRaycasts = true;
        }
    }

    private bool ComprobarLimitesTablero(SpellSlot casillaBase)
    {
        string nombreCasilla = casillaBase.gameObject.name;
        string coordenada = nombreCasilla.Substring(nombreCasilla.LastIndexOf('_') + 1);

        char filaBase = coordenada[0];
        int colBase = int.Parse(coordenada.Substring(1));

        Transform contenedorTablero = casillaBase.transform.parent;

        foreach (Vector2Int offset in objetoBucle.celdasQueOcupa)
        {
            char targetFila = (char)(filaBase + offset.y);
            int targetCol = colBase + offset.x;

            string sufijoBuscado = "_" + targetFila + targetCol;

            bool existe = false;
            foreach (Transform hijo in contenedorTablero)
            {
                if (hijo.name.EndsWith(sufijoBuscado))
                {
                    existe = true;
                    break;
                }
            }

            if (!existe) return false;
        }

        return true;
    }

    private void AjustarTamanoReal()
    {
        SpellSlot slotMuestra = FindObjectOfType<SpellSlot>();
        if (slotMuestra == null) return;

        RectTransform slotRect = slotMuestra.GetComponent<RectTransform>();

        int columnas = 0;
        int filas = 0;
        foreach (Vector2Int offset in objetoBucle.celdasQueOcupa)
        {
            if (offset.x > columnas) columnas = offset.x;
            if (offset.y > filas) filas = offset.y;
        }

        float anchoTotal = slotRect.rect.width * (columnas + 1);
        float altoTotal = slotRect.rect.height * (filas + 1);
        rectTransform.sizeDelta = new Vector2(anchoTotal, altoTotal);
    }

    private void AlinearConCasilla(SpellSlot casillaBase)
    {
        AjustarTamanoReal();

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

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (rectTransform == null) return false;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out localPoint);
        Rect rect = rectTransform.rect;

        float grosorX = rect.width * 0.08f;
        float grosorY = rect.height * 0.12f;

        bool tocandoLineaIzquierda = localPoint.x < rect.xMin + grosorX;
        bool tocandoLineaDerecha = localPoint.x > rect.xMax - grosorX;
        bool tocandoLineaAbajo = localPoint.y < rect.yMin + grosorY;
        bool tocandoLineaArriba = localPoint.y > rect.yMax - grosorY;

        if (tocandoLineaIzquierda || tocandoLineaDerecha || tocandoLineaAbajo || tocandoLineaArriba)
        {
            return true;
        }

        float tamanoCuadro = rect.height * 0.30f;
        if (localPoint.x < rect.xMin + tamanoCuadro && localPoint.y > rect.yMax - tamanoCuadro)
        {
            return true;
        }

        return false;
    }

    private bool HayOtroBucleEnCamino(SpellSlot casillaBase)
    {
        // Obtener todos los bucles que ya están puestos en el tablero
        Transform contenedorTablero = casillaBase.transform.parent;
        Transform padreSinGrid = contenedorTablero.parent;
        ObjetoBucle[] todosLosBucles = padreSinGrid.GetComponentsInChildren<ObjetoBucle>();

        // Encontrar los slots exactos que este nuevo bucle va a ocupar
        string nombreCasilla = casillaBase.gameObject.name;
        string coordenada = nombreCasilla.Substring(nombreCasilla.LastIndexOf('_') + 1);
        char filaBase = coordenada[0];
        int colBase = int.Parse(coordenada.Substring(1));

        List<Transform> slotsDestino = new List<Transform>();

        foreach (Vector2Int offset in objetoBucle.celdasQueOcupa)
        {
            char targetFila = (char)(filaBase + offset.y);
            int targetCol = colBase + offset.x;
            string sufijoBuscado = "_" + targetFila + targetCol;

            foreach (Transform hijo in contenedorTablero)
            {
                if (hijo.name.EndsWith(sufijoBuscado))
                {
                    slotsDestino.Add(hijo);
                    break;
                }
            }
        }

        // Comprueba si el centro de alguno de estos slots cae dentro de OTRA caja de bucle
        foreach (Transform slot in slotsDestino)
        {
            Vector3[] slotCorners = new Vector3[4];
            slot.GetComponent<RectTransform>().GetWorldCorners(slotCorners);
            Vector3 slotCenter = (slotCorners[0] + slotCorners[2]) / 2f;

            foreach (ObjetoBucle otroBucle in todosLosBucles)
            {
                if (otroBucle == this.objetoBucle) continue;

                Vector3[] bCorners = new Vector3[4];
                otroBucle.GetComponent<RectTransform>().GetWorldCorners(bCorners);

                // Si el centro de la casilla choca con el marco de otro bucle
                if (slotCenter.x >= bCorners[0].x && slotCenter.x <= bCorners[2].x &&
                    slotCenter.y >= bCorners[0].y && slotCenter.y <= bCorners[1].y)
                {
                    return true;
                }
            }
        }

        return false;
    }
}