using UnityEngine;
using UnityEngine.EventSystems;

public class BucleNumSlotVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 escalaOriginal;

    [Header("Configuración de Efectos")]
    [Tooltip("Cuánto se agranda el cuadrito blanco al pasar el ratón por encima (1.5 = 150%)")]
    public float factorHover = 1.5f;

    [Tooltip("El tamaño que se le forzará al número cuando esté dentro del cuadrito (0.5 = la mitad de su tamaño original)")]
    public float escalaNumeroInterno = 0.5f;

    private void Start()
    {
        // Guardamos el tamaño pequeñito original que le hayas puesto en el Inspector
        escalaOriginal = transform.localScale;
    }

    // EFECTO HOVER: Cuando el puntero del ratón entra en el cuadrito blanco
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Se hace más grande para que sea súper fácil apuntar y soltar el número
        transform.localScale = escalaOriginal * factorHover;

        // Lo ponemos al frente de todo en la UI para asegurarnos de que nada lo tape mientras interactuamos
        transform.SetAsLastSibling();
    }

    // EFECTO HOVER: Cuando el puntero del ratón sale del cuadrito blanco
    public void OnPointerExit(PointerEventData eventData)
    {
        // Vuelve a su tamaño pequeño original para no estorbar
        transform.localScale = escalaOriginal;
    }

    private void Update()
    {
        // Si el cuadrito blanco tiene un hijo (es decir, ya le hemos soltado un número dentro)
        if (transform.childCount > 0)
        {
            Transform numeroHijo = transform.GetChild(0);

            // Si el número sigue teniendo su tamaño grande original, lo encogemos
            if (numeroHijo.localScale.x > escalaNumeroInterno)
            {
                numeroHijo.localScale = new Vector3(escalaNumeroInterno, escalaNumeroInterno, 1f);
            }
        }
    }
}