using UnityEngine;

/// <summary>
/// Keeps a SpriteRenderer's transform scaled to fully cover the active camera's
/// visible area. Recomputes scale whenever the screen size, aspect ratio, or
/// camera orthographic size changes, so the background stays full-screen on
/// window resize, fullscreen toggle, or runtime camera tweaks.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    [Tooltip("Camera to fit to. Defaults to Camera.main if null.")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Extra scale margin to avoid 1px edge gaps from floating-point rounding.")]
    [SerializeField] private float margin = 1.05f;

    private SpriteRenderer spriteRenderer;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private float lastOrthoSize = -1f;
    private float lastAspect = -1f;
    private float lastFov = -1f;
    private float lastDistance = -1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        ApplyScale();
    }

    private void LateUpdate()
    {
        var cam = ResolveCamera();
        bool dirty =
            Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight;

        if (cam != null)
        {
            if (!Mathf.Approximately(cam.aspect, lastAspect)) dirty = true;
            if (cam.orthographic)
            {
                if (!Mathf.Approximately(cam.orthographicSize, lastOrthoSize)) dirty = true;
            }
            else
            {
                if (!Mathf.Approximately(cam.fieldOfView, lastFov)) dirty = true;
                float distance = Mathf.Abs(transform.position.z - cam.transform.position.z);
                if (!Mathf.Approximately(distance, lastDistance)) dirty = true;
            }
        }

        if (dirty) ApplyScale();
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null) return targetCamera;
        return Camera.main;
    }

    private void ApplyScale()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        var sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        if (sprite == null) return;

        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        if (spriteWidth <= 0f || spriteHeight <= 0f) return;

        var cam = ResolveCamera();
        float worldHeight, worldWidth;

        if (cam != null && cam.orthographic)
        {
            worldHeight = cam.orthographicSize * 2f;
            worldWidth = worldHeight * cam.aspect;
            lastOrthoSize = cam.orthographicSize;
            lastAspect = cam.aspect;
        }
        else if (cam != null)
        {
            // Perspective: compute the frustum's visible size at the background's
            // z-distance from the camera. Without this, a perspective camera would
            // see far less than the orthographic case suggests.
            float distance = Mathf.Abs(transform.position.z - cam.transform.position.z);
            // Guard against the bg sitting on top of the camera (distance 0 -> infinite scale).
            if (distance < 0.01f) distance = 10f;
            worldHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            worldWidth = worldHeight * cam.aspect;
            lastFov = cam.fieldOfView;
            lastDistance = distance;
            lastAspect = cam.aspect;
        }
        else
        {
            // No camera available - last-resort fallback so we still scale up.
            worldHeight = 25f;
            worldWidth = 45f;
        }

        float scale = Mathf.Max(worldHeight / spriteHeight, worldWidth / spriteWidth) * margin;
        transform.localScale = new Vector3(scale, scale, 1f);
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}
