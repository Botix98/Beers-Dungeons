using System;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AjustesController : MonoBehaviour
{
    [Header("UI (si no asignas, se auto-buscan por nombre)")]
    [SerializeField] private Slider sliderVolumen;
    [SerializeField] private Button btnCerrarSesion;
    [SerializeField] private Button btnEliminarCuenta;

    [Header("Escena a la que volver tras cerrar/eliminar")]
    [SerializeField] private string escenaLogin = "login";

    private float _saveDebounceSeconds = 0.35f;
    private float _pendingSaveTime = -1f;
    private float _pendingVolume = -1f;

    private async void Start()
    {
        Debug.Log("[AjustesController] Start() OK");

        AutoFindUIIfNeeded();
        HookUIEvents();

        // Si no hay sesión, volvemos a login
        if (!Session.IsLoggedIn)
        {
            Debug.LogWarning("[AjustesController] No hay sesión. Volviendo a login.");
            SceneManager.LoadScene(escenaLogin);
            return;
        }

        // Carga volumen desde BD y aplica
        await CargarVolumenDesdeBD();
    }

    private void Update()
    {
        // Guardado con debounce (para no mandar 100 PATCH mientras arrastras el slider)
        if (_pendingSaveTime > 0 && Time.time >= _pendingSaveTime)
        {
            _pendingSaveTime = -1f;
            _ = GuardarVolumenEnBD(_pendingVolume); // fire & forget
        }
    }

    private void AutoFindUIIfNeeded()
    {
        if (sliderVolumen == null)
            sliderVolumen = GameObject.Find("Slider")?.GetComponent<Slider>();

        if (btnCerrarSesion == null)
            btnCerrarSesion = GameObject.Find("BtnCerrar")?.GetComponent<Button>();

        if (btnEliminarCuenta == null)
            btnEliminarCuenta = GameObject.Find("BtnEliminar")?.GetComponent<Button>();

        if (sliderVolumen == null) Debug.LogError("[AjustesController] No encuentro Slider (name: Slider). Asignalo en Inspector.");
        if (btnCerrarSesion == null) Debug.LogError("[AjustesController] No encuentro BtnCerrar. Asignalo en Inspector o revisa el nombre.");
        if (btnEliminarCuenta == null) Debug.LogError("[AjustesController] No encuentro BtnEliminar. Asignalo en Inspector o revisa el nombre.");
    }

    private void HookUIEvents()
    {
        if (btnCerrarSesion != null)
        {
            btnCerrarSesion.onClick.RemoveAllListeners();
            btnCerrarSesion.onClick.AddListener(() =>
            {
                Debug.Log("[AjustesController] Click -> Cerrar sesión");
                _ = CerrarSesion();
            });
        }

        if (btnEliminarCuenta != null)
        {
            btnEliminarCuenta.onClick.RemoveAllListeners();
            btnEliminarCuenta.onClick.AddListener(() =>
            {
                Debug.Log("[AjustesController] Click -> Eliminar cuenta");
                _ = EliminarCuenta();
            });
        }

        if (sliderVolumen != null)
        {
            sliderVolumen.onValueChanged.RemoveAllListeners();
            sliderVolumen.onValueChanged.AddListener(v =>
            {
                // Aplica volumen en tiempo real
                AudioListener.volume = v;

                // Programa guardado con debounce
                _pendingVolume = v;
                _pendingSaveTime = Time.time + _saveDebounceSeconds;
            });
        }
    }

    private async Task CargarVolumenDesdeBD()
    {
        try
        {
            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[AjustesController] SupabaseClient.Instance es NULL. Asegúrate de que existe en DontDestroyOnLoad.");
                return;
            }

            // OJO: en tu proyecto estás usando PostgREST:
            // /rest/v1/jugadores?id=eq.<uuid>&select=volumen_musica
            string endpoint = $"/rest/v1/jugadores?id=eq.{Session.JugadorId}&select=volumen_musica";
            string json = await SupabaseClient.Instance.Get(endpoint);

            // json será un array, ejemplo: [{"volumen_musica":0.8}]
            float volumen = ExtraerVolumen(json);

            Debug.Log($"[AjustesController] Volumen cargado BD = {volumen}");

            if (sliderVolumen != null)
                sliderVolumen.SetValueWithoutNotify(volumen);

            AudioListener.volume = volumen;
        }
        catch (Exception e)
        {
            Debug.LogError("[AjustesController] Error al cargar volumen: " + e.Message);
        }
    }

    private float ExtraerVolumen(string json)
    {
        // Extra simple sin librerías: busca "volumen_musica":X
        // Si no está, devuelve 1
        if (string.IsNullOrEmpty(json)) return 1f;

        int idx = json.IndexOf("volumen_musica", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return 1f;

        int colon = json.IndexOf(":", idx);
        if (colon < 0) return 1f;

        int end = json.IndexOfAny(new[] { ',', '}', ']' }, colon + 1);
        if (end < 0) end = json.Length;

        string raw = json.Substring(colon + 1, end - (colon + 1)).Trim();
        raw = raw.Trim('"');

        // Asegura parse con punto decimal
        if (float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out float v))
            return Mathf.Clamp01(v);

        return 1f;
    }

    private async Task GuardarVolumenEnBD(float volumen)
    {
        try
        {
            if (!Session.IsLoggedIn) return;
            if (SupabaseClient.Instance == null) return;

            string endpoint = $"/rest/v1/jugadores?id=eq.{Session.JugadorId}";

            // JSON body para PATCH
            string body = $"{{\"volumen_musica\":{volumen.ToString(CultureInfo.InvariantCulture)}}}";

            await SupabaseClient.Instance.Patch(endpoint, body);
            Debug.Log($"[AjustesController] Volumen guardado BD = {volumen}");
        }
        catch (Exception e)
        {
            Debug.LogError("[AjustesController] Error guardando volumen: " + e.Message);
        }
    }

    private async Task CerrarSesion()
    {
        try
        {
            if (!Session.IsLoggedIn)
            {
                SceneManager.LoadScene(escenaLogin);
                return;
            }

            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[AjustesController] SupabaseClient.Instance NULL");
                return;
            }

            // activo = false
            string endpoint = $"/rest/v1/jugadores?id=eq.{Session.JugadorId}";
            string body = "{\"activo\":false}";
            await SupabaseClient.Instance.Patch(endpoint, body);

            Debug.Log("[AjustesController] Sesión cerrada (activo=false).");
        }
        catch (Exception e)
        {
            Debug.LogError("[AjustesController] Error cerrando sesión: " + e.Message);
        }
        finally
        {
            Session.Clear();
            SceneManager.LoadScene(escenaLogin);
        }
    }

    private async Task EliminarCuenta()
    {
        try
        {
            if (!Session.IsLoggedIn)
            {
                SceneManager.LoadScene(escenaLogin);
                return;
            }

            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[AjustesController] SupabaseClient.Instance NULL");
                return;
            }

            // 1) borrar dependencias primero (jugador_mejoras)
            await SupabaseClient.Instance.Delete($"/rest/v1/jugador_mejoras?jugador_id=eq.{Session.JugadorId}");

            // 2) borrar jugador
            await SupabaseClient.Instance.Delete($"/rest/v1/jugadores?id=eq.{Session.JugadorId}");

            Debug.Log("[AjustesController] Cuenta eliminada correctamente.");
        }
        catch (Exception e)
        {
            Debug.LogError("[AjustesController] Error eliminando cuenta: " + e.Message);
        }
        finally
        {
            Session.Clear();
            SceneManager.LoadScene(escenaLogin);
        }
    }
}
