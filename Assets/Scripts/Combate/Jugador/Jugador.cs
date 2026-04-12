using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Jugador : MonoBehaviour
{
    [Header("Estadísticas")]
    public int vidaMaxima = 100;
    public int vidaActual;

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

        if (barraVidaAmarilla != null) barraVidaAmarilla.fillAmount = 1f;
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
            Debug.Log("El jugador ha muerto. Fin de la partida.");
            // a futuro tengo que manejar aqui el mensaje de muerte y toda la parafernalia que pasa cuando pierdes por malito
        }
    }

    // Función para añadir el estado a la lista
    public void AplicarEstado(elementos nuevoEstado)
    {
        // 1. El daño Físico es un golpe normal, no deja estado alterado
        if (nuevoEstado == elementos.Fisico) return;

        // 2. Comprueba si el jugador ya tiene este estado para no repetirlo
        if (estadosActuales.Contains(nuevoEstado))
        {
            Debug.Log($"El jugador ya sufre de {nuevoEstado}. No se acumula.");
            return;
        }

        // Si pasa los dos filtros anteriores, se añade a la lista
        estadosActuales.Add(nuevoEstado);
        Debug.Log($"¡El jugador sufre el estado: {nuevoEstado}!");
    }

    public void Curar(int cantidad)
    {
        vidaActual += cantidad;
        if (vidaActual > vidaMaxima) vidaActual = vidaMaxima;
        ActualizarUI();
    }

    // esto para cuando aumentes la vida maxima con cosas "creo que lo usare a futuro pero ya se vera jeje" 
    public void AumentarVidaMaxima(int cantidad)
    {
        vidaMaxima += cantidad;
        vidaActual += cantidad; // esto es para que la vida aumentada se sume a la actual y se le sume al momento
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoVida != null) textoVida.text = $"{vidaActual}/{vidaMaxima}";

        if (barraVida != null)
        {
            float fillObjetivo = (float)vidaActual / vidaMaxima;
            barraVida.fillAmount = fillObjetivo;

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
    public void QuitarEstadoAleatorio()
    {
        if (estadosActuales.Count > 0)
        {
            int rnd = Random.Range(0, estadosActuales.Count);
            Debug.Log($"Llama Purificadora eliminó el estado: {estadosActuales[rnd]}");
            estadosActuales.RemoveAt(rnd);
        }
    }

    public void LimpiarEstados()
    {
        estadosActuales.Clear();
        Debug.Log("Todos los estados del jugador han sido eliminados.");
    }

    public void AplicarEstadoAleatorio()
    {
        // Elige del 1 al 5 (se salta el 0 que es Físico)
        elementos randElem = (elementos)Random.Range(1, 6);
        AplicarEstado(randElem);
    }

    public void EjecutarAnimacionDano()
    {
        StartCoroutine(AnimarBarraAmarilla());
    }

    private IEnumerator AnimarBarraAmarilla()
    {
        if (barraVidaAmarilla == null || barraVida == null) yield break;

        float tiempo = 0f;
        float duracion = 0.5f; // Medio segundo de animación suave
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