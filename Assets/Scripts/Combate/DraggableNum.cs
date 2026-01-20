using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableNum : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Empezando a arrastrar número");

        parentAfterDrag = transform.parent;
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
        Debug.Log("Soltando número");

        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;

        if (parentAfterDrag.GetComponent<NumSlot>() != null)
        {
            canvasGroup.blocksRaycasts = false;

            NumController.Instance.ReiniciarMano();
        }
        else
        {
            canvasGroup.blocksRaycasts = true;
        }
    }
}