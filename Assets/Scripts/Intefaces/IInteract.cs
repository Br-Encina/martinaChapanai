using UnityEngine;

public interface IInteract
{
    void OnInteract(PlayerStateMachine player);
    string GetInteractText();
}
