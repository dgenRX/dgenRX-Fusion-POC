using UnityEngine;
using Fusion;

public class PlayerCubeMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    public override void FixedUpdateNetwork()
    {
        // GetInput retrieves the data we packed in GlobalManager.OnInput
        if (GetInput<NetworkInputData>(out var input))
        {
            // Normalize the direction to prevent diagonal speed boost
            Vector3 moveDirection = input.direction.normalized;

            // Apply movement using the Runner's DeltaTime for network-sync consistency
            transform.Translate(moveDirection * speed * Runner.DeltaTime);
        }
    }
}