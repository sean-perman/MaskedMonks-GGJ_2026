using UnityEngine;

/// <summary>
/// Base class for all game effects. Create instances of derived classes
/// as ScriptableObject assets that can be assigned to masks or rooms.
/// </summary>
public abstract class GameEffect : ScriptableObject
{
    [Header("Effect Info")]
    public string effectName;
    [TextArea] public string description;
    
    /// <summary>
    /// Apply this effect using the provided context.
    /// </summary>
    /// <param name="context">The context containing references to game objects.</param>
    public abstract void Apply(EffectContext context);
}

/// <summary>
/// Context object that provides references to game objects for effects.
/// Passed to effects when they are applied.
/// </summary>
[System.Serializable]
public class EffectContext
{
    public God targetGod;
    public Church targetRoom;
    public Cult targetCult;
    
    // Optional: source information for effects that need it
    public GameObject source;
    
    public EffectContext() { }
    
    public EffectContext(God god, Church room, Cult cult)
    {
        targetGod = god;
        targetRoom = room;
        targetCult = cult;
    }
}
