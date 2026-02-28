using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EjecutorTablero : MonoBehaviour
{
    public static EjecutorTablero Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [Header("Referencias de la UI")]
    public Transform contenedorTablero;
    public Transform contenedorInventario;

    public TMP_Text txtIntencion;

    [Header("Referencias de Juego")]
    public Enemigo enemigoActual;
    public Jugador jugadorActual;

    [Header("Fases de Turno")]
    [Tooltip("Tiempo en segundos que espera entre cada hechizo")]
    public float tiempoEntreHechizos = 0.5f;
    public TMP_Text txtProgramacion; // TxtTurno1
    public TMP_Text txtResolucion;   // TxtTurno2

    // Seguro para no poder darle al botón 20 veces seguidas
    private bool ejecutando = false;

    private void Start()
    {
        // Genera la intención del primer turno nada más arrancar el juego
        if (enemigoActual != null)
        {
            enemigoActual.GenerarIntencion();
            ActualizarUIIntencion();
        }

        // Al empezar el juego estes en la fase de programación (fase 1)
        CambiarFaseVisual(true);
    }

    // Esta es la función que llama el boton
    public void EjecutarHechizos()
    {
        // Ignora los clic si esta en ejecución
        if (ejecutando) return;

        StartCoroutine(RutinaEjecucion());
    }

    private IEnumerator RutinaEjecucion()
    {
        ejecutando = true;

        // 1. Cambia la UI a Fase 2 (Resolución)
        CambiarFaseVisual(false);

        // 2. Obtener y ordenar los slots
        List<Transform> listaSlots = new List<Transform>();
        foreach (Transform hijo in contenedorTablero)
        {
            listaSlots.Add(hijo);
        }

        listaSlots.Sort((a, b) =>
        {
            string coordenadaA = a.name.Substring(a.name.IndexOf('_') + 1);
            string coordenadaB = b.name.Substring(b.name.IndexOf('_') + 1);
            return coordenadaA.CompareTo(coordenadaB);
        });

        // --- Memoria para los bufos ---
        elementos? elementoHeredado = null;
        int repeticionesHeredadas = 1;

        // Resolución de los hechizos del jugador
        for (int i = 0; i < listaSlots.Count; i++)
        {
            Transform slot = listaSlots[i];

            if (slot.childCount == 0)
            {
                // Un hueco vacío rompe el combo
                elementoHeredado = null;
                repeticionesHeredadas = 1;
                continue;
            }

            Transform hechizo = slot.GetChild(0);
            Atributos atributosHechizo = hechizo.GetComponent<Atributos>();

            // Gasta un uso del hechizo al activarse
            if (atributosHechizo != null) atributosHechizo.GastarUso();

            // Identifica el tipo de hechizo para aplicar su lógica específica
            HechizoATQ atq = hechizo.GetComponent<HechizoATQ>();
            HechizoDEF def = hechizo.GetComponent<HechizoDEF>();
            HechizoBUF buf = hechizo.GetComponent<HechizoBUF>();

            string nombreHechizoOriginal = hechizo.gameObject.name.Replace("(Clone)", "").Trim();
            string nombreNormalizado = nombreHechizoOriginal.Replace(" ", "").ToLower();

            // Lógica de BUF
            if (buf != null)
            {
                if (nombreNormalizado.Contains("doppelganger"))
                {
                    repeticionesHeredadas = buf.golpear > 1 ? buf.golpear : 2;
                }
                else
                {
                    elementoHeredado = buf.elemento;
                }

                System.Action accionEnElImpacto = () => { Debug.Log($"Bufo aplicado: {nombreHechizoOriginal}"); };
                EfectoZarandeo efecto = hechizo.GetComponent<EfectoZarandeo>();
                if (efecto == null) efecto = hechizo.gameObject.AddComponent<EfectoZarandeo>();

                yield return StartCoroutine(efecto.EjecutarSacudida(10f, 1, accionEnElImpacto));
            }
            // Lógica de ATQ y DEF
            else if (atq != null || def != null)
            {
                for (int rep = 0; rep < repeticionesHeredadas; rep++)
                {
                    // Variables para configurar cómo será la sacudida y el daño
                    float intensidadVisual = 0f;
                    int numeroDeGolpes = 1;
                    System.Action accionEnElImpacto = null;

                    // Lógica de ATQ
                    if (atq != null)
                    {
                        // Calculamos el daño real usando el multiplicador y el número asignado
                        float mult = atq.multiplicador > 0 ? atq.multiplicador : 1f;
                        numeroDeGolpes = atq.golpear > 0 ? atq.golpear : 1;

                        int danoBase = atq.ObtenerDano();
                        int danoPorGolpe = Mathf.RoundToInt(danoBase * mult);

                        // La fuerza de la sacudida depende de cuánto daño haga el golpe
                        intensidadVisual = Mathf.Clamp(5f + (danoPorGolpe * 0.5f), 5f, 40f);

                        // Determina si usa el elemento del ataque o el del bufo
                        elementos elementoFinal = elementoHeredado.HasValue ? elementoHeredado.Value : atq.elemento;

                        // Esta acción se ejecuta en el punto más alto del salto del hechizo
                        accionEnElImpacto = () => {
                            if (danoPorGolpe > 0 && enemigoActual != null)
                            {
                                Debug.Log($"Golpe de {danoPorGolpe} de daño con elemento {elementoFinal}.");
                                enemigoActual.RecibirDano(danoPorGolpe, elementoFinal); // Resta vida al enemigo
                            }
                        };
                    }
                    // Lógica de DEF
                    else if (def != null)
                    {
                        numeroDeGolpes = 1;
                        intensidadVisual = 15f;

                        // Saca el nombre del hechizo (limpiando el "(Clone)" que añade Unity al crear prefabs)
                        string nombreHechizo = hechizo.gameObject.name.Replace("(Clone)", "").Trim();

                        accionEnElImpacto = () => {
                            Debug.Log($"Ejecutando defensa: {nombreHechizo}");

                            // 1. EFECTOS BÁSICOS (Curación y Reducción)
                            if (def.Curacion > 0)
                            {
                                int cura = Mathf.RoundToInt(jugadorActual.vidaMaxima * def.Curacion);
                                jugadorActual.Curar(cura);
                            }

                            if (def.Reduccion > 0 && enemigoActual != null)
                            {
                                enemigoActual.intencionActual = Mathf.RoundToInt(enemigoActual.intencionActual * def.Reduccion);
                                ActualizarUIIntencion(); // El número del enemigo cambia al instante en pantalla
                            }

                            // 2. EFECTOS ÚNICOS SEGÚN EL NOMBRE DEL HECHIZO
                            if (nombreHechizo.Contains("LlamaPurificadora"))
                            {
                                jugadorActual.QuitarEstadoAleatorio();
                            }
                            else if (nombreHechizo.Contains("SolSagrado"))
                            {
                                jugadorActual.LimpiarEstados();
                            }
                            else if (nombreHechizo.Contains("EscudoDeDobleFilo"))
                            {
                                jugadorActual.AplicarEstadoAleatorio();
                                jugadorActual.AplicarEstadoAleatorio();
                            }
                            else if (nombreHechizo.Contains("RuedaDeLaDiosa") && enemigoActual != null)
                            {
                                // A) Suma el porcentaje de vida de ambos
                                float pctJugador = (float)jugadorActual.vidaActual / jugadorActual.vidaMaxima;
                                float pctEnemigo = (float)enemigoActual.vidaActual / enemigoActual.vidaMaxima;
                                float pctTotal = pctJugador + pctEnemigo;

                                // B) La ruleta gira: un número aleatorio entre 0.0 y 1.0 (0% a 100%)
                                float suerteJugador = Random.Range(0f, 1f);

                                // Calculamos qué parte del botín total se lleva cada uno
                                float pctParaJugador = pctTotal * suerteJugador;
                                float pctParaEnemigo = pctTotal * (1f - suerteJugador);

                                jugadorActual.FijarVida(Mathf.RoundToInt(jugadorActual.vidaMaxima * pctParaJugador));
                                enemigoActual.FijarVida(Mathf.RoundToInt(enemigoActual.vidaMaxima * pctParaEnemigo));

                                // C) Piscina de estados (Junta los de ambos sin duplicar)
                                List<elementos> poolEstados = new List<elementos>();
                                foreach (var est in jugadorActual.estadosActuales) if (!poolEstados.Contains(est)) poolEstados.Add(est);
                                foreach (var est in enemigoActual.estadosActuales) if (!poolEstados.Contains(est)) poolEstados.Add(est);

                                jugadorActual.LimpiarEstados();
                                enemigoActual.LimpiarEstados();

                                // D) Repartir estados al azar (puede que a uno le toquen todos y al otro ninguno)
                                foreach (var estado in poolEstados)
                                {
                                    // 50% de probabilidad (cara o cruz) para cada estado individual
                                    if (Random.value > 0.5f)
                                    {
                                        jugadorActual.AplicarEstado(estado);
                                    }
                                    else
                                    {
                                        enemigoActual.AplicarEstado(estado);
                                    }
                                }
                            }
                        };
                    }

                    // Gestiona el componente de sacudida y espera a que termine la animación
                    EfectoZarandeo efecto = hechizo.GetComponent<EfectoZarandeo>();
                    if (efecto == null) efecto = hechizo.gameObject.AddComponent<EfectoZarandeo>();

                    // El programa se frena aquí hasta que el hechizo termine de dar todos sus golpes
                    yield return StartCoroutine(efecto.EjecutarSacudida(intensidadVisual, numeroDeGolpes, accionEnElImpacto));

                    // Pausa si el Doppelganger hace que se repita
                    if (repeticionesHeredadas > 1 && rep < repeticionesHeredadas - 1)
                        yield return new WaitForSeconds(0.2f);
                }

                // Reseteamos el bufo para que no afecte a las casillas posteriores
                elementoHeredado = null;
                repeticionesHeredadas = 1;
            }

            // Reinicia el daño del hechizo para que no se guarde al volver al inventario
            if (atq != null) atq.EstablecerDano(0);

            // Limpia los números usados en el cuadro blanco
            NumSlot cuadroBlanco = hechizo.GetComponentInChildren<NumSlot>();
            if (cuadroBlanco != null)
            {
                foreach (Transform numero in cuadroBlanco.transform)
                {
                    Destroy(numero.gameObject);
                }
            }

            // lo devuelve al inventario y reordena la lista
            hechizo.SetParent(contenedorInventario);
            OrdenarInventarioCompleto();

            // Pausa antes de pasar a la siguiente casilla del tablero
            yield return new WaitForSeconds(tiempoEntreHechizos);
        }

        if (enemigoActual != null && jugadorActual != null && enemigoActual.vidaActual > 0)
        {
            Debug.Log("Turno del enemigo...");
            yield return new WaitForSeconds(0.5f); // Para que se note el cambio de turno

            // El enemigo te golpea con el daño indicado
            jugadorActual.RecibirDano(enemigoActual.intencionActual, enemigoActual.elementoIntencion);

            // Se prepara para el siguiente turno: sube la dificultad y elige un nuevo ataque
            enemigoActual.EscalarDificultad();
            enemigoActual.GenerarIntencion();
            ActualizarUIIntencion();
        }

        // 4. Termina la ejecución, vuelve a la Fase 1 (Programación)
        CambiarFaseVisual(true);
        ejecutando = false;

        Debug.Log("Fase de resolución terminada.");
    }

    private void ActualizarUIIntencion()
    {
        if (txtIntencion != null && enemigoActual != null)
        {
            txtIntencion.text = enemigoActual.intencionActual.ToString();

            // Pintamos el texto según el elemento que tocó
            switch (enemigoActual.elementoIntencion)
            {
                case elementos.Fisico:
                    txtIntencion.color = Color.white; // Blanco
                    break;
                case elementos.Cortante:
                    txtIntencion.color = new Color(0.7f, 0f, 0f); // Rojo sangre
                    break;
                case elementos.Calor:
                    txtIntencion.color = new Color(0.9f, 0.4f, 0f); // Naranja oscuro
                    break;
                case elementos.Frio:
                    txtIntencion.color = new Color(0.2f, 0.8f, 1f); // Celeste
                    break;
                case elementos.Toxina:
                    txtIntencion.color = new Color(0f, 0.5f, 0f); // Verde oscuro
                    break;
                case elementos.Electrico:
                    txtIntencion.color = new Color(0.3f, 0.1f, 0.6f); // Azul oscuro tirando a morado
                    break;
            }
        }
    }

    public void OrdenarInventarioCompleto()
    {
        List<Transform> todosLosHechizos = new List<Transform>();
        foreach (Transform t in contenedorInventario)
        {
            todosLosHechizos.Add(t);
        }

        todosLosHechizos.Sort((a, b) =>
        {
            Atributos attrA = a.GetComponent<Atributos>();
            Atributos attrB = b.GetComponent<Atributos>();
            if (attrA == null || attrB == null) return 0;

            int pesoA = (attrA.usosActuales <= 0) ? 1 : 0;
            int pesoB = (attrB.usosActuales <= 0) ? 1 : 0;
            return pesoA.CompareTo(pesoB);
        });

        for (int i = 0; i < todosLosHechizos.Count; i++)
        {
            todosLosHechizos[i].SetSiblingIndex(i);
        }
    }

    // Método para oscurecer e iluminar los textos
    private void CambiarFaseVisual(bool esFaseProgramacion)
    {
        if (txtProgramacion != null && txtResolucion != null)
        {
            Color colorEncendido = Color.white;
            Color colorApagado = new Color(1f, 1f, 1f, 0.4f); // Mismo color pero con 40% de opacidad (oscurecido)

            txtProgramacion.color = esFaseProgramacion ? colorEncendido : colorApagado;
            txtResolucion.color = esFaseProgramacion ? colorApagado : colorEncendido;
        }
    }
}