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

            string hash = PasswordUtils.HashPassword(password);
            string body = "{\"nombre\":\"" + nombre + "\",\"password_hash\":\"" + hash + "\",\"monedas\":0,\"tablero\":0,\"activo\":true,\"volumen_musica\":0.8}";

            await SupabaseClient.Instance.Post($"/{TABLA}", body);
        }
        catch (System.Exception e) { Debug.LogError(e.Message); }
    }

    private string Escape(string s) => UnityEngine.Networking.UnityWebRequest.EscapeURL(s);
}
