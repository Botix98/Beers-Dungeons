using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class EjecutorTablero : MonoBehaviour
{
    public static EjecutorTablero Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [Header("UI e Información del Piso")]
    public TMP_Text txtPiso;
    public TMP_Text txtRecompensa;
    public int oroBasePorVictoria = 1000;

    private int oroQueVoyAGanar;

    [Header("Pantallas de Fin de Partida")]
    public GameObject panelVictoria;
    public GameObject panelDerrota;
    public TMP_Text txtVictoria;
    public TMP_Text txtDerrota;

    [Header("Sonidos de Fin de Partida")]
    public AudioClip sonidoVictoria;
    public AudioClip sonidoDerrota;

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

    // para que la memoria del Doppelganger y los elementos funcione entre las vueltas del bucle.
    private elementos? elementoHeredado = null;
    private int repeticionesHeredadas = 1;

    private void Start()
    {
        int pisoActual = PlayerPrefs.GetInt("PisoActual", 1);
        if (txtPiso != null) txtPiso.text = "Piso: " + pisoActual;

        oroQueVoyAGanar = Mathf.RoundToInt(oroBasePorVictoria * Mathf.Pow(1.25f, pisoActual - 1));
        if (txtRecompensa != null) txtRecompensa.text = oroQueVoyAGanar.ToString();

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

        // Se prepara para la lectura de los marcos
        List<ObjetoBucle> todosLosBucles = new List<ObjetoBucle>(contenedorTablero.parent.GetComponentsInChildren<ObjetoBucle>());
        List<Transform> slotsYaProcesadosEnBucle = new List<Transform>();

        // Reiniciamos memorias de combos
        elementoHeredado = null;
        repeticionesHeredadas = 1;

        // Resolución de los hechizos del jugador
        for (int i = 0; i < listaSlots.Count; i++)
        {
            Transform slotActual = listaSlots[i];

            // Si esta casilla ya se ejecutó porque era parte de un bucle, la saltamos.
            if (slotsYaProcesadosEnBucle.Contains(slotActual)) continue;

            // Comprobamos si hay un Marco Negro encima de esta casilla
            ObjetoBucle bucleDeEsteSlot = ObtenerBucleDeSlot(slotActual, todosLosBucles);

            if (bucleDeEsteSlot != null)
            {
                // ==========================================
                // LÓGICA DE BUCLE (Se ejecuta varias veces)
                // ==========================================

                // Juntamos todas las casillas que están bajo este marco negro
                List<Transform> slotsDelBucle = new List<Transform>();
                foreach (Transform s in listaSlots)
                {
                    if (ObtenerBucleDeSlot(s, todosLosBucles) == bucleDeEsteSlot)
                    {
                        slotsDelBucle.Add(s);
                        slotsYaProcesadosEnBucle.Add(s);
                    }
                }

                int iteracionesMax = bucleDeEsteSlot.iteraciones;

                float multiplicadorVelocidad = 1f;

                for (int iter = 0; iter < iteracionesMax; iter++)
                {
                    bool esPrimeraVuelta = (iter == 0); // Para gastar uso solo la primera vez

                    // Ejecutar los hechizos
                    foreach (Transform s in slotsDelBucle)
                    {
                        if (s.childCount > 0)
                        {
                            yield return StartCoroutine(EjecutarCasilla(s, esPrimeraVuelta, multiplicadorVelocidad));
                            yield return new WaitForSeconds(tiempoEntreHechizos / multiplicadorVelocidad);
                        }
                    }

                    // Animación de difuminado del número del marco (si no es la última vuelta)
                    if (iter < iteracionesMax - 1)
                    {
                        yield return StartCoroutine(AnimarReduccionNumeros(slotsDelBucle, bucleDeEsteSlot, iteracionesMax - iter - 1, multiplicadorVelocidad));
                    }

                    // Acelera la ejecución un 40% para la siguiente vuelta
                    multiplicadorVelocidad *= 1.4f;
                }

                // Al final del bucle limpiamos las cartas y las devolvemos
                foreach (Transform s in slotsDelBucle)
                {
                    LimpiarYDevolverHechizo(s);
                }
            }
            else
            {
                // ==========================================
                // LÓGICA NORMAL (Un solo hechizo, sin bucle)
                // ==========================================

                if (slotActual.childCount > 0)
                {
                    yield return StartCoroutine(EjecutarCasilla(slotActual, true, 1f));
                    LimpiarYDevolverHechizo(slotActual);
                    yield return new WaitForSeconds(tiempoEntreHechizos);
                }
            }
        }

        // Limpia los números usados en el cuadro blanco de los bucles para el próximo turno
        foreach (var bucle in todosLosBucles)
        {
            NumSlot cuadroBucle = bucle.GetComponentInChildren<NumSlot>();
            if (cuadroBucle != null)
            {
                foreach (Transform num in cuadroBucle.transform) Destroy(num.gameObject);
            }
            bucle.iteraciones = 1;
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
        }

        if (jugadorActual != null) jugadorActual.ProcesarEstadosAlFinalDelTurno();
        if (enemigoActual != null) enemigoActual.ProcesarEstadosAlFinalDelTurno();

        if (jugadorActual != null) jugadorActual.EjecutarAnimacionDano();
        if (enemigoActual != null) enemigoActual.EjecutarAnimacionDano();

        yield return new WaitForSeconds(0.6f);

        // 4. Termina la ejecución, vuelve a la Fase 1 (Programación)
        CambiarFaseVisual(true);
        ejecutando = false;

        Debug.Log("Fase de resolución terminada.");
    }

    // ===================================================================================
    // EL "CEREBRO" DEL HECHIZO (Empaquetado para llamarlo desde bucles o normal)
    // ===================================================================================
    private IEnumerator EjecutarCasilla(Transform slot, bool gastarUsos, float velocidadActual)
    {
        if (slot.childCount == 0)
        {
            // Un hueco vacío rompe el combo
            elementoHeredado = null;
            repeticionesHeredadas = 1;
            yield break;
        }

        Transform hechizo = slot.GetChild(0);
        Atributos atributosHechizo = hechizo.GetComponent<Atributos>();

        // Gasta un uso del hechizo al activarse (solo si toca)
        if (gastarUsos && atributosHechizo != null) atributosHechizo.GastarUso();

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

            yield return StartCoroutine(efecto.EjecutarSacudida(10f, 1, accionEnElImpacto, velocidadActual));
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
                            enemigoActual.intencionBase = Mathf.RoundToInt(enemigoActual.intencionBase * def.Reduccion);
                            enemigoActual.RecalcularIntencion();
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

                            // C) pool de estados (Junta los de ambos sin duplicar)
                            List<elementos> poolEstados = new List<elementos>();
                            foreach (var est in jugadorActual.estadosActuales) if (!poolEstados.Contains(est.tipo)) poolEstados.Add(est.tipo);
                            foreach (var est in enemigoActual.estadosActuales) if (!poolEstados.Contains(est.tipo)) poolEstados.Add(est.tipo);

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
                yield return StartCoroutine(efecto.EjecutarSacudida(intensidadVisual, numeroDeGolpes, accionEnElImpacto, velocidadActual));
                // Pausa si el Doppelganger hace que se repita
                if (repeticionesHeredadas > 1 && rep < repeticionesHeredadas - 1)
                    yield return new WaitForSeconds(0.2f / velocidadActual);
            }

            // Reseteamos el bufo para que no afecte a las casillas posteriores
            elementoHeredado = null;
            repeticionesHeredadas = 1;
        }
    }

    // ===================================================================================
    // FUNCIONES AUXILIARES PARA LIMPIEZA Y LÓGICA DE BUCLES
    // ===================================================================================

    // Devuelve el hechizo a su forma original y lo manda al inventario
    private void LimpiarYDevolverHechizo(Transform slot)
    {
        if (slot.childCount == 0) return;
        Transform hechizo = slot.GetChild(0);

        // Reinicia el daño del hechizo para que no se guarde al volver al inventario
        HechizoATQ atq = hechizo.GetComponent<HechizoATQ>();
        if (atq != null) atq.EstablecerDano(0);

        // Limpia los números usados en el cuadro blanco del hechizo
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
    }

    // Magia matemática para comprobar si un slot está dentro de un bucle
    private ObjetoBucle ObtenerBucleDeSlot(Transform slot, List<ObjetoBucle> todosLosBucles)
    {
        Vector3[] slotCorners = new Vector3[4];
        slot.GetComponent<RectTransform>().GetWorldCorners(slotCorners);
        Vector3 slotCenter = (slotCorners[0] + slotCorners[2]) / 2f;

        foreach (var b in todosLosBucles)
        {
            Vector3[] bCorners = new Vector3[4];
            b.GetComponent<RectTransform>().GetWorldCorners(bCorners);
            if (slotCenter.x >= bCorners[0].x && slotCenter.x <= bCorners[2].x &&
                slotCenter.y >= bCorners[0].y && slotCenter.y <= bCorners[1].y)
            {
                return b;
            }
        }
        return null;
    }

    public void ActualizarUIIntencion()
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
    // Anima TODO a la vez (Bucle y Hechizos) con Fade in / Fade out
    private IEnumerator AnimarReduccionNumeros(List<Transform> slotsDelBucle, ObjetoBucle bucle, int nuevoValorBucle, float velocidadActual)
    {
        List<TMP_Text> todosLosTextos = new List<TMP_Text>();
        List<DraggableNum> todosLosNums = new List<DraggableNum>();
        List<int> nuevosValores = new List<int>();
        List<Color> coloresOriginales = new List<Color>();

        // 1. Recopilar el número del marco negro (El Bucle)
        DraggableNum numBucle = bucle.GetComponentInChildren<DraggableNum>();
        if (numBucle != null)
        {
            TMP_Text txtBucle = numBucle.GetComponentInChildren<TMP_Text>();
            if (txtBucle != null)
            {
                todosLosTextos.Add(txtBucle);
                todosLosNums.Add(numBucle);
                nuevosValores.Add(nuevoValorBucle);
                coloresOriginales.Add(txtBucle.color);
            }
        }

        // 2. Recopilar los números pegados en los Hechizos
        foreach (Transform s in slotsDelBucle)
        {
            if (s.childCount > 0)
            {
                Transform hechizo = s.GetChild(0);
                DraggableNum numHechizo = hechizo.GetComponentInChildren<DraggableNum>();

                if (numHechizo != null && numHechizo.valor > 0)
                {
                    TMP_Text txtHechizo = numHechizo.GetComponentInChildren<TMP_Text>();
                    if (txtHechizo != null)
                    {
                        todosLosTextos.Add(txtHechizo);
                        todosLosNums.Add(numHechizo);
                        nuevosValores.Add(numHechizo.valor - 1); // Al hechizo se le resta 1
                        coloresOriginales.Add(txtHechizo.color);

                        // Avisamos al hechizo para que recalcule el daño en base al nuevo valor
                        HechizoATQ atq = hechizo.GetComponent<HechizoATQ>();
                        if (atq != null) atq.EstablecerDano(numHechizo.valor - 1);
                    }
                }
            }
        }

        // Si no hay números que animar, salimos de la corrutina
        if (todosLosTextos.Count == 0) yield break;

        // La velocidad del Fade aumenta según lo rápido que vaya el bucle
        float velocidadFade = 4f * velocidadActual;

        // 3. Animación FADE OUT (Todos a la vez)
        for (float alpha = 1f; alpha >= 0f; alpha -= Time.deltaTime * velocidadFade)
        {
            for (int i = 0; i < todosLosTextos.Count; i++)
            {
                Color c = coloresOriginales[i];
                todosLosTextos[i].color = new Color(c.r, c.g, c.b, alpha);
            }
            yield return null;
        }

        // 4. Cambiamos los valores numéricos estando invisibles
        for (int i = 0; i < todosLosTextos.Count; i++)
        {
            todosLosNums[i].valor = nuevosValores[i];
            todosLosTextos[i].text = nuevosValores[i].ToString();
        }

        // 5. Animación FADE IN (Todos a la vez)
        for (float alpha = 0f; alpha <= 1f; alpha += Time.deltaTime * velocidadFade)
        {
            for (int i = 0; i < todosLosTextos.Count; i++)
            {
                Color c = coloresOriginales[i];
                todosLosTextos[i].color = new Color(c.r, c.g, c.b, alpha);
            }
            yield return null;
        }

        // Asegurarnos de que el canal alpha se queda perfectamente en 1 al terminar
        for (int i = 0; i < todosLosTextos.Count; i++)
        {
            Color c = coloresOriginales[i];
            todosLosTextos[i].color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    // ===================================================================================
    // FIN DE PARTIDA
    // ===================================================================================
    public void MostrarVictoria()
    {
        StopAllCoroutines();
        ejecutando = false;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sonidoVictoria);

        int pisoActual = PlayerPrefs.GetInt("PisoActual", 1);
        int monedasActuales = PlayerPrefs.GetInt("monedas", 0);

        PlayerPrefs.SetInt("monedas", monedasActuales + oroQueVoyAGanar);

        if (txtVictoria != null)
        {
            int siguientePiso = pisoActual + 1;
            txtVictoria.text = $"¡Lo lograste! Obtuviste {oroQueVoyAGanar} de oro, ya puedes avanzar al piso {siguientePiso}; no olvides comprar en la tienda. :)";
        }

        Debug.Log($"¡Piso {pisoActual} superado! Ganaste {oroQueVoyAGanar} monedas.");

        if (panelVictoria != null) panelVictoria.SetActive(true);
    }

    public void MostrarDerrota()
    {
        // Detiene instantaneamente todo
        StopAllCoroutines();
        ejecutando = false;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(sonidoDerrota);

        // Calculamos las Anécdotas (1 por cada piso superado)
        int pisoActual = PlayerPrefs.GetInt("PisoActual", 1);
        int anecdotasGanadas = Mathf.Max(0, pisoActual - 1);

        // Se las sumamos al jugador de forma permanente usando tu RunManager
        if (RunManager.Instance != null && RunManager.Instance.jugador != null)
        {
            RunManager.Instance.jugador.monedas += anecdotasGanadas;
            RunManager.Instance.ActualizarMonedasJugador();
        }

        // Actualizamos el texto gigante del pergamino
        if (txtDerrota != null)
        {
            txtDerrota.text = $"Hola. :)\n" +
                $"Si lees esto, que sepas que te desmayaste por un GOBLIN, esa criatura del demonio te intentó robar tu preciada bola de cristal >:\\ , pero un humilde servidor te salvó la vida, arrastrando tu moribundo cuerpo por la mazmorra hasta la salida, salvándote la vida, por pura caridad. De nada :).\n\n" +
                $"Durante nuestra incursión hasta el piso {pisoActual} de la mazmorra conseguiste muchos objetos, así que te los guardé para que no los pierdas, ya te los devolveré algún día.\n\n" +
                $"Conseguiste un total de {anecdotasGanadas} logros durante la incursión, no dudes en contar tus hazañas en la taberna, con suerte te invitan a algún trago y, ya que estamos, pues me compartes un poquito, ¿no?\n\n" +
                $"Atentamente, tu querido compañero.\n" +
                $"P.D. Si te vuelves a aventurar en la mazmorra, no dudes en avisarme, jeje.";
        }

        // Mostramos el panel de derrota
        if (panelDerrota != null) panelDerrota.SetActive(true);
    }
}