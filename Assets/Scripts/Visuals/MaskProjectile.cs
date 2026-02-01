using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Projectile that travels from source to target and applies effect on impact.
/// Used for visual feedback when masks are used.
/// </summary>
public class MaskProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float arcHeight = 1f;
    
    private Vector3 startPos;
    private Vector3 targetPos;
    private float travelTime;
    private float elapsed = 0f;
    private SpriteRenderer spriteRenderer;
    
    private Action onImpact;
    private bool hasArrived = false;
    
    /// <summary>
    /// Initialize and launch the projectile.
    /// </summary>
    /// <param name="start">Starting world position</param>
    /// <param name="target">Target world position</param>
    /// <param name="maskType">Type of mask for visual style</param>
    /// <param name="impactCallback">Called when projectile hits target</param>
    public void Launch(Vector3 start, Vector3 target, MaskType maskType, Action impactCallback)
    {
        startPos = start;
        targetPos = target;
        onImpact = impactCallback;
        
        float distance = Vector3.Distance(start, target);
        travelTime = distance / speed;
        
        transform.position = start;
        
        // Create visual
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateProjectileSprite(maskType);
        spriteRenderer.color = GetProjectileColor(maskType);
        spriteRenderer.sortingOrder = 20;
        
        float size = GetProjectileSize(maskType);
        transform.localScale = new Vector3(size, size, 1f);
    }
    
    private void Update()
    {
        if (hasArrived) return;
        
        elapsed += Time.deltaTime;
        float t = elapsed / travelTime;
        
        if (t >= 1f)
        {
            // Arrived at target
            hasArrived = true;
            transform.position = targetPos;
            
            // Spawn impact effect
            SpawnImpactEffect();
            
            // Invoke callback
            onImpact?.Invoke();
            
            // Destroy projectile
            Destroy(gameObject, 0.1f);
            return;
        }
        
        // Arc movement
        Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        currentPos.y += arc;
        
        transform.position = currentPos;
        
        // Rotate to face direction of travel
        if (t > 0.01f)
        {
            Vector3 prevPos = Vector3.Lerp(startPos, targetPos, t - 0.01f);
            prevPos.y += Mathf.Sin((t - 0.01f) * Mathf.PI) * arcHeight;
            Vector3 dir = (currentPos - prevPos).normalized;
            if (dir.magnitude > 0.01f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
    
    private void SpawnImpactEffect()
    {
        var impactObj = new GameObject("Impact");
        impactObj.transform.position = targetPos;
        
        var impactRenderer = impactObj.AddComponent<SpriteRenderer>();
        impactRenderer.sprite = CreateImpactSprite();
        impactRenderer.color = spriteRenderer.color;
        impactRenderer.sortingOrder = 21;
        
        var impactAnim = impactObj.AddComponent<ImpactAnimation>();
        impactAnim.Initialize(0.3f);
    }
    
    private Sprite CreateProjectileSprite(MaskType maskType)
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        float cx = size / 2f;
        float cy = size / 2f;
        
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, Color.clear);
        
        switch (maskType)
        {
            case MaskType.Lightning:
                // Lightning bolt
                DrawLightningBolt(tex, size);
                break;
            case MaskType.Flood:
                // Water drop
                DrawWaterDrop(tex, size);
                break;
            case MaskType.Strike:
                // Fist/impact
                DrawFist(tex, size);
                break;
            default:
                // Generic projectile (diamond)
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        float dx = Mathf.Abs(x - cx);
                        float dy = Mathf.Abs(y - cy);
                        if (dx + dy < size / 2.5f)
                            tex.SetPixel(x, y, Color.white);
                    }
                }
                break;
        }
        
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private void DrawLightningBolt(Texture2D tex, int size)
    {
        float cx = size / 2f;
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            float offsetX = Mathf.Sin(t * Mathf.PI * 2.5f) * size * 0.2f;
            for (int x = 0; x < size; x++)
            {
                if (Mathf.Abs(x - cx - offsetX) < size * 0.15f)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private void DrawWaterDrop(Texture2D tex, int size)
    {
        float cx = size / 2f;
        float cy = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Teardrop shape
                float dx = (x - cx) / (size * 0.3f);
                float dy = (y - cy) / (size * 0.4f);
                float distSq = dx * dx + dy * dy * (1 + dy * 0.3f);
                if (distSq < 1f)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private void DrawFist(Texture2D tex, int size)
    {
        float cx = size / 2f;
        float cy = size / 2f;
        float radius = size * 0.35f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist < radius)
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }
    
    private Sprite CreateImpactSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        float cx = size / 2f;
        float cy = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float radius = size / 2f;
                float innerRadius = radius * 0.5f;
                
                if (dist < radius && dist > innerRadius)
                    tex.SetPixel(x, y, Color.white);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    private Color GetProjectileColor(MaskType maskType)
    {
        return maskType switch
        {
            MaskType.Strike => new Color(1f, 0.4f, 0.2f),
            MaskType.Lightning => new Color(1f, 1f, 0.3f),
            MaskType.Flood => new Color(0.3f, 0.5f, 1f),
            MaskType.Smiting => new Color(1f, 0.3f, 0.3f),
            MaskType.Wrath => new Color(1f, 0.2f, 0.5f),
            MaskType.Whispers => new Color(0.5f, 0.3f, 0.8f),
            _ => Color.white
        };
    }
    
    private float GetProjectileSize(MaskType maskType)
    {
        return maskType switch
        {
            MaskType.Lightning => 0.5f,
            MaskType.Flood => 0.6f,
            _ => 0.4f
        };
    }
    
    /// <summary>
    /// Static helper to spawn a projectile.
    /// </summary>
    public static MaskProjectile Create(Vector3 start, Vector3 target, MaskType maskType, Action onImpact)
    {
        var obj = new GameObject($"Projectile_{maskType}");
        var projectile = obj.AddComponent<MaskProjectile>();
        projectile.Launch(start, target, maskType, onImpact);
        return projectile;
    }
}

/// <summary>
/// Simple impact animation that expands and fades.
/// </summary>
public class ImpactAnimation : MonoBehaviour
{
    private float duration;
    private float elapsed;
    private SpriteRenderer spriteRenderer;
    private Vector3 startScale;
    
    public void Initialize(float dur)
    {
        duration = dur;
        elapsed = 0f;
        spriteRenderer = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }
    
    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        
        // Expand
        transform.localScale = startScale * (1f + t * 2f);
        
        // Fade
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f - t;
            spriteRenderer.color = c;
        }
    }
}
