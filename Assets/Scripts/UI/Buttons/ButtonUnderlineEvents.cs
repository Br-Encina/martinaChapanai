using UnityEngine;
using TMPro;

// Poné este script en el botón.
// Después agregá un componente "Event Trigger" al mismo GameObject y conectá:
//   - Evento "Pointer Enter" → este GameObject → ButtonUnderlineEvents.OnHoverEnter()
//   - Evento "Pointer Exit"  → este GameObject → ButtonUnderlineEvents.OnHoverExit()

public class ButtonUnderlineEvents : MonoBehaviour
{
    TextMeshProUGUI label;

    private void Awake()
    {
        label = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnHoverEnter()
    {
        if (label != null)
            label.fontStyle |= FontStyles.Underline;
    }

    public void OnHoverExit()
    {
        if (label != null)
            label.fontStyle &= ~FontStyles.Underline;
    }
}
