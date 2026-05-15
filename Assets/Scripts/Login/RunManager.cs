using UnityEngine;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

public class RunManager : MonoBehaviour
{
    [System.Serializable]
    public class Jugador
    {
        public string id;
        public string nombre;
        public int tablero;
        public int monedas;
        [JsonProperty("volumen_musica")]
        public float volumenMusica;
    }

    [System.Serializable]
    public class MejorasJugador
    {
        [JsonProperty("mejora_id")]
        public long idMejora;
        [JsonProperty("nivel_actual")]
        public int nivelActual;
        public bool desbloqueada;
        [JsonProperty("adquirida_at")]
        public System.DateTime fechaAdquisicion;
        
    }
    [System.Serializable]
    public class Mejoras
    {
        public long id;
        [JsonProperty("tipo")]
        public string tipoMejora;
        public int nivel;
    }

    [SerializeField] public Jugador jugador;
    [SerializeField] public List<MejorasJugador> mejorasJugador;
    [SerializeField] private List<Mejoras> mejoras;

    public static RunManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async void CargarJugador()
    {
        string columnas = "nombre,tablero,monedas,volumen_musica";
        string endpoint = $"/jugadores?select={columnas}&id=eq.{Session.JugadorId}";

        try 
        {
            string jsonResponse = await SupabaseClient.Instance.Get(endpoint);
            // Convertimos la respuesta a una lista de jugadores (aunque solo esperamos uno)
            List<Jugador> jugadoresObtenidos = JsonConvert.DeserializeObject<List<Jugador>>(jsonResponse);
            if (jugadoresObtenidos != null && jugadoresObtenidos.Count > 0)
            {
                jugador = jugadoresObtenidos[0];
                Debug.Log("Jugador cargado correctamente.");

                CargarMejorasJugador();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }
    public async void CargarMejorasJugador()
    {
        string endpoint = $"/jugador_mejoras?select=mejora_id,nivel_actual,desbloqueada,adquirida_at&jugador_id=eq.{Session.JugadorId}&order=mejora_id.asc";

        try
        {
            string jsonResponse = await SupabaseClient.Instance.Get(endpoint);
            mejorasJugador = JsonConvert.DeserializeObject<List<MejorasJugador>>(jsonResponse);
            Debug.Log("Mejoras del jugador cargadas correctamente.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }

    public async void ActualizarMonedasJugador()
    {
        // 3. Preparamos el paquete JSON solo con el campo de las monedas
        var datosNuevos = new
        {
            monedas = jugador.monedas
        };

        string jsonBody = JsonConvert.SerializeObject(datosNuevos);

        // 4. Preparamos la ruta exacta usando el ID del jugador
        string endpoint = $"/jugadores?id=eq.{Session.JugadorId}";

        try
        {
            // 5. Enviamos la actualización a la base de datos con PATCH
            await SupabaseClient.Instance.Patch(endpoint, jsonBody);
            
            Debug.Log($"Monedas guardadas en la nube correctamente. Nuevo saldo: {jugador.monedas}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error de conexión al guardar monedas: " + e.Message);
        }
    }

    public async void ActualizarMejorasJugador()
    {
        // 1. Verificamos que la lista exista y tenga datos antes de hacer nada
        if (mejorasJugador == null || mejorasJugador.Count == 0)
        {
            Debug.LogWarning("No hay mejoras en la lista para actualizar.");
            return;
        }

        try
        {
            // 2. Recorremos todas las mejoras que tiene el jugador cargadas
            foreach (var mejora in mejorasJugador)
            {
                // Creamos un objeto anonimo solo con los campos que pueden haber cambiado
                var datosActualizados = new
                {
                    nivel_actual = mejora.nivelActual,
                    desbloqueada = mejora.desbloqueada
                };

                string jsonBody = JsonConvert.SerializeObject(datosActualizados);

                // 3. Armamos el endpoint filtrando por el ID del jugador Y el ID de esta mejora específica
                string endpoint = $"/jugador_mejoras?jugador_id=eq.{Session.JugadorId}&mejora_id=eq.{mejora.idMejora}";

                // 4. Enviamos la actualización a Supabase
                await SupabaseClient.Instance.Patch(endpoint, jsonBody);
            }

            Debug.Log("Todas las mejoras del jugador se han guardado en la base de datos correctamente");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al intentar actualizar las mejoras: " + e.Message);
        }
    }
}
