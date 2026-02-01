using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Targeting mode for masks.
/// </summary>
public enum TargetingMode
{
    Room,   // Target a single room
    Column  // Target an entire column (left-right only)
}

/// <summary>
/// Defines all player actions and their default key bindings.
/// Supports both keyboard (KeyCode) and gamepad (button indices) input.
/// </summary>
[Serializable]
public class PlayerInputBindings
{
    [Header("Input Mode")]
    public bool useGamepad = false;
    
    [Header("Cursor Movement")]
    public KeyCode cursorUp = KeyCode.W;
    public KeyCode cursorDown = KeyCode.S;
    public KeyCode cursorLeft = KeyCode.A;
    public KeyCode cursorRight = KeyCode.D;
    
    [Header("Gamepad Cursor Movement")]
    // Use D-pad (0=any dpad) or left stick (1) - we'll default to left stick for cursor
    public int gamepadCursorUp = -1;
    public int gamepadCursorDown = -1;
    public int gamepadCursorLeft = -1;
    public int gamepadCursorRight = -1;
    
    [Header("Follower Commands")]
    public KeyCode sendToSanctuary = KeyCode.Z;
    public KeyCode sendFromSanctuary = KeyCode.X;
    public int gamepadSendToSanctuary = 4; // LB
    public int gamepadSendFromSanctuary = 5; // RB
    
    [Header("Room Commands")]
    public KeyCode upgradeRoom = KeyCode.Q;
    public int gamepadUpgradeRoom = 3; // Y button
    
    [Header("Mask Commands")]
    public KeyCode useMask1 = KeyCode.Alpha1;
    public KeyCode useMask2 = KeyCode.Alpha2;
    public KeyCode useMask3 = KeyCode.Alpha3;
    public KeyCode useMask4 = KeyCode.Alpha4;
    public int gamepadUseMask1 = 0; // A button
    public int gamepadUseMask2 = 1; // B button
    public int gamepadUseMask3 = 2; // X button
    public int gamepadUseMask4 = 3; // Y button
    
    [Header("Targeting")]
    public KeyCode confirmTarget = KeyCode.BackQuote;
    public KeyCode cancelTarget = KeyCode.LeftControl;
    public int gamepadConfirmTarget = 0; // A button
    public int gamepadCancelTarget = 1; // B button
    
    /// <summary>
    /// Create default bindings for Player 1 (WASD layout).
    /// </summary>
    public static PlayerInputBindings CreatePlayer1Defaults()
    {
        return new PlayerInputBindings
        {
            useGamepad = false,
            cursorUp = KeyCode.W,
            cursorDown = KeyCode.S,
            cursorLeft = KeyCode.A,
            cursorRight = KeyCode.D,
            sendToSanctuary = KeyCode.Z,
            sendFromSanctuary = KeyCode.X,
            upgradeRoom = KeyCode.Q,
            useMask1 = KeyCode.Alpha1,
            useMask2 = KeyCode.Alpha2,
            useMask3 = KeyCode.Alpha3,
            useMask4 = KeyCode.Alpha4,
            confirmTarget = KeyCode.BackQuote,
            cancelTarget = KeyCode.LeftControl
        };
    }
    
    /// <summary>
    /// Create default bindings for Player 2 (Arrow keys layout).
    /// </summary>
    public static PlayerInputBindings CreatePlayer2Defaults()
    {
        return new PlayerInputBindings
        {
            useGamepad = false,
            cursorUp = KeyCode.UpArrow,
            cursorDown = KeyCode.DownArrow,
            cursorLeft = KeyCode.LeftArrow,
            cursorRight = KeyCode.RightArrow,
            sendToSanctuary = KeyCode.Comma,
            sendFromSanctuary = KeyCode.Period,
            upgradeRoom = KeyCode.Slash,
            useMask1 = KeyCode.K,
            useMask2 = KeyCode.L,
            useMask3 = KeyCode.Semicolon,
            useMask4 = KeyCode.Quote,
            confirmTarget = KeyCode.Return,
            cancelTarget = KeyCode.RightControl
        };
    }
    
