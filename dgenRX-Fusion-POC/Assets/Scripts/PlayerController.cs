using Fusion;
using TMPro;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private TextMeshPro _label;
    [SerializeField] private float _moveSpeed = 5f;

    [Networked] private Vector3 _networkedPosition { get; set; }
    
    [Networked] public int Card1Index { get; set; } = -1;
    [Networked] public int Card2Index { get; set; } = -1;

    public override void Spawned()
    {
        // Log to console so we can see exactly which player spawned this cube
        Debug.Log($"[PlayerController] Spawned for Player: {Object.InputAuthority} - IsLocal: {Object.HasInputAuthority}");

        if (Object.HasInputAuthority)
        {
            _networkedPosition = transform.position;
        }

        if (_label != null)
        {
            _label.text = "";
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _networkedPosition += data.direction * _moveSpeed * Runner.DeltaTime;
            transform.position = _networkedPosition;
        }
    }

    public override void Render()
    {
        if (_label == null) return;

        if (Card1Index >= 0 && Card2Index >= 0)
        {
            _label.text = $"Cards: {Card1Index} & {Card2Index}";
        }
        else
        {
            _label.text = "";
        }
    }
}