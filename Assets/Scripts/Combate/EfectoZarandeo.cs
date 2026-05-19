using System.Collections;
using System;
using UnityEngine;

public class EfectoZarandeo : MonoBehaviour
{
    // Esta corrutina maneja la animación visual y sincroniza el daño
    public IEnumerator EjecutarSacudida(float intensidad, int golpes, Action accionPorGolpe, float velocidadActual = 1f)
    {
        Vector3 posOriginal = transform.localPosition;
        float tiempoPorGolpe = 0.3f / velocidadActual; // Lo que tarda en hacer un salto completo (0.15s subir, 0.15s bajar)

        for (int i = 0; i < golpes; i++)
        {
            float tiempo = 0;

            // 1. SUBIR (preparar el impacto)
            while (tiempo < tiempoPorGolpe / 2)
            {
                transform.localPosition = Vector3.Lerp(posOriginal, posOriginal + new Vector3(0, intensidad, 0), tiempo / (tiempoPorGolpe / 2));
                tiempo += Time.deltaTime;
                yield return null;
            }

            // 2. GOLPE Momento exacto del impacto (aquí se restará la vida)
            accionPorGolpe?.Invoke();

            // 3. BAJAR (volver a la posición original)
            tiempo = 0;
            while (tiempo < tiempoPorGolpe / 2)
            {
                transform.localPosition = Vector3.Lerp(posOriginal + new Vector3(0, intensidad, 0), posOriginal, tiempo / (tiempoPorGolpe / 2));
                tiempo += Time.deltaTime;
                yield return null;
            }

            // Asegurar que queda perfectamente centrado
            transform.localPosition = posOriginal;

            // Pequeña pausa si da varios golpes seguidos
            if (golpes > 1) yield return new WaitForSeconds(0.1f);
        }
    }
}