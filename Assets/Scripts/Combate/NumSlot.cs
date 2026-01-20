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
            }
        }
    }
}