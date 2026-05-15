using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class LoginController : MonoBehaviour
{
    [Header("Referencias UI (si no las asignas, se auto-buscan por nombre)")]
    [SerializeField] private TMP_InputField ipUsuario;
    [SerializeField] private TMP_InputField ipContrasena;

    [SerializeField] private Button btnIniciarSesion;
    [SerializeField] private Button btnCrearCuenta;

    [Header("Escena destino tras login")]
    [SerializeField] private string escenaMenuPrincipal = "MenuPrincipal";

    private const string TABLA = "jugadores";

    private void Awake()
    {
        if (ipUsuario == null)
        {
            var go = GameObject.Find("IpUsuario");
            if (go != null) ipUsuario = go.GetComponent<TMP_InputField>();
        }

        if (ipContrasena == null)
        {
            var go = GameObject.Find("IpContrase?a");
            if (go != null) ipContrasena = go.GetComponent<TMP_InputField>();
        }

        if (btnIniciarSesion == null)
        {
            var go = GameObject.Find("BtnIniciarSesion");
            if (go != null) btnIniciarSesion = go.GetComponent<Button>();
        }

        if (btnCrearCuenta == null)
        {
            var go = GameObject.Find("BtnCrear");
            if (go != null) btnCrearCuenta = go.GetComponent<Button>();
        }
    }

    private void Start()
    {
        if (btnIniciarSesion != null)
            btnIniciarSesion.onClick.AddListener(OnIniciarSesion);

        if (btnCrearCuenta != null)
            btnCrearCuenta.onClick.AddListener(OnCrearCuenta);
    }

    public async void OnIniciarSesion()
    {
        try
        {
            string nombre = (ipUsuario != null) ? ipUsuario.text.Trim() : "";
            string password = (ipContrasena != null) ? ipContrasena.text : "";

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(password)) return;

            string hash = PasswordUtils.HashPassword(password);
            string query = $"/{TABLA}?nombre=eq.{Escape(nombre)}&password_hash=eq.{hash}&select=id,nombre";

            string json = await SupabaseClient.Instance.Get(query);

            if (json.Trim() == "[]") return;

            string id = "";
            var match = System.Text.RegularExpressions.Regex.Match(json, "\"id\":\"(.*?)\"");
            if (match.Success) id = match.Groups[1].Value;

            Session.JugadorId = id;
            Session.Nombre = nombre;

            RunManager.Instance.CargarJugador();

            SceneManager.LoadScene(escenaMenuPrincipal);
        }
        catch (System.Exception e) { Debug.LogError(e.Message); }
    }

    public async void OnCrearCuenta()
    {
        try
        {
            string nombre = (ipUsuario != null) ? ipUsuario.text.Trim() : "";
            string password = (ipContrasena != null) ? ipContrasena.text : "";

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(password)) return;

            // Hacemos un GET buscando exactamente ese nombre. Si existe rechazamos la creacion de la cuenta
            string checkEndpoint = $"/jugadores?select=nombre&nombre=eq.{nombre}";
            string checkResponse = await SupabaseClient.Instance.Get(checkEndpoint);

            // Si la lista tiene al menos 1 elemento, el nombre esta pillado.
            var usuariosExistentes = JsonConvert.DeserializeObject<List<RunManager.Jugador>>(checkResponse);
            if (usuariosExistentes != null && usuariosExistentes.Count > 0)
            {
                Debug.LogWarning("El nombre de usuario ya está registrado. Por favor, elige otro.");
                
                // TODO: Indicar al juador que no se ha creado la cuenta porque el nombre ya existe

                return;
            }

            string hash = PasswordUtils.HashPassword(password);
            string body = "{\"nombre\":\"" + nombre + "\",\"password_hash\":\"" + hash + "\",\"monedas\":0,\"tablero\":0,\"activo\":true,\"volumen_musica\":0.8}";

            string respuestaJugador = await SupabaseClient.Instance.Post($"/{TABLA}", body);
            var jugadoresCreados = JsonConvert.DeserializeObject<List<RunManager.Jugador>>(respuestaJugador);
            string nuevoIdUsuario = jugadoresCreados[0].id;

            // Obtener todas las mejoras existentes en el catálogo para inicializarlas
            string jsonMejoras = await SupabaseClient.Instance.Get("/mejoras?select=id");
            var listaMejorasBase = JsonConvert.DeserializeObject<List<RunManager.Mejoras>>(jsonMejoras);

            // Preparamos los datos para la tabla intermedia 'jugador_mejoras'
            // Creamos una lista para hacer un insert masivo
            List<object> mejorasIniciales = new List<object>();

            foreach (var mejora in listaMejorasBase)
            {
                mejorasIniciales.Add(new
                {
                    jugador_id = nuevoIdUsuario,
                    mejora_id = mejora.id,
                    nivel_actual = 1,
                    desbloqueada = false
                });
            }

            // Insertamos todas las mejoras de una sola vez
            string bodyMejoras = JsonConvert.SerializeObject(mejorasIniciales);
            await SupabaseClient.Instance.Post("/jugador_mejoras", bodyMejoras);

            Debug.Log("Cuenta y mejoras inicializadas con éxito.");

            // Tras crear la cuenta, intentamos iniciar sesión automáticamente
            OnIniciarSesion();
        }
        catch (System.Exception e) { Debug.LogError(e.Message); }
    }

    private string Escape(string s) => UnityEngine.Networking.UnityWebRequest.EscapeURL(s);
}
