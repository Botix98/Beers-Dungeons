using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        // === Auto-buscar inputs si no están asignados ===
        if (ipUsuario == null)
        {
            var go = GameObject.Find("IpUsuario");
            if (go != null) ipUsuario = go.GetComponent<TMP_InputField>();
        }

        if (ipContrasena == null)
        {
            var go = GameObject.Find("IpContraseña");
            if (go != null) ipContrasena = go.GetComponent<TMP_InputField>();
        }

        // === Auto-buscar botones si no están asignados ===
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
        // Enganchar eventos OnClick automáticamente (sin tocar el inspector del botón)
        if (btnIniciarSesion != null)
            btnIniciarSesion.onClick.AddListener(OnIniciarSesion);

        if (btnCrearCuenta != null)
            btnCrearCuenta.onClick.AddListener(OnCrearCuenta);

        // Seguridad básica: si falta algo, avisamos en consola.
        if (ipUsuario == null) Debug.LogError("No se encontró IpUsuario (TMP_InputField).");
        if (ipContrasena == null) Debug.LogError("No se encontró IpContraseña (TMP_InputField).");
        if (btnIniciarSesion == null) Debug.LogError("No se encontró BtnIniciarSesion (Button).");
        if (btnCrearCuenta == null) Debug.LogError("No se encontró BtnCrear (Button).");
    }

    // =====================
    // INICIAR SESIÓN
    // =====================
    public async void OnIniciarSesion()
    {
        try
        {
            string nombre = (ipUsuario != null) ? ipUsuario.text.Trim() : "";
            string password = (ipContrasena != null) ? ipContrasena.text : "";

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("Campos vacíos.");
                return;
            }

            string hash = PasswordUtils.HashPassword(password);

            string query =
                $"/{TABLA}?nombre=eq.{Escape(nombre)}" +
                $"&password_hash=eq.{hash}" +
                $"&select=id,nombre";

            string json = await SupabaseClient.Instance.Get(query);

            if (json.Trim() == "[]")
            {
                Debug.LogWarning("Usuario o contraseña incorrectos.");
                return;
            }

            // “Sesión” local
            PlayerPrefs.SetString("jugador_nombre", nombre);
            PlayerPrefs.Save();

            SceneManager.LoadScene(escenaMenuPrincipal);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    // =====================
    // CREAR CUENTA
    // =====================
    public async void OnCrearCuenta()
    {
        try
        {
            string ping = await SupabaseClient.Instance.Get("/?select=1");
            Debug.Log("PING: " + ping);
            string nombre = (ipUsuario != null) ? ipUsuario.text.Trim() : "";
            string password = (ipContrasena != null) ? ipContrasena.text : "";

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("Campos vacíos.");
                return;
            }

            // comprobar si ya existe el usuario en la base de datos
            string existe = await SupabaseClient.Instance.Get(
                $"/{TABLA}?nombre=eq.{Escape(nombre)}&select=id"
            );

            if (existe.Trim() != "[]")
            {
                Debug.LogWarning("El usuario ya existe.");
                return;
            }

            string hash = PasswordUtils.HashPassword(password);

            string body =
                "{"
                + $"\"nombre\":\"{nombre}\","
                + $"\"password_hash\":\"{hash}\","
                + "\"monedas\":0,"
                + "\"tablero\":0,"
                + "\"activo\":true,"
                + "\"volumen_musica\":0.8"
                + "}";

            await SupabaseClient.Instance.Post($"/{TABLA}", body);

            Debug.Log("Cuenta creada correctamente. Ya puedes iniciar sesión.");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private string Escape(string s) => UnityEngine.Networking.UnityWebRequest.EscapeURL(s);
}
