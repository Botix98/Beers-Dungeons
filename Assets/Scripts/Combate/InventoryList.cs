using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryList : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;

        if (dropped != null)
        {
            DraggableItem item = dropped.GetComponent<DraggableItem>();
            if (item != null)
            {
                NumSlot cuadroBlanco = item.GetComponentInChildren<NumSlot>();

                if (cuadroBlanco != null && cuadroBlanco.transform.childCount > 0)
                {
                    Debug.Log("No puedes devolver un hechizo al inventario si tiene un número.");

                    return;
                }

                item.parentAfterDrag = transform;
            }
        }
    }
}