using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for the room layout of a cult's church.
/// Manages a grid of rooms where followers can be assigned.
/// </summary>
public class Church : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField] private int gridWidth = 3;
    [SerializeField] private int gridHeight = 4;
    
    [Header("Rooms")]
    [SerializeField] private List<Room> rooms = new();
    
    /// <summary>Reference to the cult that owns this church.</summary>
    private Cult cult;
    
    // === Properties ===
    
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public IReadOnlyList<Room> Rooms => rooms;
    
    // === Initialization ===
    
    /// <summary>
    /// Initialize the church with a reference to its owning cult.
    /// </summary>
    public void Initialize(Cult cult)
    {
        this.cult = cult;
        
        // Initialize all existing rooms
        foreach (var room in rooms)
        {
            if (room != null)
            {
                room.Initialize(this, cult, room.Location);
            }
        }
    }
    
    // === Room Management ===
    
    /// <summary>
    /// Add a room to the church at a specific position.
    /// </summary>
    public bool AddRoom(Room room, Vector2Int position)
    {
        if (!IsValidPosition(position))
        {
            Debug.LogWarning($"Invalid room position: {position}");
            return false;
        }
        
        if (GetRoomAt(position) != null)
        {
            Debug.LogWarning($"Position {position} already has a room!");
            return false;
        }
        
        room.Initialize(this, cult, position);
        rooms.Add(room);
        return true;
    }
    
    /// <summary>
    /// Remove a room from the church.
    /// </summary>
    public bool RemoveRoom(Room room)
    {
        return rooms.Remove(room);
    }
    
    /// <summary>
    /// Get the room at a specific grid position.
    /// </summary>
    /// <returns>The room at the position, or null if empty.</returns>
    public Room GetRoomAt(Vector2Int position)
    {
        foreach (var room in rooms)
        {
            if (room != null && room.Location == position)
            {
                return room;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get the room at a specific grid position (x, y overload).
    /// </summary>
    public Room GetRoomAt(int x, int y)
    {
        return GetRoomAt(new Vector2Int(x, y));
    }
    
    /// <summary>
    /// Check if a position is within the grid bounds.
    /// </summary>
    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth &&
               position.y >= 0 && position.y < gridHeight;
    }
    
    /// <summary>
    /// Get all empty positions in the grid.
    /// </summary>
    public List<Vector2Int> GetEmptyPositions()
    {
        var empty = new List<Vector2Int>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var pos = new Vector2Int(x, y);
                if (GetRoomAt(pos) == null)
                {
                    empty.Add(pos);
                }
            }
        }
        
        return empty;
    }
    
    /// <summary>
    /// Find the first room of a specific type.
    /// </summary>
    public Room GetRoomOfType(RoomType type)
    {
        foreach (var room in rooms)
        {
            if (room != null && room.Type == type)
            {
                return room;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get all rooms of a specific type.
    /// </summary>
    public List<Room> GetRoomsOfType(RoomType type)
    {
        var result = new List<Room>();
        foreach (var room in rooms)
        {
            if (room != null && room.Type == type)
            {
                result.Add(room);
            }
        }
        return result;
    }
}
