using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bridge : MonoBehaviour
{
    public static Bridge Instance;

    // Clave = Nombre exacto del hechizo, Valor = Usos restantes
    public Dictionary<string, int> inventarioUsos = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // Evita duplicados si vuelve a pasar por aqu�
        }
    }

    // ==========================================
    // SE LLAMA ANTES DE SALIR DEL COMBATE
    // ==========================================
    public void GuardarUsos()
    {
        inventarioUsos.Clear();

        // Busca todos los hechizos de la escena
        Atributos[] todosLosHechizos = FindObjectsOfType<Atributos>();

        foreach (Atributos hechizo in todosLosHechizos)
        {
            string nombreLimpio = hechizo.gameObject.name.Replace("(Clone)", "").Trim();

            if (!inventarioUsos.ContainsKey(nombreLimpio))
            {
                inventarioUsos.Add(nombreLimpio, hechizo.usosActuales);
            }
        }

        Debug.Log($"Bridge: Se han guardado los usos de {inventarioUsos.Count} hechizos.");
    }

    // ==========================================
    // SE LLAMA AL ENTRAR AL COMBATE
    // ==========================================
    public void CargarUsos()
    {
        Atributos[] todosLosHechizos = FindObjectsOfType<Atributos>();

        foreach (Atributos hechizo in todosLosHechizos)
        {
            string nombreLimpio = hechizo.gameObject.name.Replace("(Clone)", "").Trim();

            if (inventarioUsos.ContainsKey(nombreLimpio))
            {
                // Le pone los usos que tenga guardados el diccionario
                hechizo.ConfigurarUsos(inventarioUsos[nombreLimpio]);
            }
            else
            {
                // Es un hechizo que no estaba registrado
                hechizo.ConfigurarUsos(-1);
            }
        }
    }

    // Detecta cu�ndo se ha cargado una nueva escena y aplica los usos
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CargarUsos();
    }
}