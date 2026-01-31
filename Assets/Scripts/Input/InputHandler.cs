using System;
using UnityEngine;

/// <summary>
/// Handles input for one player using controller or keyboard.
/// Attach one instance per player.
/// </summary>
public class InputHandler : MonoBehaviour
{
    [Header("Player Configuration")]
    [SerializeField] private int playerIndex = 0;  // 0 = Player 1, 1 = Player 2
    [SerializeField] private Cult cult;
    
    [Header("Keyboard Bindings (Player 1)")]
    [SerializeField] private KeyCode navigateUp = KeyCode.W;
    [SerializeField] private KeyCode navigateDown = KeyCode.S;
    [SerializeField] private KeyCode navigateLeft = KeyCode.A;
    [SerializeField] private KeyCode navigateRight = KeyCode.D;
    [SerializeField] private KeyCode confirm = KeyCode.Space;
    [SerializeField] private KeyCode cancel = KeyCode.Escape;
    [SerializeField] private KeyCode assignFollower = KeyCode.Q;
    [SerializeField] private KeyCode unassignFollower = KeyCode.E;
    [SerializeField] private KeyCode mask1 = KeyCode.Alpha1;
    [SerializeField] private KeyCode mask2 = KeyCode.Alpha2;
    [SerializeField] private KeyCode mask3 = KeyCode.Alpha3;
    [SerializeField] private KeyCode mask4 = KeyCode.Alpha4;
    [SerializeField] private KeyCode upgradeRoom = KeyCode.U;
    
    [Header("Keyboard Bindings (Player 2 - Arrows)")]
    [SerializeField] private KeyCode p2NavigateUp = KeyCode.UpArrow;
    [SerializeField] private KeyCode p2NavigateDown = KeyCode.DownArrow;
    [SerializeField] private KeyCode p2NavigateLeft = KeyCode.LeftArrow;
    [SerializeField] private KeyCode p2NavigateRight = KeyCode.RightArrow;
    [SerializeField] private KeyCode p2Confirm = KeyCode.Return;
    [SerializeField] private KeyCode p2Cancel = KeyCode.Backspace;
    [SerializeField] private KeyCode p2AssignFollower = KeyCode.RightShift;
    [SerializeField] private KeyCode p2UnassignFollower = KeyCode.RightControl;
    [SerializeField] private KeyCode p2Mask1 = KeyCode.Keypad1;
    [SerializeField] private KeyCode p2Mask2 = KeyCode.Keypad2;
    [SerializeField] private KeyCode p2Mask3 = KeyCode.Keypad3;
    [SerializeField] private KeyCode p2Mask4 = KeyCode.Keypad4;
    [SerializeField] private KeyCode p2UpgradeRoom = KeyCode.KeypadPlus;
    
    [Header("Selection State")]
    [SerializeField] private Vector2Int selectedRoomPosition = Vector2Int.zero;
    [SerializeField] private Room selectedRoom;
    [SerializeField] private bool isTargetingEnemy = false;
    [SerializeField] private Vector2Int targetRoomPosition = Vector2Int.zero;
    
    [Header("Controller Settings")]
    [SerializeField] private float joystickDeadzone = 0.5f;
    [SerializeField] private float navigationCooldown = 0.2f;
    private float navigationTimer = 0f;
    
    // === Events ===
    
    public event Action<Room> OnRoomSelected;
    public event Action<Room> OnRoomConfirmed;
    public event Action OnSelectionCancelled;
    public event Action<int> OnMaskActivated;
    public event Action<Room> OnFollowerAssigned;
    public event Action<Room> OnFollowerUnassigned;
    public event Action<Room> OnRoomUpgraded;
    
    // === Properties ===
    
    public Cult Cult => cult;
    public Room SelectedRoom => selectedRoom;
    public Vector2Int SelectedPosition => selectedRoomPosition;
    public bool IsTargetingEnemy => isTargetingEnemy;
    
    // === Unity Lifecycle ===
    
    private void Update()
    {
        if (cult == null || cult.church == null) return;
        
        navigationTimer -= Time.deltaTime;
        
        HandleNavigation();
        HandleActions();
        HandleMasks();
    }
    
    // === Navigation ===
    
    private void HandleNavigation()
    {
        Vector2Int direction = GetNavigationInput();
        
        if (direction != Vector2Int.zero && navigationTimer <= 0f)
        {
            if (isTargetingEnemy)
            {
                NavigateEnemyRooms(direction);
            }
            else
            {
                NavigateOwnRooms(direction);
            }
            navigationTimer = navigationCooldown;
        }
    }
    
