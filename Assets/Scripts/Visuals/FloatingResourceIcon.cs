using UnityEngine;

/// <summary>
/// Floating resource icon that rises and fades when a room generates a resource.
/// Automatically destroys itself after the animation completes.
/// </summary>
public class FloatingResourceIcon : MonoBehaviour
{
    private float floatHeight = 1.5f;
    private float duration = 1.0f;
    private int amount = 1;
    
    private float elapsed = 0f;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private TextMesh amountText;
    
    public void Initialize(float height, float dur, int resourceAmount)
    {
        floatHeight = height;
        duration = dur;
        amount = resourceAmount;
        
        startPosition = transform.localPosition;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Create amount text if more than 1
        if (amount > 1)
        {
            var textObj = new GameObject("AmountText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0.2f, -0.1f, 0);
            amountText = textObj.AddComponent<TextMesh>();
            amountText.text = $"+{amount}";
            amountText.fontSize = 24;
            amountText.characterSize = 0.05f;
            amountText.anchor = TextAnchor.MiddleLeft;
            amountText.alignment = TextAlignment.Left;
            amountText.color = spriteRenderer != null ? spriteRenderer.color : Color.white;
            
            // Add renderer for sorting
            var textRenderer = textObj.GetComponent<MeshRenderer>();
            if (textRenderer != null)
            {
                textRenderer.sortingOrder = 11;
            }
        }
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
        
        // Float upward with ease-out
        float easedT = 1f - Mathf.Pow(1f - t, 2f);
        transform.localPosition = startPosition + Vector3.up * floatHeight * easedT;
        
        // Fade out in the last 30% of the animation
        if (t > 0.7f && spriteRenderer != null)
        {
            float fadeT = (t - 0.7f) / 0.3f;
            Color c = spriteRenderer.color;
            c.a = 1f - fadeT;
            spriteRenderer.color = c;
            
            if (amountText != null)
            {
                Color tc = amountText.color;
                tc.a = 1f - fadeT;
                amountText.color = tc;
            }
        }
        
        // Scale pulse at start
        if (t < 0.2f)
        {
            float scaleT = t / 0.2f;
            float scale = 1f + 0.3f * Mathf.Sin(scaleT * Mathf.PI);
            transform.localScale = Vector3.one * scale * transform.localScale.x / (transform.localScale.x / scale);
        }
    }
}
