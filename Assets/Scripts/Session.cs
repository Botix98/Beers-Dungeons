using UnityEngine;

public static class Session
{
    private const string KEY_ID = "SESSION_JUGADOR_ID";
    private const string KEY_NOMBRE = "SESSION_JUGADOR_NOMBRE";

    public static bool IsLoggedIn => !string.IsNullOrEmpty(JugadorId);

    public static string JugadorId
    {
        get => PlayerPrefs.GetString(KEY_ID, "");
        set { PlayerPrefs.SetString(KEY_ID, value ?? ""); PlayerPrefs.Save(); }
    }

    public static string Nombre
    {
        get => PlayerPrefs.GetString(KEY_NOMBRE, "");
        set { PlayerPrefs.SetString(KEY_NOMBRE, value ?? ""); PlayerPrefs.Save(); }
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY_ID);
        PlayerPrefs.DeleteKey(KEY_NOMBRE);
        PlayerPrefs.Save();
    }
}