    private Vector2Int GetNavigationInput()
    {
        Vector2Int dir = Vector2Int.zero;
        
        if (playerIndex == 0)
        {
            // Player 1: WASD
            if (Input.GetKey(navigateUp)) dir.y += 1;
            if (Input.GetKey(navigateDown)) dir.y -= 1;
            if (Input.GetKey(navigateLeft)) dir.x -= 1;
            if (Input.GetKey(navigateRight)) dir.x += 1;
        }
        else
        {
            // Player 2: Arrow keys
            if (Input.GetKey(p2NavigateUp)) dir.y += 1;
            if (Input.GetKey(p2NavigateDown)) dir.y -= 1;
            if (Input.GetKey(p2NavigateLeft)) dir.x -= 1;
            if (Input.GetKey(p2NavigateRight)) dir.x += 1;
        }
        
        // Controller support (left stick for own rooms)
        float horizontal = Input.GetAxis($"Horizontal");
        float vertical = Input.GetAxis($"Vertical");
        
        if (Mathf.Abs(horizontal) > joystickDeadzone)
            dir.x = horizontal > 0 ? 1 : -1;
        if (Mathf.Abs(vertical) > joystickDeadzone)
            dir.y = vertical > 0 ? 1 : -1;
        
        // Clamp to single direction per frame
        if (dir.x != 0 && dir.y != 0)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                dir.y = 0;
            else
                dir.x = 0;
        }
        
