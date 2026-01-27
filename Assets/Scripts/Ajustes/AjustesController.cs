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

    // --- EVENTOS DE CARGA (NUEVO) ---

    private void OnEnable()
    {
        // Se suscribe para escuchar cuando Unity cambia de escena
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        // Se desuscribe al destruir el objeto
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Si la escena que se acaba de cargar es "ajustes", buscamos los botones
        if (escena.name == "ajustes")
        {
            Debug.Log("[AjustesController] Escena de Ajustes detectada. Vinculando...");
            BuscarBotonesYConectar();
        }
    }

    // --- LÓGICA DE BÚSQUEDA ---

    private async void BuscarBotonesYConectar()
    {
        // Buscamos los objetos por su nombre exacto en la Hierarchy
        btnCerrarSesion = GameObject.Find("BtnCerrar")?.GetComponent<Button>();
        btnEliminarCuenta = GameObject.Find("BtnEliminar")?.GetComponent<Button>();
        sliderVolumen = GameObject.Find("Slider")?.GetComponent<Slider>();

        // Si los encontró, les asigna las funciones de clic
        HookUIEvents();

        if (Session.IsLoggedIn)
        {
            await CargarVolumenDesdeBD();
        }
    }

    private void Start()
    {
        // La primera vez que el GameManager aparece (en el Login), 
        // intentamos buscar si ya estamos en ajustes por si acaso.
        BuscarBotonesYConectar();
    }

    private void Update()
    {
        if (_pendingSaveTime > 0 && Time.time >= _pendingSaveTime)
        {
            _pendingSaveTime = -1f;
            _ = GuardarVolumenEnBD(_pendingVolume);
        }
    }

    private void HookUIEvents()
    {
        if (btnCerrarSesion != null)
        {
            btnCerrarSesion.onClick.RemoveAllListeners();
            btnCerrarSesion.onClick.AddListener(OnClickCerrarSesion);
            Debug.Log("[AjustesController] Botón Cerrar Vinculado.");
        }

        if (btnEliminarCuenta != null)
        {
            btnEliminarCuenta.onClick.RemoveAllListeners();
            btnEliminarCuenta.onClick.AddListener(OnClickEliminarCuenta);
            Debug.Log("[AjustesController] Botón Eliminar Vinculado.");
        }

        if (sliderVolumen != null)
        {
            sliderVolumen.onValueChanged.RemoveAllListeners();
            sliderVolumen.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = v;
                _pendingVolume = v;
                _pendingSaveTime = Time.time + _saveDebounceSeconds;
            });
        }
    }

    // --- FUNCIONES DE CLIC ---

    public void OnClickCerrarSesion()
    {
        Debug.Log(">>> CLIC: Cerrar Sesión <<<");
        _ = EjecutarCerrarSesion();
    }

    public void OnClickEliminarCuenta()
    {
        Debug.Log(">>> CLIC: Eliminar Cuenta <<<");
        _ = EjecutarEliminarCuenta();
    }

    // (El resto de tus funciones asíncronas de Supabase se mantienen igual debajo...)
    private async Task EjecutarCerrarSesion() { try { await CerrarSesion(); } catch (Exception e) { Debug.LogError(e.Message); } }
    private async Task EjecutarEliminarCuenta() { try { await EliminarCuenta(); } catch (Exception e) { Debug.LogError(e.Message); } }
    private async Task CerrarSesion() { /* Tu lógica */ }
    private async Task EliminarCuenta() { /* Tu lógica */ }
    private async Task CargarVolumenDesdeBD() { /* Tu lógica */ }
    private float ExtraerVolumen(string json) { return 1f; }
    private async Task GuardarVolumenEnBD(float v) { /* Tu lógica */ }
}