    /// <summary>
    /// Create gamepad default bindings (works for both players, uses the new Input System).
    /// playerIndex: 0 for Player 1, 1 for Player 2 (for keyboard fallbacks).
    /// </summary>
    public static PlayerInputBindings CreateGamepadDefaults(int playerIndex = 0)
    {
        // Use appropriate keyboard defaults as fallback based on player
        var keyboardDefaults = playerIndex == 0 
            ? CreatePlayer1Defaults() 
            : CreatePlayer2Defaults();
        
        return new PlayerInputBindings
        {
            useGamepad = true,
            // Keyboard fallbacks (won't be used if gamepad connected, but keep player-specific mappings)
            cursorUp = keyboardDefaults.cursorUp,
            cursorDown = keyboardDefaults.cursorDown,
            cursorLeft = keyboardDefaults.cursorLeft,
            cursorRight = keyboardDefaults.cursorRight,
            sendToSanctuary = keyboardDefaults.sendToSanctuary,
            sendFromSanctuary = keyboardDefaults.sendFromSanctuary,
            upgradeRoom = keyboardDefaults.upgradeRoom,
            useMask1 = keyboardDefaults.useMask1,
            useMask2 = keyboardDefaults.useMask2,
            useMask3 = keyboardDefaults.useMask3,
            useMask4 = keyboardDefaults.useMask4,
            confirmTarget = keyboardDefaults.confirmTarget,
            cancelTarget = keyboardDefaults.cancelTarget,
            // Gamepad defaults (buttons for movement, actions, masks from D-pad)
            gamepadCursorUp = -1,    // D-pad handled by analog stick
            gamepadCursorDown = -1,
            gamepadCursorLeft = -1,
            gamepadCursorRight = -1,
            gamepadSendToSanctuary = 1,    // B button (was flipped, now corrected)
            gamepadSendFromSanctuary = 0,  // A button (was flipped, now corrected)
            gamepadUpgradeRoom = 3,        // Y button (was flipped, now corrected)
            gamepadUseMask1 = -1,  // D-pad up (handled separately)
            gamepadUseMask2 = -1,  // D-pad down
            gamepadUseMask3 = -1,  // D-pad left
            gamepadUseMask4 = -1,  // D-pad right
            gamepadConfirmTarget = 1,  // B button (corrected)
            gamepadCancelTarget = 0    // A button (corrected)
        };
    }
    
    /// <summary>
    /// Convert gamepad button index to its corresponding Joystick KeyCode.
    /// </summary>
    public static KeyCode GamepadButtonToKeyCode(int buttonIndex, int playerIndex = 0)
    {
        int baseButton = (int)KeyCode.Joystick1Button0 + (playerIndex * 20);
        return (KeyCode)(baseButton + buttonIndex);
    }
    
    /// <summary>
    /// Apply gamepad button mappings to KeyCode fields.
    /// This converts gamepad button indices (0-9) to actual KeyCode values.
    /// </summary>
    public void ApplyGamepadKeyMappings(int playerIndex = 0)
    {
        // Map gamepad buttons to their KeyCode equivalents
        if (gamepadCursorUp >= 0) cursorUp = GamepadButtonToKeyCode(gamepadCursorUp, playerIndex);
        if (gamepadCursorDown >= 0) cursorDown = GamepadButtonToKeyCode(gamepadCursorDown, playerIndex);
        if (gamepadCursorLeft >= 0) cursorLeft = GamepadButtonToKeyCode(gamepadCursorLeft, playerIndex);
        if (gamepadCursorRight >= 0) cursorRight = GamepadButtonToKeyCode(gamepadCursorRight, playerIndex);
        
        if (gamepadSendToSanctuary >= 0) sendToSanctuary = GamepadButtonToKeyCode(gamepadSendToSanctuary, playerIndex);
        if (gamepadSendFromSanctuary >= 0) sendFromSanctuary = GamepadButtonToKeyCode(gamepadSendFromSanctuary, playerIndex);
        
        if (gamepadUpgradeRoom >= 0) upgradeRoom = GamepadButtonToKeyCode(gamepadUpgradeRoom, playerIndex);
        
        if (gamepadUseMask1 >= 0) useMask1 = GamepadButtonToKeyCode(gamepadUseMask1, playerIndex);
        if (gamepadUseMask2 >= 0) useMask2 = GamepadButtonToKeyCode(gamepadUseMask2, playerIndex);
        if (gamepadUseMask3 >= 0) useMask3 = GamepadButtonToKeyCode(gamepadUseMask3, playerIndex);
        if (gamepadUseMask4 >= 0) useMask4 = GamepadButtonToKeyCode(gamepadUseMask4, playerIndex);
        
        if (gamepadConfirmTarget >= 0) confirmTarget = GamepadButtonToKeyCode(gamepadConfirmTarget, playerIndex);
        if (gamepadCancelTarget >= 0) cancelTarget = GamepadButtonToKeyCode(gamepadCancelTarget, playerIndex);
    }
    
