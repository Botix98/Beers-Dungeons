using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Jugador : MonoBehaviour
{
    [Header("Estad�sticas")]
    public int vidaMaxima;
    public int vidaActual;

    [Header("Estados Alterados")]
    public List<EstadoAlterado> estadosActuales = new List<EstadoAlterado>();

    [Header("Interfaz UI")]
    public TMP_Text textoVida;
    public Image barraVida;
    public Image barraVidaAmarilla;

    private void Start()
    {
        foreach (var mejora in RunManager.Instance.mejorasJugador)
        {
            Debug.Log($"Mejora ID: {mejora.idMejora}, Nivel: {mejora.nivelActual}, Desbloqueada: {mejora.desbloqueada}");
        }
        vidaMaxima = RunManager.Instance.mejorasJugador[1].desbloqueada ? (100 + (25 * RunManager.Instance.mejorasJugador[1].nivelActual)) : 100;
        PlayerPrefs.SetInt("vidaMax", vidaMaxima);

        vidaActual = vidaMaxima;
        ActualizarUI();

        if (barraVidaAmarilla != null) barraVidaAmarilla.fillAmount = 1f;
    }

    public bool TieneEstado(elementos tipo)
    {
        foreach (var e in estadosActuales) if (e.tipo == tipo) return true;
        return false;
    }

    public void RecibirDano(int cantidad, elementos estadoAtaque)
    {
        // 1. Modificadores de da�o (Vulnerabilidades)
        float multiplicador = 1f;
        if (TieneEstado(elementos.Calor)) multiplicador += 0.5f; // Recibe x1.5 de da�o
        if (TieneEstado(elementos.Electrico)) multiplicador += 1f; // Recibe x2 de da�o

        int danoFinal = Mathf.RoundToInt(cantidad * multiplicador);

        // 2. Aplicar el da�o
        vidaActual -= danoFinal;
        if (vidaActual < 0) vidaActual = 0;

        ActualizarUI();

        // 3. Aplicar el estado del ataque (solo si te hizo m�s de 0)
        if (cantidad > 0)
        {
            AplicarEstado(estadoAtaque);
        }

        if (vidaActual == 0) Debug.Log("El jugador ha muerto. Fin de la partida.");
    }

    public void AplicarEstado(elementos nuevoEstado)
    {
        if (nuevoEstado == elementos.Fisico) return;
        if (TieneEstado(nuevoEstado)) return; // No se acumulan

        int duracion = -1; // Infinito por defecto
        if (nuevoEstado == elementos.Cortante) duracion = 3; // Cortante dura 3 turnos

        estadosActuales.Add(new EstadoAlterado(nuevoEstado, duracion));
        Debug.Log($"El jugador sufre el estado: {nuevoEstado}");
    }

    public void ProcesarEstadosAlFinalDelTurno()
    {
        for (int i = estadosActuales.Count - 1; i >= 0; i--)
        {
            EstadoAlterado estado = estadosActuales[i];

            if (estado.tipo == elementos.Calor)
            {
                int danoCalor = Mathf.Max(1, Mathf.RoundToInt(vidaMaxima * 0.05f));
                RecibirDano(danoCalor, elementos.Fisico); // Se pasa "F�sico" para que no rebote
            }
            else if (estado.tipo == elementos.Toxina)
            {
                // La toxina empieza en 2% y se multiplica x2 cada turno
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

            // Restar duraci�n y eliminar si se acaba
            if (estado.turnosRestantes > 0)
            {
                estado.turnosRestantes--;
                if (estado.turnosRestantes == 0) estadosActuales.RemoveAt(i);
            }
        }
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
            Debug.Log($"Llama Purificadora elimin� el estado: {estadosActuales[rnd]}");
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
        // Elige del 1 al 5 (se salta el 0 que es F�sico)
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
        float duracion = 0.5f; // Medio segundo de animaci�n suave
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