        return dir;
    }
    
    private void NavigateOwnRooms(Vector2Int direction)
    {
        Vector2Int newPos = selectedRoomPosition + direction;
        
        // Clamp to grid bounds
        newPos.x = Mathf.Clamp(newPos.x, 0, cult.church.GridWidth - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, cult.church.GridHeight - 1);
        
        if (newPos != selectedRoomPosition)
        {
            selectedRoomPosition = newPos;
            selectedRoom = cult.church.GetRoomAt(selectedRoomPosition);
            OnRoomSelected?.Invoke(selectedRoom);
            
            Debug.Log($"Selected position: {selectedRoomPosition}, Room: {selectedRoom?.Type.ToString() ?? "Empty"}");
        }
    }
    
    private void NavigateEnemyRooms(Vector2Int direction)
    {
        var opponent = GameManager.Instance?.GetOpponent(cult);
        if (opponent?.church == null) return;
        
        Vector2Int newPos = targetRoomPosition + direction;
        
        // Clamp to enemy grid bounds
        newPos.x = Mathf.Clamp(newPos.x, 0, opponent.church.GridWidth - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, opponent.church.GridHeight - 1);
        
        if (newPos != targetRoomPosition)
        {
            targetRoomPosition = newPos;
            var targetRoom = opponent.church.GetRoomAt(targetRoomPosition);
            Debug.Log($"Targeting enemy position: {targetRoomPosition}, Room: {targetRoom?.Type.ToString() ?? "Empty"}");
        }
    }
    
    // === Actions ===
    
    private void HandleActions()
    {
        bool confirmPressed = playerIndex == 0 ? Input.GetKeyDown(confirm) : Input.GetKeyDown(p2Confirm);
        bool cancelPressed = playerIndex == 0 ? Input.GetKeyDown(cancel) : Input.GetKeyDown(p2Cancel);
        bool assignPressed = playerIndex == 0 ? Input.GetKeyDown(assignFollower) : Input.GetKeyDown(p2AssignFollower);
        bool unassignPressed = playerIndex == 0 ? Input.GetKeyDown(unassignFollower) : Input.GetKeyDown(p2UnassignFollower);
        bool upgradePressed = playerIndex == 0 ? Input.GetKeyDown(upgradeRoom) : Input.GetKeyDown(p2UpgradeRoom);
        
        if (confirmPressed)
        {
            if (isTargetingEnemy)
            {
                // Confirm enemy target selection
                var opponent = GameManager.Instance?.GetOpponent(cult);
                var targetRoom = opponent?.church?.GetRoomAt(targetRoomPosition);
                OnRoomConfirmed?.Invoke(targetRoom);
                isTargetingEnemy = false;
            }
            else
            {
                OnRoomConfirmed?.Invoke(selectedRoom);
            }
        }
        
        if (cancelPressed)
        {
            if (isTargetingEnemy)
            {
                isTargetingEnemy = false;
                Debug.Log("Cancelled enemy targeting");
            }
            OnSelectionCancelled?.Invoke();
        }
        
        if (assignPressed && selectedRoom != null)
        {
            TryAssignFollower();
        }
        
        if (unassignPressed && selectedRoom != null)
        {
            TryUnassignFollower();
        }
        
        if (upgradePressed && selectedRoom != null)
        {
            TryUpgradeRoom();
        }
    }
    
    private void TryAssignFollower()
    {
        if (selectedRoom == null || !selectedRoom.HasSpace) return;
        
        // Get a follower from sanctuary
        var sanctuary = cult.church.GetRoomOfType(RoomType.Sanctuary);
        if (sanctuary != null && sanctuary.Followers.Count > 0)
        {
            var follower = sanctuary.Followers[0] as Follower;
            if (follower != null)
            {
                sanctuary.RemoveFollower(follower);
                selectedRoom.AddFollower(follower);
                OnFollowerAssigned?.Invoke(selectedRoom);
                Debug.Log($"Assigned follower to {selectedRoom.Type}");
            }
        }
        else
        {
            Debug.Log("No followers available in Sanctuary to assign");
        }
    }
    
    private void TryUnassignFollower()
    {
        if (selectedRoom == null || selectedRoom.Followers.Count == 0) return;
        if (selectedRoom.Type == RoomType.Sanctuary) return; // Can't unassign from sanctuary
        
        var sanctuary = cult.church.GetRoomOfType(RoomType.Sanctuary);
        if (sanctuary != null)
        {
            var follower = selectedRoom.Followers[0] as Follower;
            if (follower != null)
            {
                selectedRoom.RemoveFollower(follower);
                sanctuary.AddFollower(follower);
                OnFollowerUnassigned?.Invoke(selectedRoom);
                Debug.Log($"Unassigned follower from {selectedRoom.Type} to Sanctuary");
            }
        }
    }
    
    private void TryUpgradeRoom()
    {
        if (selectedRoom == null) return;
        
        // nth upgrade costs n wealth (level 1->2 costs 1, level 2->3 costs 2, etc.)
        int upgradeCost = selectedRoom.Level;
        
        if (cult.SpendMoney(upgradeCost))
        {
            selectedRoom.IncreaseLevel();
            OnRoomUpgraded?.Invoke(selectedRoom);
            Debug.Log($"Upgraded {selectedRoom.Type} to level {selectedRoom.Level} (cost: {upgradeCost})");
        }
        else
        {
            Debug.Log($"Not enough money to upgrade (need {upgradeCost})");
        }
    }
    
    // === Masks ===
    
    private void HandleMasks()
    {
        bool mask1Pressed = playerIndex == 0 ? Input.GetKeyDown(mask1) : Input.GetKeyDown(p2Mask1);
        bool mask2Pressed = playerIndex == 0 ? Input.GetKeyDown(mask2) : Input.GetKeyDown(p2Mask2);
        bool mask3Pressed = playerIndex == 0 ? Input.GetKeyDown(mask3) : Input.GetKeyDown(p2Mask3);
        bool mask4Pressed = playerIndex == 0 ? Input.GetKeyDown(mask4) : Input.GetKeyDown(p2Mask4);
        
        if (mask1Pressed) TryActivateMask(0);
        if (mask2Pressed) TryActivateMask(1);
        if (mask3Pressed) TryActivateMask(2);
        if (mask4Pressed) TryActivateMask(3);
    }
    
    private void TryActivateMask(int slotIndex)
    {
        if (cult?.god == null) return;
        
        var masks = cult.god.StoredMasks;
        if (slotIndex >= masks.Count)
        {
            Debug.Log($"No mask in slot {slotIndex + 1}");
            return;
        }
        
        var mask = masks[slotIndex];
        
        // Check if we can afford it
        if (!mask.CanAfford(cult))
        {
            Debug.Log($"Cannot afford mask {mask.Type}");
            return;
        }
        
        // Check if targeting is needed
        if (mask.TargetType == MaskTargetType.EnemyRoom)
        {
            // Start enemy targeting mode
            isTargetingEnemy = true;
            targetRoomPosition = Vector2Int.zero;
            Debug.Log("Select an enemy room to target...");
            // Store pending mask for when target is confirmed
            // TODO: Store pending mask index and apply on confirm
        }
        else if (mask.TargetType == MaskTargetType.OwnRoom)
        {
            // Use currently selected own room
            if (selectedRoom != null)
            {
                ActivateMask(slotIndex, selectedRoom, null);
            }
            else
            {
                Debug.Log("Select one of your rooms first");
            }
        }
        else
        {
            // No targeting needed
            ActivateMask(slotIndex, null, null);
        }
    }
    
    private void ActivateMask(int slotIndex, Room targetRoom, God targetGod)
    {
        if (cult?.god == null) return;
        
        var masks = cult.god.StoredMasks;
        if (slotIndex >= masks.Count) return;
        
        var mask = masks[slotIndex];
        
        // Pay the cost
        mask.PayCost(cult);
        
        // Determine target god if attacking enemy
        if (targetGod == null && mask.TargetType == MaskTargetType.EnemyRoom)
        {
            var opponent = GameManager.Instance?.GetOpponent(cult);
            targetGod = opponent?.god;
        }
        
        // Apply the effect
        mask.ApplyEffect(cult, targetRoom, targetGod);
        
        // Remove mask from storage (it's consumed)
        cult.god.SetMask(slotIndex);
        cult.god.ClearMask();
        
        OnMaskActivated?.Invoke(slotIndex);
        Debug.Log($"Activated mask: {mask.Type}");
    }
    
    // === Public Methods ===
    
    /// <summary>
    /// Set the cult this input handler controls.
    /// </summary>
    public void SetCult(Cult newCult)
    {
        cult = newCult;
        selectedRoomPosition = Vector2Int.zero;
        selectedRoom = cult?.church?.GetRoomAt(selectedRoomPosition);
    }
    
    /// <summary>
    /// Start targeting an enemy room (called when a mask requires it).
    /// </summary>
    public void StartEnemyTargeting()
    {
        isTargetingEnemy = true;
        targetRoomPosition = Vector2Int.zero;
    }
}
