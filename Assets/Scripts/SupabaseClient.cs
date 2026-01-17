using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class SupabaseClient : MonoBehaviour
{
    [Header("Supabase")]
    [SerializeField] private string supabaseUrl;
    [SerializeField] private string anonKey;

    public static SupabaseClient Instance { get; private set; }

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

    // =========================
    // GET
    // =========================
    public async Task<string> Get(string endpoint)
    {
        string fullUrl = supabaseUrl + endpoint;
        Debug.Log("GET -> " + fullUrl);

        var request = UnityWebRequest.Get(fullUrl);
        request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", "Bearer " + anonKey);
        request.SetRequestHeader("Accept", "application/json");

        request.certificateHandler = new BypassCertificate();

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("UnityWebRequest result: " + request.result);
            Debug.LogError("responseCode: " + request.responseCode);
            Debug.LogError("error: " + request.error);
            Debug.LogError("download text: " + request.downloadHandler.text);

            throw new System.Exception($"GET ERROR {request.responseCode}: {request.downloadHandler.text}");
        }

        return request.downloadHandler.text;
    }

    // =========================
    // POST
    // =========================
    public async Task<string> Post(string endpoint, string jsonBody)
    {
        string fullUrl = supabaseUrl + endpoint;
        Debug.Log("POST -> " + fullUrl);

        var request = new UnityWebRequest(fullUrl, "POST");
        byte[] body = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", "Bearer " + anonKey);
        request.SetRequestHeader("Accept", "application/json");

        request.certificateHandler = new BypassCertificate();

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("UnityWebRequest result: " + request.result);
            Debug.LogError("responseCode: " + request.responseCode);
            Debug.LogError("error: " + request.error);
            Debug.LogError("download text: " + request.downloadHandler.text);

            throw new System.Exception($"POST ERROR {request.responseCode}: {request.downloadHandler.text}");
        }

        return request.downloadHandler.text;
    }

    // =========================
    // PATCH
    // =========================
    public async Task<string> Patch(string endpoint, string jsonBody)
    {
        string fullUrl = supabaseUrl + endpoint;
        Debug.Log("PATCH -> " + fullUrl);

        var request = new UnityWebRequest(fullUrl, "PATCH");
        byte[] body = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", "Bearer " + anonKey);
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Prefer", "return=representation");

        request.certificateHandler = new BypassCertificate();

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("UnityWebRequest result: " + request.result);
            Debug.LogError("responseCode: " + request.responseCode);
            Debug.LogError("error: " + request.error);
            Debug.LogError("download text: " + request.downloadHandler.text);

            throw new System.Exception($"PATCH ERROR {request.responseCode}: {request.downloadHandler.text}");
        }

        return request.downloadHandler.text;
    }

    // =========================
    // DELETE
    // =========================
    public async Task Delete(string endpoint)
    {
        string fullUrl = supabaseUrl + endpoint;
        Debug.Log("DELETE -> " + fullUrl);

        var request = new UnityWebRequest(fullUrl, "DELETE");
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", "Bearer " + anonKey);
        request.SetRequestHeader("Accept", "application/json");

        request.certificateHandler = new BypassCertificate();

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("UnityWebRequest result: " + request.result);
            Debug.LogError("responseCode: " + request.responseCode);
            Debug.LogError("error: " + request.error);
            Debug.LogError("download text: " + request.downloadHandler.text);

            throw new System.Exception($"DELETE ERROR {request.responseCode}: {request.downloadHandler.text}");
        }
    }
}

//  Clase para saltarse SSL (SOLO DESARROLLO)
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
