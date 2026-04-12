using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    [Header("Estadísticas")]
    public int vidaMaxima = 100;
    public int vidaActual;

    [Header("Ataque y Escalado")]
    public int danoMinimo = 5;
    public int danoMaximo = 10;
    [HideInInspector] public int intencionActual; // Aquí guarda lo que va a pegar este turno

    [HideInInspector] public elementos elementoIntencion;

    [Header("Estados Alterados")]
    public List<elementos> estadosActuales = new List<elementos>();

    [Header("Interfaz UI")]
    public TMP_Text textoVida;
    public Image barraVida;
    public Image barraVidaAmarilla;

    private void Start()
    {
        vidaActual = vidaMaxima;
        ActualizarUI();
    }

    public void RecibirDano(int cantidad, elementos estadoAtaque)
    {
        vidaActual -= cantidad;
        if (vidaActual < 0) vidaActual = 0;

        ActualizarUI();

        if (cantidad > 0)
        {
            AplicarEstado(estadoAtaque);
        }

        if (vidaActual == 0)
        {
            Debug.Log("¡Enemigo derrotado!");
            // a futuro tengo que poner aqui lo que pasa cuando matas al bicho pelao
        }
    }

    public void AplicarEstado(elementos nuevoEstado)
    {
        // 1. El daño Físico no deja estado alterado
        if (nuevoEstado == elementos.Fisico) return;

        // 2. Comprueba si el enemigo ya tiene este estado
        if (estadosActuales.Contains(nuevoEstado))
        {
            Debug.Log($"El enemigo ya sufre de {nuevoEstado}. No se acumula.");
            return;
        }

        // Si es un estado nuevo y no es físico
        estadosActuales.Add(nuevoEstado);
        Debug.Log($"¡El enemigo sufre el estado: {nuevoEstado}!");
    }

    // Logica de intencion y escalado de dificultad
    public void GenerarIntencion()
    {
        // Elige un número aleatorio entre el mínimo y el máximo (el +1 es porque Random.Range no incluye el último número)
        intencionActual = Random.Range(danoMinimo, danoMaximo + 1);
        // Elige un elemento aleatorio (del 0 al 5, ya que hay 6 elementos en el enum)
        elementoIntencion = (elementos)Random.Range(0, 6);
    }

    public void EscalarDificultad()
    {
        // Aumenta el máximo de daño que puede hacer en el futuro
        danoMaximo += 2;
        Debug.Log($"La dificultad sube. El enemigo ahora puede hacer hasta {danoMaximo} de daño.");
    }

    private void ActualizarUI()
    {
        if (textoVida != null) textoVida.text = $"{vidaActual}/{vidaMaxima}";

        if (barraVida != null)
        {
            barraVida.fillAmount = (float)vidaActual / vidaMaxima;

            if (barraVidaAmarilla != null && barraVidaAmarilla.fillAmount < barraVida.fillAmount)
            {
                barraVidaAmarilla.fillAmount = barraVida.fillAmount;
            }
        }
    }

    public void FijarVida(int cantidad)
    {
        vidaActual = Mathf.Clamp(cantidad, 0, vidaMaxima);
        ActualizarUI();
    }

    public void LimpiarEstados()
    {
        estadosActuales.Clear();
        Debug.Log("Todos los estados del enemigo han sido eliminados.");
    }

    public void EjecutarAnimacionDano()
    {
        StartCoroutine(AnimarBarraAmarilla());
    }

    private IEnumerator AnimarBarraAmarilla()
    {
        if (barraVidaAmarilla == null || barraVida == null) yield break;

        float tiempo = 0f;
        float duracion = 0.5f;
        float fillInicial = barraVidaAmarilla.fillAmount;
        float fillObjetivo = barraVida.fillAmount;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            barraVidaAmarilla.fillAmount = Mathf.Lerp(fillInicial, fillObjetivo, tiempo / duracion);
            yield return null;
        }

        barraVidaAmarilla.fillAmount = fillObjetivo;
    }
}