    /// <summary>
    /// Clone these bindings.
    /// </summary>
    public PlayerInputBindings Clone()
    {
        return new PlayerInputBindings
        {
            useGamepad = useGamepad,
            cursorUp = cursorUp,
            cursorDown = cursorDown,
            cursorLeft = cursorLeft,
            cursorRight = cursorRight,
            gamepadCursorUp = gamepadCursorUp,
            gamepadCursorDown = gamepadCursorDown,
            gamepadCursorLeft = gamepadCursorLeft,
            gamepadCursorRight = gamepadCursorRight,
            sendToSanctuary = sendToSanctuary,
            sendFromSanctuary = sendFromSanctuary,
            gamepadSendToSanctuary = gamepadSendToSanctuary,
            gamepadSendFromSanctuary = gamepadSendFromSanctuary,
            upgradeRoom = upgradeRoom,
            gamepadUpgradeRoom = gamepadUpgradeRoom,
            useMask1 = useMask1,
            useMask2 = useMask2,
            useMask3 = useMask3,
            useMask4 = useMask4,
            gamepadUseMask1 = gamepadUseMask1,
            gamepadUseMask2 = gamepadUseMask2,
            gamepadUseMask3 = gamepadUseMask3,
            gamepadUseMask4 = gamepadUseMask4,
            confirmTarget = confirmTarget,
            cancelTarget = cancelTarget,
            gamepadConfirmTarget = gamepadConfirmTarget,
            gamepadCancelTarget = gamepadCancelTarget
        };
    }
}

