using UnityEngine;
using System.Collections;

public class AparicionDePista : MonoBehaviour
{
    [Header("Posiciones")]
    public Vector2 posicionInicial;
    [Tooltip("Cuántos píxeles baja desde la posición inicial.")]
    [SerializeField] float distanciaBajada = 400f;

    [Header("Tiempos")]
    [SerializeField] float tiempoDeEntrada = 0.5f; // cuánto tarda en bajar
    [SerializeField] float tiempoDeEspera = 2f;   // cuánto tiempo se queda visible
    [SerializeField] float tiempoDeSalida = 0.5f; // cuánto tarda en volver

    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Si no se asignó posición inicial en el Inspector, usamos la posición actual
        if (posicionInicial == Vector2.zero)
            posicionInicial = rectTransform.anchoredPosition;

        MostrarPista();
    }

    // Llamá este método desde un botón, un evento o desde otro script
    public void MostrarPista()
    {
        
        StartCoroutine(AnimarPista());
    }

    IEnumerator AnimarPista()
    {
        Vector2 posicionVisible = posicionInicial + Vector2.down * distanciaBajada;

        // ── Bajar ─────────────────────────────────────────────
        yield return Mover(posicionInicial, posicionVisible, tiempoDeEntrada);

        // ── Esperar visible ───────────────────────────────────
        yield return new WaitForSeconds(tiempoDeEspera);

        // ── Volver arriba ─────────────────────────────────────
        yield return Mover(posicionVisible, posicionInicial, tiempoDeSalida);
    }

    IEnumerator Mover(Vector2 desde, Vector2 hasta, float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            // EaseInOut: el movimiento arranca suave, acelera y frena suave
            t = t * t * (3f - 2f * t);

            rectTransform.anchoredPosition = Vector2.Lerp(desde, hasta, t);
            yield return null;
        }

        rectTransform.anchoredPosition = hasta; // asegurar posición exacta al final
    }
}