using System;
using UnityEngine;

/// <summary>
/// Defines all player actions and their default key bindings.
/// </summary>
[Serializable]
public class PlayerInputBindings
{
    [Header("Cursor Movement")]
    public KeyCode cursorUp = KeyCode.W;
    public KeyCode cursorDown = KeyCode.S;
    public KeyCode cursorLeft = KeyCode.A;
    public KeyCode cursorRight = KeyCode.D;
    
    [Header("Follower Commands")]
    public KeyCode sendToSanctuary = KeyCode.Z;
    public KeyCode sendFromSanctuary = KeyCode.X;
    
    [Header("Room Commands")]
    public KeyCode upgradeRoom = KeyCode.Q;
    
    [Header("Mask Commands")]
    public KeyCode useMask1 = KeyCode.Alpha1;
    public KeyCode useMask2 = KeyCode.Alpha2;
    public KeyCode useMask3 = KeyCode.Alpha3;
    public KeyCode useMask4 = KeyCode.Alpha4;
    
    [Header("Targeting")]
    public KeyCode confirmTarget = KeyCode.BackQuote;
    public KeyCode cancelTarget = KeyCode.LeftControl;
    
    /// <summary>
    /// Create default bindings for Player 1 (WASD layout).
    /// </summary>
    public static PlayerInputBindings CreatePlayer1Defaults()
    {
        return new PlayerInputBindings
        {
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
    /// Clone these bindings.
    /// </summary>
    public PlayerInputBindings Clone()
    {
        return new PlayerInputBindings
        {
            cursorUp = cursorUp,
            cursorDown = cursorDown,
            cursorLeft = cursorLeft,
            cursorRight = cursorRight,
            sendToSanctuary = sendToSanctuary,
            sendFromSanctuary = sendFromSanctuary,
            upgradeRoom = upgradeRoom,
            useMask1 = useMask1,
            useMask2 = useMask2,
            useMask3 = useMask3,
            useMask4 = useMask4,
            confirmTarget = confirmTarget,
            cancelTarget = cancelTarget
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
    
    private void Update()
    {
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
        
        if (Input.GetKeyDown(bindings.cursorUp)) movement.y = 1;
        if (Input.GetKeyDown(bindings.cursorDown)) movement.y = -1;
        if (Input.GetKeyDown(bindings.cursorLeft)) movement.x = -1;
        if (Input.GetKeyDown(bindings.cursorRight)) movement.x = 1;
        
        if (movement != Vector2Int.zero)
        {
            if (isTargeting)
            {
                // Move target cursor
                MoveTargetCursor(movement);
            }
            else
            {
                // Move room cursor
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
        
        // Clamp to opponent's grid
        newPos.x = Mathf.Clamp(newPos.x, 0, church.GridWidth - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, church.GridHeight - 1);
        
        targetPosition = newPos;
    }
    
    private void ProcessFollowerCommands()
    {
        if (isTargeting) return;
        
        if (Input.GetKeyDown(bindings.sendToSanctuary))
        {
            SendLowestCommitmentToSanctuary();
        }
        
        if (Input.GetKeyDown(bindings.sendFromSanctuary))
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
        
        if (Input.GetKeyDown(bindings.upgradeRoom))
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
            return;
        }
        
        // nth upgrade costs n wealth (so level 1->2 costs 1, level 2->3 costs 2, etc.)
        int upgradeCost = currentRoom.Level;
        
        if (!cult.SpendMoney(upgradeCost))
        {
            Debug.Log($"Not enough money to upgrade {currentRoom.Type} (need {upgradeCost})");
            return;
        }
        
        currentRoom.Upgrade();
        Debug.Log($"Upgraded {currentRoom.Type} to level {currentRoom.Level} (cost: {upgradeCost})");
    }
    
    private void ProcessMaskCommands()
    {
        if (cult.god == null) return;
        
        int maskSlot = -1;
        
        if (Input.GetKeyDown(bindings.useMask1)) maskSlot = 0;
        else if (Input.GetKeyDown(bindings.useMask2)) maskSlot = 1;
        else if (Input.GetKeyDown(bindings.useMask3)) maskSlot = 2;
        else if (Input.GetKeyDown(bindings.useMask4)) maskSlot = 3;
        
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
            return;
        }
        
        var mask = masks[slot];
        
        // Check if mask requires targeting an enemy
        if (mask.TargetType == MaskTargetType.EnemyRoom)
        {
            // Enter targeting mode
            StartTargeting(slot);
        }
        else
        {
            // Apply immediately to self
            ApplyMaskToSelf(slot);
        }
    }
    
    private void StartTargeting(int slot)
    {
        isTargeting = true;
        activeMaskSlot = slot;
        targetPosition = Vector2Int.zero;
        OnMaskActivated?.Invoke(slot);
        Debug.Log($"Targeting mode activated for mask slot {slot + 1}");
    }
    
    private void ProcessTargetingInput()
    {
        if (!isTargeting) return;
        
        if (Input.GetKeyDown(bindings.confirmTarget))
        {
            ConfirmTarget();
        }
        
        if (Input.GetKeyDown(bindings.cancelTarget))
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
                CancelTargeting();
                return;
            }
            
            // Pay the cost first
            mask.PayCost(cult);
            
            // Apply mask effect to target (sourceCult, targetRoom, targetGod)
            mask.ApplyEffect(cult, targetRoom, opponent.god);
            
            // Remove mask after use by re-getting from storage and removing at index
            cult.god.RemoveMaskFromStorage(activeMaskSlot);
            
            Debug.Log($"Used {mask.Type} on enemy {targetRoom?.Type.ToString() ?? "empty slot"} (spent {mask.FavorCost} favor)");
            OnTargetConfirmed?.Invoke(targetRoom);
        }
        
        StopTargeting();
    }
    
    private void CancelTargeting()
    {
        Debug.Log("Targeting cancelled");
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
            return;
        }
        
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
}