/// <summary>
/// Manages input for a single player, tracking cursor position and processing commands.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int playerIndex = 0; // 0 = Player 1, 1 = Player 2
    [SerializeField] private Cult cult;
    
    [Header("Bindings")]
    [SerializeField] private PlayerInputBindings bindings;
    
    [Header("State")]
    [SerializeField] private Vector2Int cursorPosition = Vector2Int.zero;
    [SerializeField] private bool isTargeting = false;
    [SerializeField] private int activeMaskSlot = -1; // -1 = no mask active
    [SerializeField] private Vector2Int targetPosition = Vector2Int.zero;
    
    // Joystick debounce - prevents rapid repeated movement from stick drift/holding
    private Vector2 prevJoystickInput = Vector2.zero;
    private float joystickDeadzone = 0.5f;

    // Helper: cache previous trigger/button states if needed in future
    // (kept simple for now - we primarily map standard face buttons and shoulders)

    private Gamepad GetAssignedGamepad()
    {
        if (bindings == null || !bindings.useGamepad) return null;
        var list = Gamepad.all;
        if (playerIndex >= 0 && playerIndex < list.Count) return list[playerIndex];
        return null;
    }

    private bool GamepadButtonWasPressed(Gamepad gp, int buttonIndex)
    {
        if (gp == null) return false;
        switch (buttonIndex)
        {
            case 0: return gp.buttonSouth.wasPressedThisFrame; // A
            case 1: return gp.buttonEast.wasPressedThisFrame;  // B
            case 2: return gp.buttonWest.wasPressedThisFrame;  // X
            case 3: return gp.buttonNorth.wasPressedThisFrame; // Y
            case 4: return gp.leftShoulder.wasPressedThisFrame; // LB
            case 5: return gp.rightShoulder.wasPressedThisFrame; // RB
            case 8: return gp.startButton != null && gp.startButton.wasPressedThisFrame;
            case 9: return gp.selectButton != null && gp.selectButton.wasPressedThisFrame;
            default: return false;
        }
    }
    
    // Events
    public event Action<Vector2Int> OnCursorMoved;
    public event Action<Room> OnRoomSelected;
    public event Action<int> OnMaskActivated;
    public event Action<Room> OnTargetConfirmed;
    public event Action OnTargetCancelled;
    
    // Properties
    public int PlayerIndex => playerIndex;
    public Cult Cult => cult;
    public Vector2Int CursorPosition => cursorPosition;
    public Vector2Int TargetPosition => targetPosition;
    public bool IsTargeting => isTargeting;
    public int ActiveMaskSlot => activeMaskSlot;
    public PlayerInputBindings Bindings => bindings;
    
    private void Awake()
    {
        // Initialize with default bindings based on player index
        if (bindings == null)
        {
            bindings = playerIndex == 0 
                ? PlayerInputBindings.CreatePlayer1Defaults() 
                : PlayerInputBindings.CreatePlayer2Defaults();
        }
    }
    
    public void Initialize(int index, Cult cult)
    {
        this.playerIndex = index;
        this.cult = cult;
        this.bindings = index == 0 
            ? PlayerInputBindings.CreatePlayer1Defaults() 
            : PlayerInputBindings.CreatePlayer2Defaults();
        
        // Start cursor at center of grid
        if (cult?.church != null)
        {
            cursorPosition = new Vector2Int(cult.church.GridWidth / 2, cult.church.GridHeight / 2);
        }
    }
    
    /// <summary>
    /// Switch input mode between keyboard and gamepad.
    /// </summary>
    public void SetUseGamepad(bool useGamepad)
    {
        if (bindings != null)
        {
            bindings.useGamepad = useGamepad;
            if (useGamepad)
            {
                Debug.Log($"Player {playerIndex + 1} switched to gamepad input");
            }
            else
            {
                Debug.Log($"Player {playerIndex + 1} switched to keyboard input");
            }
        }
    }
    
    /// <summary>
    /// Apply default gamepad bindings for this player.
    /// </summary>
    public void ApplyGamepadDefaults()
    {
        bindings = PlayerInputBindings.CreateGamepadDefaults(playerIndex);
        bindings.ApplyGamepadKeyMappings(playerIndex);
        Debug.Log($"Player {playerIndex + 1} applied gamepad defaults");
    }
    
    private void Update()
    {
        // DEBUG: Comprehensive gamepad input logging (Input System)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"=== GAMEPAD DEBUG (Player {playerIndex + 1}) ===");
            Debug.Log($"Gamepad Connected (InputSystem): {GetAssignedGamepad() != null}");
            Debug.Log($"All Joysticks: {string.Join(", ", Input.GetJoystickNames())}");

            // Raw axis values (legacy)
            Debug.Log($"Raw Horizontal: {Input.GetAxis("Horizontal")}");
            Debug.Log($"Raw Vertical: {Input.GetAxis("Vertical")}");

            // Try all possible axes to find buttons
            Debug.Log("=== CHECKING ALL AXES ===");
            for (int i = 0; i < 28; i++)
            {
                try
                {
                    string axisName = $"Joy1Axis{i}";
                    float val = Input.GetAxisRaw(axisName);
                    if (val != 0)
                    {
                        Debug.Log($"Joy1Axis{i}: {val}");
                    }
                }
                catch { }
            }
        }
        
        // DEBUG: Continuous axis logging (so we can see if anything changes)
        if (playerIndex == 0) // Only log once per frame to avoid spam
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (h != 0 || v != 0)
            {
                Debug.Log($"Axes - H: {h:F2}, V: {v:F2}");
            }
        }
        
        if (cult == null || cult.church == null) return;
        
        ProcessCursorInput();
        ProcessFollowerCommands();
        ProcessRoomCommands();
        ProcessMaskCommands();
        ProcessTargetingInput();
    }
    
    private void ProcessCursorInput()
    {
        Vector2Int movement = Vector2Int.zero;

        // Keyboard handling (legacy KeyCode) remains for rebinding/fallback
        if (!bindings.useGamepad)
        {
            if (Input.GetKeyDown(bindings.cursorUp)) movement.y = 1;
            if (Input.GetKeyDown(bindings.cursorDown)) movement.y = -1;
            if (Input.GetKeyDown(bindings.cursorLeft)) movement.x = -1;
            if (Input.GetKeyDown(bindings.cursorRight)) movement.x = 1;
        }
        else
        {
            var gp = GetAssignedGamepad();
            if (gp != null)
            {
                // Read left stick and debounce
                Vector2 stick = gp.leftStick.ReadValue();
                float h = stick.x;
                float v = stick.y;

                if (Mathf.Abs(h) > joystickDeadzone && Mathf.Abs(prevJoystickInput.x) <= joystickDeadzone)
                {
                    movement.x = h > 0 ? 1 : -1;
                }
                if (Mathf.Abs(v) > joystickDeadzone && Mathf.Abs(prevJoystickInput.y) <= joystickDeadzone)
                {
                    movement.y = v > 0 ? 1 : -1;
                }

                prevJoystickInput = new Vector2(h, v);

                // Do NOT use D-pad for cursor movement: stick only controls cursor/targeting.
                // D-pad is reserved for mask selection handled in ProcessMaskCommands().
            }
        }

        if (movement != Vector2Int.zero)
        {
            if (isTargeting)
            {
                MoveTargetCursor(movement);
            }
            else
            {
                MoveCursor(movement);
            }
        }
    }
    
    private void MoveCursor(Vector2Int delta)
    {
        var church = cult.church;
        Vector2Int newPos = cursorPosition + delta;

        // Clamp to grid bounds
        newPos.x = Mathf.Clamp(newPos.x, 0, church.GridWidth - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, church.GridHeight - 1);

        if (newPos != cursorPosition)
        {
            cursorPosition = newPos;
            AudioManager.PlayUICursorMove();
            OnCursorMoved?.Invoke(cursorPosition);

            var room = church.GetRoomAt(cursorPosition);
            if (room != null)
            {
                OnRoomSelected?.Invoke(room);
            }
        }
    }
    
    private void MoveTargetCursor(Vector2Int delta)
    {
        // Get opponent's church for targeting
        var opponent = GameManager.Instance?.GetOpponent(cult);
        if (opponent?.church == null) return;

        var church = opponent.church;
        Vector2Int newPos = targetPosition + delta;
        Vector2Int oldPos = targetPosition;

        // For column targeting, only allow horizontal movement
        if (currentTargetingMode == TargetingMode.Column)
        {
            // Ignore vertical movement
            newPos.y = targetPosition.y;
        }

        // Clamp to opponent's grid
        newPos.x = Mathf.Clamp(newPos.x, 0, church.GridWidth - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, church.GridHeight - 1);

        targetPosition = newPos;

        // Play cursor sound if position changed
        if (targetPosition != oldPos)
        {
            AudioManager.PlayUICursorMove();
        }

        // Notify of column change for visual feedback
        if (currentTargetingMode == TargetingMode.Column)
        {
            OnTargetColumnChanged?.Invoke(targetPosition.x);
        }
    }
    
    // Event for column targeting visual feedback
    public event System.Action<int> OnTargetColumnChanged;
    
    private void ProcessFollowerCommands()
    {
        if (isTargeting) return;
        
        bool sendToSanctuaryPressed = false;
        bool sendFromSanctuaryPressed = false;
        
        // Keyboard fallback
        if (!bindings.useGamepad)
        {
            if (Input.GetKeyDown(bindings.sendToSanctuary)) sendToSanctuaryPressed = true;
            if (Input.GetKeyDown(bindings.sendFromSanctuary)) sendFromSanctuaryPressed = true;
        }
        else
        {
            var gp = GetAssignedGamepad();
            if (gp != null)
            {
                if (bindings.gamepadSendToSanctuary >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadSendToSanctuary)) sendToSanctuaryPressed = true;
                if (bindings.gamepadSendFromSanctuary >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadSendFromSanctuary)) sendFromSanctuaryPressed = true;
            }
        }
        
        if (sendToSanctuaryPressed)
        {
            SendLowestCommitmentToSanctuary();
        }
        
        if (sendFromSanctuaryPressed)
        {
            SendHighestCommitmentFromSanctuary();
        }
    }
    
    private void SendLowestCommitmentToSanctuary()
    {
        var currentRoom = cult.church.GetRoomAt(cursorPosition);
        if (currentRoom == null || currentRoom.Type == RoomType.Sanctuary) return;
        if (currentRoom.Followers.Count == 0) return;
        
        var sanctuary = cult.church.GetRoomOfType(RoomType.Sanctuary);
        if (sanctuary == null) return;
        
        // Find lowest commitment follower in current room
        Follower lowestFollower = null;
        float lowestCommitment = float.MaxValue;
        
        foreach (var follower in currentRoom.Followers)
        {
            if (follower.Commitment < lowestCommitment)
            {
                lowestCommitment = follower.Commitment;
                lowestFollower = follower;
            }
        }
        
        if (lowestFollower == null) return;
        
        if (sanctuary.HasSpace)
        {
            // Simple move - sanctuary has space
            currentRoom.RemoveFollower(lowestFollower);
            sanctuary.AddFollower(lowestFollower);
            Debug.Log($"Moved {lowestFollower.name} to Sanctuary (commitment: {lowestCommitment:F0})");
        }
        else
        {
            // Sanctuary is full - swap with highest commitment follower in sanctuary
            Follower highestInSanctuary = null;
            float highestCommitment = float.MinValue;
            
            foreach (var follower in sanctuary.Followers)
            {
                if (follower.Commitment > highestCommitment)
                {
                    highestCommitment = follower.Commitment;
                    highestInSanctuary = follower;
                }
            }
            
            if (highestInSanctuary != null)
            {
                // Perform swap - remove from both rooms first to ensure space
                currentRoom.RemoveFollower(lowestFollower);
                sanctuary.RemoveFollower(highestInSanctuary);
                // Now add to swapped locations
                sanctuary.AddFollower(lowestFollower);
                currentRoom.AddFollower(highestInSanctuary);
                Debug.Log($"Swapped {lowestFollower.name} (commitment: {lowestCommitment:F0}) with {highestInSanctuary.name} (commitment: {highestCommitment:F0})");
            }
        }
    }
    
    private void SendHighestCommitmentFromSanctuary()
    {
        var currentRoom = cult.church.GetRoomAt(cursorPosition);
        if (currentRoom == null) return;
        
        var sanctuary = cult.church.GetRoomOfType(RoomType.Sanctuary);
        if (sanctuary == null) return;
        
        // If cursor is on Sanctuary, find lowest commitment pawn in entire church and send to sanctuary
        if (currentRoom.Type == RoomType.Sanctuary)
        {
            if (!sanctuary.HasSpace) return;
            
            // Find lowest commitment follower across all non-sanctuary rooms
            Follower lowestFollower = null;
            float lowestCommitment = float.MaxValue;
            Room sourceRoom = null;
            
            foreach (var room in cult.church.Rooms)
            {
                if (room.Type == RoomType.Sanctuary) continue;
                
                foreach (var follower in room.Followers)
                {
                    if (follower.Commitment < lowestCommitment)
                    {
                        lowestCommitment = follower.Commitment;
                        lowestFollower = follower;
                        sourceRoom = room;
                    }
                }
            }
            
            if (lowestFollower != null && sourceRoom != null)
            {
                sourceRoom.RemoveFollower(lowestFollower);
                sanctuary.AddFollower(lowestFollower);
                Debug.Log($"Auto-moved {lowestFollower.name} from {sourceRoom.Type} to Sanctuary (commitment: {lowestCommitment:F0})");
            }
            return;
        }
        
        // Original behavior: cursor on non-sanctuary room, send highest from sanctuary to current room
        if (!currentRoom.HasSpace) return;
        if (sanctuary.Followers.Count == 0) return;
        
        // Find highest commitment follower in sanctuary
        Follower highestFollower = null;
        float highestCommitment = float.MinValue;
        
        foreach (var follower in sanctuary.Followers)
        {
            if (follower.Commitment > highestCommitment)
            {
                highestCommitment = follower.Commitment;
                highestFollower = follower;
            }
        }
        
        if (highestFollower != null)
        {
            sanctuary.RemoveFollower(highestFollower);
            currentRoom.AddFollower(highestFollower);
            Debug.Log($"Moved {highestFollower.name} to {currentRoom.Type} (commitment: {highestCommitment:F0})");
        }
    }
    
    private void ProcessRoomCommands()
    {
        if (isTargeting) return;
        
        bool upgradeRoomPressed = false;
        
        // Keyboard fallback
        if (!bindings.useGamepad)
        {
            if (Input.GetKeyDown(bindings.upgradeRoom)) upgradeRoomPressed = true;
        }
        else
        {
            var gp = GetAssignedGamepad();
            if (gp != null && bindings.gamepadUpgradeRoom >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadUpgradeRoom))
            {
                upgradeRoomPressed = true;
            }
        }
        
        if (upgradeRoomPressed)
        {
            UpgradeCurrentRoom();
        }
    }
    
    private void UpgradeCurrentRoom()
    {
        var currentRoom = cult.church.GetRoomAt(cursorPosition);
        if (currentRoom == null)
        {
            Debug.Log("No room at cursor position to upgrade");
            AudioManager.PlayUIError();
            return;
        }

        // For unbuilt rooms (level 0), cost is the build cost
        // For built rooms, nth upgrade costs n wealth (so level 1->2 costs 1, level 2->3 costs 2, etc.)
        int upgradeCost = currentRoom.UpgradeCost;

        if (!cult.SpendMoney(upgradeCost))
        {
            string action = currentRoom.IsBuilt ? "upgrade" : "build";
            Debug.Log($"Not enough money to {action} {currentRoom.Type} (need {upgradeCost})");
            AudioManager.PlayUIError();
            return;
        }

        currentRoom.Upgrade();
        string actionDone = currentRoom.Level == 1 ? "Built" : "Upgraded";
        Debug.Log($"{actionDone} {currentRoom.Type} to level {currentRoom.Level} (cost: {upgradeCost})");
    }
    
    private void ProcessMaskCommands()
    {
        if (cult.god == null) return;
        
        int maskSlot = -1;
        
        // Keyboard fallback
        if (!bindings.useGamepad)
        {
            if (Input.GetKeyDown(bindings.useMask1)) maskSlot = 0;
            else if (Input.GetKeyDown(bindings.useMask2)) maskSlot = 1;
            else if (Input.GetKeyDown(bindings.useMask3)) maskSlot = 2;
            else if (Input.GetKeyDown(bindings.useMask4)) maskSlot = 3;
        }
        else
        {
            var gp = GetAssignedGamepad();
            if (gp != null)
            {
                if (gp.dpad.up.wasPressedThisFrame && !isTargeting) maskSlot = 0;
                else if (gp.dpad.down.wasPressedThisFrame && !isTargeting) maskSlot = 1;
                else if (gp.dpad.left.wasPressedThisFrame && !isTargeting) maskSlot = 2;
                else if (gp.dpad.right.wasPressedThisFrame && !isTargeting) maskSlot = 3;

                // Also allow face buttons if bindings specify them
                if (maskSlot < 0)
                {
                    if (bindings.gamepadUseMask1 >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadUseMask1) && !isTargeting) maskSlot = 0;
                    else if (bindings.gamepadUseMask2 >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadUseMask2) && !isTargeting) maskSlot = 1;
                    else if (bindings.gamepadUseMask3 >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadUseMask3) && !isTargeting) maskSlot = 2;
                    else if (bindings.gamepadUseMask4 >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadUseMask4) && !isTargeting) maskSlot = 3;
                }
            }
        }
        
        if (maskSlot >= 0)
        {
            ActivateMask(maskSlot);
        }
    }
    
    private void ActivateMask(int slot)
    {
        var masks = cult.god.Masks;
        if (slot >= masks.Count || masks[slot] == null)
        {
            Debug.Log($"No mask in slot {slot + 1}");
            AudioManager.PlayUIError();
            return;
        }
        
        var mask = masks[slot];
        
        // Handle different targeting types
        switch (mask.TargetType)
        {
            case MaskTargetType.EnemyRoom:
                StartTargeting(slot, TargetingMode.Room);
                break;
            case MaskTargetType.EnemyColumn:
                StartTargeting(slot, TargetingMode.Column);
                break;
            case MaskTargetType.EnemyBottomRow:
                // Flood - no targeting needed, hits bottom row automatically
                ApplyFloodMask(slot);
                break;
            case MaskTargetType.Passive:
                // Shield masks cannot be manually activated
                Debug.Log("Shield masks are passive and activate automatically when attacked!");
                break;
            case MaskTargetType.OwnRoom:
                // Apply to currently selected own room
                ApplyMaskToSelf(slot);
                break;
            default:
                // Apply immediately (no targeting needed)
                ApplyMaskToSelf(slot);
                break;
        }
    }
    
    private TargetingMode currentTargetingMode = TargetingMode.Room;
    
    private void StartTargeting(int slot, TargetingMode mode = TargetingMode.Room)
    {
        isTargeting = true;
        activeMaskSlot = slot;
        currentTargetingMode = mode;
        targetPosition = Vector2Int.zero;
        OnMaskActivated?.Invoke(slot);
        
        string modeDesc = mode == TargetingMode.Column ? "column" : "room";
        Debug.Log($"Targeting mode activated for mask slot {slot + 1} ({modeDesc})");
    }
    
    private void ApplyFloodMask(int slot)
    {
        var mask = cult.god.Masks[slot];
        var opponent = GameManager.Instance?.GetOpponent(cult);

        if (opponent?.church == null)
        {
            Debug.Log("No opponent to target!");
            AudioManager.PlayUIError();
            return;
        }

        // Check if we can afford the mask
        if (!mask.CanAfford(cult))
        {
            Debug.Log($"Cannot afford {mask.Type} mask! Need {mask.FavorCost} favor.");
            AudioManager.PlayUIError();
            return;
        }

        // Play select sound for confirmed action
        AudioManager.PlayUISelect();

        // Pay the cost
        mask.PayCost(cult);

        // Apply flood effect (hits entire bottom row)
        mask.ApplyEffect(cult, null, opponent.god, -1, opponent.church);

        // Remove mask after use
        cult.god.RemoveMaskFromStorage(slot);

        Debug.Log($"Used Flood mask on enemy bottom row (spent {mask.FavorCost} favor)");
    }
    
    private void ProcessTargetingInput()
    {
        if (!isTargeting) return;
        
        bool confirmPressed = false;
        bool cancelPressed = false;
        
        // Keyboard fallback
        if (!bindings.useGamepad)
        {
            if (Input.GetKeyDown(bindings.confirmTarget)) confirmPressed = true;
            if (Input.GetKeyDown(bindings.cancelTarget)) cancelPressed = true;
        }
        else
        {
            var gp = GetAssignedGamepad();
            if (gp != null)
            {
                if (bindings.gamepadConfirmTarget >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadConfirmTarget)) confirmPressed = true;
                if (bindings.gamepadCancelTarget >= 0 && GamepadButtonWasPressed(gp, bindings.gamepadCancelTarget)) cancelPressed = true;
            }
        }
        
        if (confirmPressed)
        {
            ConfirmTarget();
        }
        
        if (cancelPressed)
        {
            CancelTargeting();
        }
    }
    
    private void ConfirmTarget()
    {
        var opponent = GameManager.Instance?.GetOpponent(cult);
        if (opponent?.church == null) return;

        var targetRoom = opponent.church.GetRoomAt(targetPosition);

        if (activeMaskSlot >= 0 && cult.god.Masks.Count > activeMaskSlot)
        {
            var mask = cult.god.Masks[activeMaskSlot];

            // Check if we can afford the mask
            if (!mask.CanAfford(cult))
            {
                Debug.Log($"Cannot afford {mask.Type} mask! Need {mask.FavorCost} favor.");
                AudioManager.PlayUIError();
                CancelTargeting();
                return;
            }

            // Play select sound for confirmed action
            AudioManager.PlayUISelect();

            // Pay the cost first
            mask.PayCost(cult);

            // Apply mask effect based on targeting mode
            if (currentTargetingMode == TargetingMode.Column)
            {
                // Lightning mask - pass column info
                mask.ApplyEffect(cult, targetRoom, opponent.god, targetPosition.x, opponent.church);
                Debug.Log($"Used {mask.Type} on enemy column {targetPosition.x} (spent {mask.FavorCost} favor)");
            }
            else
            {
                // Standard room targeting
                mask.ApplyEffect(cult, targetRoom, opponent.god);
                Debug.Log($"Used {mask.Type} on enemy {targetRoom?.Type.ToString() ?? "empty slot"} (spent {mask.FavorCost} favor)");
            }

            // Remove mask after use by re-getting from storage and removing at index
            cult.god.RemoveMaskFromStorage(activeMaskSlot);

            OnTargetConfirmed?.Invoke(targetRoom);
        }

        StopTargeting();
    }
    
    private void CancelTargeting()
    {
        Debug.Log("Targeting cancelled");
        AudioManager.PlayUICancel();
        OnTargetCancelled?.Invoke();
        StopTargeting();
    }
    
    private void StopTargeting()
    {
        isTargeting = false;
        activeMaskSlot = -1;
    }
    
    private void ApplyMaskToSelf(int slot)
    {
        var mask = cult.god.Masks[slot];

        // Check if we can afford the mask
        if (!mask.CanAfford(cult))
        {
            Debug.Log($"Cannot afford {mask.Type} mask! Need {mask.FavorCost} favor.");
            AudioManager.PlayUIError();
            return;
        }

        // Play select sound for confirmed action
        AudioManager.PlayUISelect();

        // Pay the cost first
        mask.PayCost(cult);

        // Apply to own room at cursor (sourceCult, targetRoom, targetGod)
        var selfRoom = cult.church.GetRoomAt(cursorPosition);
        mask.ApplyEffect(cult, selfRoom, cult.god);

        // Remove mask after use
        cult.god.RemoveMaskFromStorage(slot);

        Debug.Log($"Applied {mask.Type} to own {selfRoom?.Type.ToString() ?? "empty slot"} (spent {mask.FavorCost} favor)");
    }
    
    /// <summary>
    /// Get the currently selected room.
    /// </summary>
    public Room GetSelectedRoom()
    {
        return cult?.church?.GetRoomAt(cursorPosition);
    }
    
    /// <summary>
    /// Get the currently targeted room (enemy).
    /// </summary>
    public Room GetTargetedRoom()
    {
        if (!isTargeting) return null;
        var opponent = GameManager.Instance?.GetOpponent(cult);
        return opponent?.church?.GetRoomAt(targetPosition);
    }

    // --- External Input System hooks (for PlayerInput / Input System bridging) ---

    // Receive stick input (Vector2) from Input System
    public void ExternalMove(Vector2 stickValue)
    {
        Vector2Int movement = Vector2Int.zero;
        float h = stickValue.x;
        float v = stickValue.y;

        if (Mathf.Abs(h) > joystickDeadzone && Mathf.Abs(prevJoystickInput.x) <= joystickDeadzone)
        {
            movement.x = h > 0 ? 1 : -1;
        }
        if (Mathf.Abs(v) > joystickDeadzone && Mathf.Abs(prevJoystickInput.y) <= joystickDeadzone)
        {
            movement.y = v > 0 ? 1 : -1;
        }

        prevJoystickInput = stickValue;

        if (movement != Vector2Int.zero)
        {
            if (isTargeting) MoveTargetCursor(movement);
            else MoveCursor(movement);
        }
    }

    // Receive D-pad input (Vector2) from Input System
    public void ExternalDpad(Vector2 dpadValue)
    {
        // D-pad should be used only for mask selection, not for moving cursor/target.
        if (isTargeting) return;

        if (dpadValue.y > 0.5f) ActivateMask(0);
        else if (dpadValue.y < -0.5f) ActivateMask(1);
        else if (dpadValue.x < -0.5f) ActivateMask(2);
        else if (dpadValue.x > 0.5f) ActivateMask(3);
    }

    // Buttons
    public void ExternalConfirm()
    {
        if (isTargeting) ConfirmTarget();
    }

    public void ExternalCancel()
    {
        if (isTargeting) CancelTargeting();
    }

    public void ExternalSendTo()
    {
        if (!isTargeting) SendLowestCommitmentToSanctuary();
    }

    public void ExternalSendFrom()
    {
        if (!isTargeting) SendHighestCommitmentFromSanctuary();
    }

    public void ExternalUpgrade()
    {
        if (!isTargeting) UpgradeCurrentRoom();
    }

    public void ExternalUseMask(int slot)
    {
        if (!isTargeting) ActivateMask(slot);
    }
}
