using UnityEngine;
using Fusion;

public class PlayerCubeMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    public override void FixedUpdateNetwork()
    {
        // GetInput retrieves the data packed in GlobalManager.OnInput
        // It references the single definition now located in GlobalManager
        if (GetInput<NetworkInputData>(out var input))
        {
            // Convert Vector2 input from the struct into a 3D movement vector
            Vector3 moveDirection = new Vector3(input.direction.x, 0, input.direction.y);
            
            // Normalize the direction to prevent diagonal speed boost
            moveDirection = moveDirection.normalized;

            // Apply movement using the Runner's DeltaTime for network-sync consistency
            transform.Translate(moveDirection * speed * Runner.DeltaTime);
        }
    }
}