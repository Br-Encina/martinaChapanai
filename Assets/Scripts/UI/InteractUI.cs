using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI text;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show(string message)
    {
        if (panel == null) return;
        panel.SetActive(true);
        if (text != null) text.text = message;
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }
}
