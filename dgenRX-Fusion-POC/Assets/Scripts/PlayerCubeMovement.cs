using UnityEngine;
using Fusion;

public class PlayerCubeMovement : NetworkBehaviour
{
    private Vector3 _velocity;
    private bool _isGrounded;

    // FixedUpdateNetwork is the Fusion version of FixedUpdate
    public override void FixedUpdateNetwork()
    {
        // ONLY move if this window has "Input Authority" (is the local player)
        if (HasInputAuthority)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = new Vector3(x, 0, z) * Runner.DeltaTime * 5f;
            
            // Standard and most reliable way for NetworkTransform to sync in Fusion 2
            transform.position += move;
        }
    }
}