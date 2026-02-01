Player Input System Setup
=========================

This project uses Unity's new Input System for gamepad detection and button reading while keeping the legacy KeyCode-based rebinding UI for keyboard fallbacks.

Recommended next steps to fully migrate to an Action-based workflow (optional):

1. Create an Input Actions asset:
   - In Unity: Right-click in Project window -> Create -> Input Actions. Name it `PlayerActions`.
   - Define an Action Map `Player` with actions: `Move` (Vector2), `Dpad` (Vector2), `Confirm` (Button), `Cancel` (Button), `SendTo` (Button), `SendFrom` (Button), `Upgrade` (Button), `Mask1`..`Mask4` (Buttons).
   - Add bindings for `<Gamepad>/leftStick`, `<Gamepad>/dpad`, and face/shoulder buttons.

2. Generate C# class from the `.inputactions` asset (there's a checkbox in the asset inspector). This gives you a `PlayerActions` C# wrapper.

3. Create a `PlayerInput` component on player GameObjects and assign the `PlayerActions` asset.
   - Set Behavior to `Send Messages` or `Invoke Unity Events` depending on preference.
   - Use `PlayerInput`'s `playerIndex` or `joinBehavior` to keep inputs distinct per player.

4. In code, consume the actions either via the generated `PlayerActions` class or by subscribing to `PlayerInput` events.

Notes about device pairing and distinct players:
- Use `PlayerInput` + `Join` behavior to assign gamepads to players automatically.
- If you prefer explicit pairing, call `playerInput.SwitchCurrentControlScheme` or use `InputUser.PerformPairingWithDevice`.

If you want, I can create the `.inputactions` asset and a `PlayerInput` prefab and wire up a `PlayerInputBridge` script to forward events to `PlayerController` methods. Reply to confirm and I'll generate the assets and bridging code.
