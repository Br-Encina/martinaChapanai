using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class CartaInteract : InteractBase
{
    //AudioSource audioSource;

    [SerializeField] float rotacionVelocity = 2f;

    [SerializeField] Vector3 rotacionEje = Vector3.up;

    private void Start()
    {
        //audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
   
        transform.Rotate(rotacionEje * rotacionVelocity * Time.deltaTime);
    }

    public override void OnInteract(PlayerStateMachine player)
    {
        //audioSource.Play();
        Debug.Log("tomaste la carta!");
        base.OnInteract(player);

        Destroy(gameObject, 0.1f);
    }
}

