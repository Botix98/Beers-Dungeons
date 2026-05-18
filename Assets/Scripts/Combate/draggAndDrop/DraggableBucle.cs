using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // <--- AÑADIDO: Necesario para perforar la UI

[RequireComponent(typeof(CanvasGroup))]
// <--- AÑADIDO: ICanvasRaycastFilter al final de esta línea
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

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
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

        if (casillaBase != null && ComprobarLimitesTablero(casillaBase))
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
        DraggableNum draggableNum = dropped.GetComponent<DraggableNum>(); // Ahora también detecta números

        if (draggableItem != null || draggableNum != null)
        {
            // Apagamos la colisión temporalmente para ver qué hay detrás
            canvasGroup.blocksRaycasts = false;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> resultados = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, resultados);

            foreach (RaycastResult res in resultados)
            {
                // Si lo que soltaron fue un HECHIZO
                if (draggableItem != null)
                {
                    SpellSlot slot = res.gameObject.GetComponent<SpellSlot>();
                    if (slot != null && slot.transform.childCount == 0 && slot.transform.parent.name == "ImgCuadri")
                    {
                        draggableItem.parentAfterDrag = slot.transform;
                        break;
                    }
                }
                // Si lo que soltaron fue un NÚMERO
                else if (draggableNum != null)
                {
                    NumSlot numSlotDestino = res.gameObject.GetComponent<NumSlot>();
                    if (numSlotDestino != null)
                    {
                        // Le pasamos el número al hechizo que hay debajo
                        numSlotDestino.OnDrop(eventData);
                        break;
                    }
                }
            }

            // Volvemos a encender la colisión del marco
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

    private void AlinearConCasilla(SpellSlot casillaBase)
    {
        RectTransform slotRect = casillaBase.GetComponent<RectTransform>();

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

        Vector3[] slotEsquinas = new Vector3[4];
        slotRect.GetWorldCorners(slotEsquinas);
        Vector3 esquinaArribaIzquierdaCasilla = slotEsquinas[1];

        Vector3[] marcoEsquinas = new Vector3[4];
        rectTransform.GetWorldCorners(marcoEsquinas);
        Vector3 esquinaArribaIzquierdaMarco = marcoEsquinas[1];

        Vector3 diferencia = esquinaArribaIzquierdaCasilla - esquinaArribaIzquierdaMarco;
        transform.position += diferencia;
    }

    // =================================================================================
    // EL "TALADRO" INTELIGENTE: ADAPTABLE A CUALQUIER RESOLUCIÓN
    // =================================================================================
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (rectTransform == null) return false;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out localPoint);
        Rect rect = rectTransform.rect;

        // Calculamos un grosor muy fino basado en el tamaño real de tu pieza (aprox 10% del tamaño)
        float grosorX = rect.width * 0.08f;
        float grosorY = rect.height * 0.12f;

        // 1. LAS LÍNEAS NEGRAS: Solo los bordes finitos de los extremos
        bool tocandoLineaIzquierda = localPoint.x < rect.xMin + grosorX;
        bool tocandoLineaDerecha = localPoint.x > rect.xMax - grosorX;
        bool tocandoLineaAbajo = localPoint.y < rect.yMin + grosorY;
        bool tocandoLineaArriba = localPoint.y > rect.yMax - grosorY;

        if (tocandoLineaIzquierda || tocandoLineaDerecha || tocandoLineaAbajo || tocandoLineaArriba)
        {
            return true;
        }

        // 2. EL CUADRITO BLANCO: Restringimos su área estrictamente a su esquinita
        float tamanoCuadro = rect.height * 0.30f;
        if (localPoint.x < rect.xMin + tamanoCuadro && localPoint.y > rect.yMax - tamanoCuadro)
        {
            return true;
        }

        // 3. TODO LO DEMÁS (el 80% del centro): Es completamente invisible al ratón
        return false;
    }
}