using UnityEngine;
using UnityEngine.EventSystems;

public class NumSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;

            DraggableNum draggableNum = dropped.GetComponent<DraggableNum>();

            if (draggableNum != null)
            {
                draggableNum.parentAfterDrag = transform;

                // le pasa el daño que se le a puesto al hechizo, si no se le a puesto nada pues sera 0
                HechizoATQ hechizo = GetComponentInParent<HechizoATQ>();
                if (hechizo != null)
                {
                    hechizo.EstablecerDano(draggableNum.valor);
                }

                // Es un bucle?
                ObjetoBucle bucle = GetComponentInParent<ObjetoBucle>();
                if (bucle != null)
                {
                    bucle.EstablecerIteraciones(draggableNum.valor);
                }
            }
        }
    }
}