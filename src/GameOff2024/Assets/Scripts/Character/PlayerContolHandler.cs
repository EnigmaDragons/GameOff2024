using NeoCC;
using NeoFPS;
using UnityEngine;

public class PlayerControlHandler : OnMessage<EnablePlayerControls, DisablePlayerControls, EnablePlayerLookControls>
{
    [SerializeField] private MouseAndGamepadAimController aimController;
    [SerializeField] private NeoCharacterController movementController;
    
    protected override void Execute(EnablePlayerControls msg)
    {
        Log.Info("Enable Player Controls");
        aimController.enabled = true;
        movementController.enabled = true;

    }

    protected override void Execute(DisablePlayerControls msg)
    {
        Log.Info("Disable Player Controls");
        aimController.enabled = false;
        movementController.enabled = false;
    }

    protected override void Execute(EnablePlayerLookControls msg)
    {
        Log.Info("Enable Player Look Controls");
        aimController.enabled = true;
    }
}
