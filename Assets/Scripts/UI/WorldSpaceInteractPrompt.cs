using UnityEngine;
using TMPro;

// Agregá este componente a cada objeto interactuable (CartaInteract, LeverInteractable, etc.)
// Crea automáticamente un canvas flotante en world space con el texto "[E]" u otro personalizado.
// No necesitás crear ningún prefab ni canvas en la escena — se arma solo por código.

public class WorldSpaceInteractPrompt : MonoBehaviour
{
    [Header("Texto")]
    [Tooltip("Texto que aparece sobre el objeto cuando el jugador está en rango.")]
    [SerializeField] private string promptText = "[E]";

    [Header("Posición y tamaño")]
    [Tooltip("Altura del cartel relativa al pivote del objeto.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float canvasWidth = 1f;
    [SerializeField] private float canvasHeight = 0.5f;
    [SerializeField] private float fontSize = 0.25f;

    [Header("Apariencia")]
    [SerializeField] private Color textColor = Color.white;

    private Canvas canvas;
    private TextMeshProUGUI label;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        BuildUI();
        Hide(); // Empieza oculto
    }

    private void BuildUI()
    {
        // Objeto hijo que contiene el canvas
        GameObject canvasGO = new GameObject("InteractPromptCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = offset;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one;

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;

        RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(canvasWidth, canvasHeight);

        // Objeto hijo del canvas con el texto
        GameObject textGO = new GameObject("PromptText");
        textGO.transform.SetParent(canvasGO.transform, false);

        label = textGO.AddComponent<TextMeshProUGUI>();
        label.text = promptText;
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    private void LateUpdate()
    {
        // Billboard: el cartel siempre mira hacia la cámara
        if (mainCamera != null && canvas.gameObject.activeSelf)
        {
            canvas.transform.LookAt(mainCamera.transform);
            canvas.transform.Rotate(0f, 180f, 0f);
        }
    }

    public void Show()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }
}
