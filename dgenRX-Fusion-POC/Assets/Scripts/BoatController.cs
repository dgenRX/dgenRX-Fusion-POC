using Fusion;
using UnityEngine;

public class BoatController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    public override void FixedUpdateNetwork()
    {
        // Only the client that has authority over this object should process input
        if (!HasStateAuthority) return;

        float move = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");

        transform.Translate(Vector3.forward * move * moveSpeed * Runner.DeltaTime);
        transform.Rotate(Vector3.up * rotate * rotationSpeed * Runner.DeltaTime);
    }
}