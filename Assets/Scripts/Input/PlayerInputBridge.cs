using UnityEngine;
using UnityEngine.InputSystem;

// Bridge that forwards PlayerInput (Send Messages) callbacks to a PlayerController.
// Add this component to the same GameObject as your PlayerController (or assign the controller reference).
// Configure your PlayerInput component to use the generated InputActions asset and set "Behavior" to "Send Messages".
public class PlayerInputBridge : MonoBehaviour
{
    [SerializeField] private PlayerController controller;

    public void SetController(PlayerController ctrl)
    {
        controller = ctrl;
    }

    // Message callbacks from PlayerInput (names must match the actions' names)
    public void OnMove(InputValue value)
    {
        if (controller == null) return;
        Vector2 v = value.Get<Vector2>();
        controller.ExternalMove(v);
    }

    public void OnDpad(InputValue value)
    {
        if (controller == null) return;
        Vector2 v = value.Get<Vector2>();
        controller.ExternalDpad(v);
    }

    public void OnConfirm()
    {
        if (controller == null) return;
        controller.ExternalConfirm();
    }

    public void OnCancel()
    {
        if (controller == null) return;
        controller.ExternalCancel();
    }

    public void OnSendTo()
    {
        if (controller == null) return;
        controller.ExternalSendTo();
    }

    public void OnSendFrom()
    {
        if (controller == null) return;
        controller.ExternalSendFrom();
    }

    public void OnUpgrade()
    {
        if (controller == null) return;
        controller.ExternalUpgrade();
    }

    public void OnMask1()
    {
        if (controller == null) return;
        controller.ExternalUseMask(0);
    }

    public void OnMask2()
    {
        if (controller == null) return;
        controller.ExternalUseMask(1);
    }

    public void OnMask3()
    {
        if (controller == null) return;
        controller.ExternalUseMask(2);
    }

    public void OnMask4()
    {
        if (controller == null) return;
        controller.ExternalUseMask(3);
    }
}
