using UnityEngine;

/// <summary>
/// Briefly jiggles the transform's local position when Trigger() is called.
/// Decays linearly over the duration and snaps back to the captured original
/// position when finished. Uses unscaled time so it still fires while the game
/// is paused.
///
/// Attach to any GameObject and call Trigger() to play the effect.
/// </summary>
public class ShakeEffect : MonoBehaviour
{
    [Tooltip("Default shake amplitude in world units.")]
    [SerializeField] private float defaultAmplitude = 0.18f;
    [Tooltip("Default shake duration in seconds.")]
    [SerializeField] private float defaultDuration = 0.18f;

    private Vector3 origin;
    private bool originCaptured;
    private bool shaking;
    private float startTime;
    private float duration;
    private float amplitude;

    private void Awake()
    {
        CaptureOrigin();
    }

    private void OnEnable()
    {
        // Re-capture in case parent moved while disabled.
        if (!shaking) CaptureOrigin();
    }

    private void CaptureOrigin()
    {
        origin = transform.localPosition;
        originCaptured = true;
    }

    public void Trigger() => Trigger(defaultAmplitude, defaultDuration);

    public void Trigger(float amplitudeOverride, float durationOverride)
    {
        if (!originCaptured) CaptureOrigin();
        // If a previous shake is still running, restart from the captured origin
        // rather than the offset position (otherwise drift accumulates).
        if (shaking) transform.localPosition = origin;

        amplitude = Mathf.Max(0f, amplitudeOverride);
        duration = Mathf.Max(0.01f, durationOverride);
        startTime = Time.unscaledTime;
        shaking = true;
    }

    private void Update()
    {
        if (!shaking) return;

        float t = (Time.unscaledTime - startTime) / duration;
        if (t >= 1f)
        {
            transform.localPosition = origin;
            shaking = false;
            return;
        }

        // Linear decay over duration so the shake calms down.
        float decay = 1f - t;
        Vector2 offset = Random.insideUnitCircle * (amplitude * decay);
        transform.localPosition = origin + new Vector3(offset.x, offset.y, 0f);
    }
}
