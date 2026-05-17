using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    [Header("Estad�sticas")]
    public int vidaMaxima = 100;
    public int vidaActual;

    [Header("Ataque y Escalado")]
    public int danoMinimo = 5;
    public int danoMaximo = 10;


    [HideInInspector] public int intencionBase; // El da�o puro antes de estados
    [HideInInspector] public int intencionActual; // El da�o que se va a hacer
    [HideInInspector] public elementos elementoIntencion;

    [Header("Estados Alterados")]
    public List<EstadoAlterado> estadosActuales = new List<EstadoAlterado>();

    [Header("Interfaz UI")]
    public TMP_Text textoVida;
    public Image barraVida;
    public Image barraVidaAmarilla;

    private void Start()
    {
        vidaActual = vidaMaxima;
        ActualizarUI();
    }

    public bool TieneEstado(elementos tipo)
    {
        foreach (var e in estadosActuales) if (e.tipo == tipo) return true;
        return false;
    }

    public void RecibirDano(int cantidad, elementos estadoAtaque)
    {
        float multiplicador = 1f;
        if (TieneEstado(elementos.Electrico)) multiplicador += 1f; // x2 da�o recibido

        int danoFinal = Mathf.RoundToInt(cantidad * multiplicador);

        vidaActual -= danoFinal;
        if (vidaActual < 0) vidaActual = 0;

        ActualizarUI();

        if (cantidad > 0) AplicarEstado(estadoAtaque);
        if (vidaActual == 0) Debug.Log("�Enemigo derrotado!");
    }

    public void AplicarEstado(elementos nuevoEstado)
    {
        if (nuevoEstado == elementos.Fisico) return;
        if (TieneEstado(nuevoEstado)) return;

        int duracion = -1; // Infinito
        if (nuevoEstado == elementos.Cortante) duracion = 3;

        estadosActuales.Add(new EstadoAlterado(nuevoEstado, duracion));
        Debug.Log($"�El enemigo sufre el estado: {nuevoEstado}!");

        // Recalcular el ataque en tiempo real por si le has tirado Calor o Frio
        RecalcularIntencion();
    }

    public void ProcesarEstadosAlFinalDelTurno()
    {
        for (int i = estadosActuales.Count - 1; i >= 0; i--)
        {
            EstadoAlterado estado = estadosActuales[i];

            if (estado.tipo == elementos.Calor)
            {
                int danoCalor = Mathf.Max(1, Mathf.RoundToInt(vidaMaxima * 0.05f));
                RecibirDano(danoCalor, elementos.Fisico);
            }
            else if (estado.tipo == elementos.Toxina)
            {
                float multToxina = Mathf.Pow(2, estado.turnosActivo);
                int danoToxina = Mathf.Max(1, Mathf.RoundToInt(vidaMaxima * 0.02f * multToxina));
                RecibirDano(danoToxina, elementos.Fisico);
            }
            else if (estado.tipo == elementos.Cortante)
            {
                int danoCortante = Mathf.Max(1, Mathf.RoundToInt(vidaActual * 0.15f));
                RecibirDano(danoCortante, elementos.Fisico);
            }

            estado.turnosActivo++;

            if (estado.turnosRestantes > 0)
            {
                estado.turnosRestantes--;
                if (estado.turnosRestantes == 0) estadosActuales.RemoveAt(i);
            }
        }
    }

    // Logica de intencion y escalado de dificultad
    public void GenerarIntencion()
    {
        // Elige un n�mero aleatorio entre el m�nimo y el m�ximo (el +1 es porque Random.Range no incluye el �ltimo n�mero)
        intencionBase = Random.Range(danoMinimo, danoMaximo + 1);
        // Elige un elemento aleatorio (del 0 al 5, ya que hay 6 elementos en el enum)
        elementoIntencion = (elementos)Random.Range(0, 6);
        RecalcularIntencion();
    }

    public void RecalcularIntencion()
    {
        float mult = 1f;
        if (TieneEstado(elementos.Calor)) mult += 0.5f; // Enemigo hace x1.5 de da�o
        if (TieneEstado(elementos.Frio)) mult -= 0.3f;  // Enemigo hace 30% menos de da�o

        intencionActual = Mathf.RoundToInt(intencionBase * mult);

        // Actualizamos la UI en tiempo real
        if (EjecutorTablero.Instance != null) EjecutorTablero.Instance.ActualizarUIIntencion();
    }

    public void EscalarDificultad()
    {
        // Aumenta el m�ximo de da�o que puede hacer en el futuro
        danoMaximo += 2;
        Debug.Log($"La dificultad sube. El enemigo ahora puede hacer hasta {danoMaximo} de da�o.");
    }

    private void ActualizarUI()
    {
        if (textoVida != null) textoVida.text = $"{vidaActual}/{vidaMaxima}";
        if (barraVida != null)
        {
            barraVida.fillAmount = (float)vidaActual / vidaMaxima;
            if (barraVidaAmarilla != null && barraVidaAmarilla.fillAmount < barraVida.fillAmount)
                barraVidaAmarilla.fillAmount = barraVida.fillAmount;
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
