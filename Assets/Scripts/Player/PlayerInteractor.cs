using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    public float interactRange = 1.5f;
    public LayerMask interactLayer;

    [Header("References")]
    public InteractionUI ui; // UI global (opcional, podés dejarla null si usás solo el cartel del objeto)

    IInteract currentInteractable;
    WorldSpaceInteractPrompt currentPrompt;
    Collider[] buffer = new Collider[8];

    PlayerStateMachine playerSM;
    InputSystem_Actions action;

    private void Awake()
    {
        playerSM = GetComponent<PlayerStateMachine>();
        action = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        action.Player.Enable();
        action.Player.Interact.started += OnInteractPressed;
    }

    private void OnDisable()
    {
        action.Player.Interact.started -= OnInteractPressed;
        action.Player.Disable();
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (currentInteractable == null) return;

        Debug.Log("[Interactor] Interact pressed with " + ((MonoBehaviour)currentInteractable).gameObject.name);
        currentInteractable.OnInteract(playerSM);
    }

    private void Update()
    {
        DetectInteractable();
    }

    void DetectInteractable()
    {
        int hits = Physics.OverlapSphereNonAlloc(transform.position, interactRange, buffer, interactLayer);

        if (hits <= 0)
        {
            if (currentInteractable != null)
            {
                HideCurrentPrompt();
                currentInteractable = null;
                if (ui != null) ui.Hide();
                Debug.Log("[Interactor] No interactable in range.");
            }
            return;
        }

        // Priorizar el hit más cercano
        float minDist = float.MaxValue;
        IInteract found = null;
        for (int i = 0; i < hits; i++)
        {
            var col = buffer[i];
            var interact = col.GetComponent<IInteract>();
            if (interact == null) continue;

            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < minDist)
            {
                minDist = d;
                found = interact;
            }
        }

        if (found != null)
        {
            bool isNew = currentInteractable == null ||
                         ((MonoBehaviour)found).gameObject != ((MonoBehaviour)currentInteractable).gameObject;

            if (isNew)
            {
                HideCurrentPrompt();

                currentInteractable = found;

                currentPrompt = ((MonoBehaviour)found).GetComponent<WorldSpaceInteractPrompt>();
                if (currentPrompt != null) currentPrompt.Show();

                if (ui != null) ui.Show(found.GetInteractText());
                Debug.Log("[Interactor] Found interactable: " + ((MonoBehaviour)found).gameObject.name);
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                HideCurrentPrompt();
                currentInteractable = null;
                if (ui != null) ui.Hide();
                Debug.Log("[Interactor] Found nothing interactable in hits.");
            }
        }
    }

    void HideCurrentPrompt()
    {
        if (currentPrompt != null)
        {
            currentPrompt.Hide();
            currentPrompt = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}