using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image image;
    [HideInInspector] public Transform parentAfterDrag;

    private Atributos atributos;
    private bool puedeArrastrar = true;

    private void Awake()
    {
        image = GetComponent<Image>();
        atributos = GetComponent<Atributos>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (atributos != null && atributos.usosActuales <= 0)
        {
            puedeArrastrar = false; // Bloquea el arrastre

            // muestra el mensaje
            if (AlertaUsos.Instance != null)
            {
                AlertaUsos.Instance.Mostrar();
            }

            return;
        }

        puedeArrastrar = true;
        Debug.Log("Begin drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!puedeArrastrar) return;

        Debug.Log("Dragging");
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!puedeArrastrar) return;

        Debug.Log("End drag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;

        if (parentAfterDrag.GetComponent<InventoryList>() != null)
        {
            if (EjecutorTablero.Instance != null)
            {
                EjecutorTablero.Instance.OrdenarInventarioCompleto();
            }
        }
    }
}