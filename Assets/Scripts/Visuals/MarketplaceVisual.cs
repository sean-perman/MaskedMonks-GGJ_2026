using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Renders the marketplace as a vertical column of green dots in the centre
/// of the screen. Each dot represents one citizen slot - filled (bright green)
/// when occupied and dim when empty. Updates automatically based on
/// Marketplace.Instance.CitizenCount.
/// </summary>
[RequireComponent(typeof(Marketplace))]
public class MarketplaceVisual : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("Number of dots in the column. Should match Marketplace.maxCapacity for a clean visual.")]
    [SerializeField] private int dotCount = 10;
    [Tooltip("World-space size of each dot.")]
    [SerializeField] private float dotSize = 0.55f;
    [Tooltip("Vertical spacing between dot centres in world units.")]
    [SerializeField] private float dotSpacing = 0.9f;

    [Header("Colors")]
    [SerializeField] private Color filledColor = new Color(0.35f, 0.95f, 0.4f);
    [SerializeField] private Color emptyColor = new Color(0.15f, 0.35f, 0.2f, 0.55f);

    private Marketplace marketplace;
    private ShakeEffect shake;
    private readonly List<SpriteRenderer> dots = new();
    private int lastDrawnCount = -1;

    private void Awake()
    {
        marketplace = GetComponent<Marketplace>();
        shake = GetComponent<ShakeEffect>();
        if (shake == null) shake = gameObject.AddComponent<ShakeEffect>();
    }

    private void Start()
    {
        BuildDots();
        Refresh(force: true);
    }

    private void Update()
    {
        if (marketplace == null) return;
        int count = marketplace.CitizenCount;
        if (count != lastDrawnCount) Refresh();
    }

    private void BuildDots()
    {
        // Center the column on this transform so positioning the marketplace
        // GameObject at (0, 0, 0) drops the column into the screen centre.
        float totalHeight = (dotCount - 1) * dotSpacing;
        float startY = totalHeight / 2f;

        var dotSprite = CreateCircleSprite();

        for (int i = 0; i < dotCount; i++)
        {
            var dotObj = new GameObject($"MarketplaceDot_{i}");
            dotObj.transform.SetParent(transform);
            // Top of the column = first/topmost slot, so index 0 sits at the top.
            dotObj.transform.localPosition = new Vector3(0f, startY - i * dotSpacing, 0f);
            dotObj.transform.localScale = new Vector3(dotSize, dotSize, 1f);

            var sr = dotObj.AddComponent<SpriteRenderer>();
            sr.sprite = dotSprite;
            sr.sortingOrder = -1;
            dots.Add(sr);
        }
    }

    private void Refresh(bool force = false)
    {
        if (marketplace == null) return;
        int count = marketplace.CitizenCount;
        if (!force && count == lastDrawnCount) return;

        // Shake when a citizen was added (count went up). Skip on the initial
        // population pass (force == true) so we don't shake every dot in.
        if (!force && count > lastDrawnCount && shake != null)
        {
            shake.Trigger();
        }

        // Fill from the bottom up: dots near the bottom of the column fill first.
        // Indexing: dot 0 sits at the top of the column, dot (dotCount - 1) at the
        // bottom. Filled set = the last `count` dots.
        int firstFilled = dots.Count - count;
        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].color = i >= firstFilled ? filledColor : emptyColor;
        }
        lastDrawnCount = count;
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        float r = size / 2f;
        var center = new Vector2(r, r);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                // Soft edge for nicer-looking dots.
                float alpha = Mathf.Clamp01((r - d) / 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
