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
                item.parentAfterDrag = transform;
            }
        }
    }
}