using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractBase : MonoBehaviour, IInteract
{
    public string interactText = "[E]";

    private void Reset()
    {
        // Asegurar que tenga collider
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public virtual void OnInteract(PlayerStateMachine player)
    {
        Debug.Log($"{name} interactuado por {player.name}");
    }

    public string GetInteractText()
    {
        return interactText;
    }
}
