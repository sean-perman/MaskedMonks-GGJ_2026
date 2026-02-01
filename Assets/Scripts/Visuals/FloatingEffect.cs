using UnityEngine;

/// <summary>
/// Makes an object float up and down with a gentle bobbing motion.
/// Attach to the god sprite object.
/// </summary>
public class FloatingEffect : MonoBehaviour
{
    [Header("Float Settings")]
    [Tooltip("How far up and down the object floats")]
    [SerializeField] private float floatHeight = 0.15f;

    [Tooltip("How fast the object bobs up and down")]
    [SerializeField] private float floatSpeed = 1.5f;

    [Header("Optional Rotation")]
    [Tooltip("Enable slight rotation wobble")]
    [SerializeField] private bool enableRotation = false;

    [Tooltip("Maximum rotation angle in degrees")]
    [SerializeField] private float rotationAmount = 3f;

    [Tooltip("Rotation speed multiplier")]
    [SerializeField] private float rotationSpeed = 1f;

    // Starting position and rotation
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    // Random offset so multiple objects don't sync up
    private float timeOffset;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;

        // Random offset so gods don't bob in perfect sync
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float time = Time.time + timeOffset;

        // Calculate vertical offset using sine wave
        float yOffset = Mathf.Sin(time * floatSpeed) * floatHeight;

        // Apply position
        transform.localPosition = startLocalPosition + new Vector3(0f, yOffset, 0f);

        // Optional rotation wobble
        if (enableRotation)
        {
            float zRotation = Mathf.Sin(time * rotationSpeed) * rotationAmount;
            transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, zRotation);
        }
    }

    /// <summary>
    /// Reset to starting position (useful if object is moved).
    /// </summary>
    public void ResetStartPosition()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }
}
