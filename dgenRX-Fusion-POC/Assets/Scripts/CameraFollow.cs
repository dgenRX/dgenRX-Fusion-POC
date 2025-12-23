using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset = new Vector3(0, 5, -10);
    public float SmoothSpeed = 0.125f;

    void LateUpdate()
    {
        // We only move if a target has been assigned
        if (Target != null)
        {
            Vector3 desiredPosition = Target.position + Offset;
            // Smoothly interpolate between current position and desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
            transform.position = smoothedPosition;

            // Keep the camera looking at the player
            transform.LookAt(Target);
        }
    }
}