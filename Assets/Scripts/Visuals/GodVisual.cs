using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Visual representation of a God showing strength bar, favor pips, money pips, and mask storage.
/// Attach this to God GameObjects for visual feedback.
/// 
/// Favor is shown as purple squares, Money as gold circles.
/// These appear directly under the strength bar.
/// Mask cost pips appear in the top-left of each mask slot.
/// </summary>
[RequireComponent(typeof(God))]
public class GodVisual : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float barWidth = 2f;
    [SerializeField] private float barHeight = 0.25f;
    [SerializeField] private float pipSize = 0.15f;
    [SerializeField] private float pipSpacing = 0.05f;
    [SerializeField] private float maskSlotSize = 0.5f;
    [SerializeField] private float maskSlotSpacing = 0.1f;
    
    [Header("Colors")]
    [SerializeField] private Color strengthBarColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color strengthBgColor = new Color(0.3f, 0.1f, 0.1f);
    [SerializeField] private Color favorColor = new Color(0.6f, 0.3f, 0.8f);       // Purple
    [SerializeField] private Color favorEmptyColor = new Color(0.3f, 0.15f, 0.4f, 0.5f);
    [SerializeField] private Color moneyColor = new Color(1f, 0.85f, 0.3f);        // Gold
    [SerializeField] private Color moneyEmptyColor = new Color(0.5f, 0.4f, 0.15f, 0.5f);
    [SerializeField] private Color maskSlotColor = new Color(0.4f, 0.4f, 0.5f);
    [SerializeField] private Color maskSlotEmptyColor = new Color(0.2f, 0.2f, 0.25f, 0.5f);
    
    [Header("Resource Display")]
    [SerializeField] private int favorPipValue = 1;    // Each pip represents 1 favor
    [SerializeField] private int moneyPipValue = 1;     // Each pip represents 1 money
    [SerializeField] private int maxDisplayPips = 10;   // Max pips to show per resource
    
    private God god;
    private Cult cult;
    
    // Visual elements
    private SpriteRenderer strengthBarBg;
    private SpriteRenderer strengthBarFill;
    private List<SpriteRenderer> favorPips = new();
    private List<SpriteRenderer> moneyPips = new();
    private List<MaskSlotVisual> maskSlots = new();
    
    private void Awake()
    {
        god = GetComponent<God>();
    }
    
    private void Start()
    {
        // Find owning cult
        cult = GetComponentInParent<Cult>();
        CreateVisuals();
    }
    
    private void CreateVisuals()
    {
        CreateStrengthBar();
        CreateResourcePips();
        CreateMaskSlots();
    }
    
    private void CreateStrengthBar()
    {
        // Background
        var bgObj = new GameObject("StrengthBarBg");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        strengthBarBg = bgObj.AddComponent<SpriteRenderer>();
        strengthBarBg.sprite = CreateSquareSprite();
        strengthBarBg.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        strengthBarBg.color = strengthBgColor;
        strengthBarBg.sortingOrder = 10;
        
        // Fill
        var fillObj = new GameObject("StrengthBarFill");
        fillObj.transform.SetParent(bgObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        strengthBarFill = fillObj.AddComponent<SpriteRenderer>();
        strengthBarFill.sprite = CreateSquareSprite();
        strengthBarFill.color = strengthBarColor;
        strengthBarFill.sortingOrder = 11;
    }
    
    private void CreateResourcePips()
    {
        float startY = 0.5f - barHeight / 2 - pipSize - 0.05f;
        
        // Favor pips (purple squares) - top row
        float favorStartX = -barWidth / 2;
        for (int i = 0; i < maxDisplayPips; i++)
        {
            var pipObj = new GameObject($"FavorPip_{i}");
            pipObj.transform.SetParent(transform);
            pipObj.transform.localPosition = new Vector3(
                favorStartX + i * (pipSize + pipSpacing) + pipSize / 2,
                startY,
                0
            );
            
            var pip = pipObj.AddComponent<SpriteRenderer>();
            pip.sprite = CreateSquareSprite();
            pip.transform.localScale = new Vector3(pipSize, pipSize, 1f);
            pip.sortingOrder = 10;
            favorPips.Add(pip);
        }
        
        // Money pips (gold circles) - bottom row
        float moneyStartY = startY - pipSize - pipSpacing;
        for (int i = 0; i < maxDisplayPips; i++)
        {
            var pipObj = new GameObject($"MoneyPip_{i}");
            pipObj.transform.SetParent(transform);
            pipObj.transform.localPosition = new Vector3(
                favorStartX + i * (pipSize + pipSpacing) + pipSize / 2,
                moneyStartY,
                0
            );
            
            var pip = pipObj.AddComponent<SpriteRenderer>();
            pip.sprite = CreateCircleSprite();
            pip.transform.localScale = new Vector3(pipSize, pipSize, 1f);
            pip.sortingOrder = 10;
            moneyPips.Add(pip);
        }
    }
    
    private void CreateMaskSlots()
    {
        float startY = 0.5f - barHeight / 2 - pipSize * 2 - pipSpacing * 3 - maskSlotSize / 2 - 0.2f;
        float startX = -((4 - 1) * (maskSlotSize + maskSlotSpacing)) / 2;
        
        for (int i = 0; i < 4; i++) // Max 4 mask slots
        {
            var slotObj = new GameObject($"MaskSlot_{i}");
            slotObj.transform.SetParent(transform);
            slotObj.transform.localPosition = new Vector3(
                startX + i * (maskSlotSize + maskSlotSpacing),
                startY,
                0
            );
            
            var slotVisual = new MaskSlotVisual();
            slotVisual.Create(slotObj.transform, maskSlotSize, pipSize * 0.5f);
            maskSlots.Add(slotVisual);
        }
    }
    
    private void Update()
    {
        if (god == null) return;
        
        UpdateStrengthBar();
        UpdateFavorPips();
        UpdateMoneyPips();
        UpdateMaskSlots();
    }
    
    private void UpdateStrengthBar()
    {
        float percent = god.MaxStrength > 0 ? (float)god.Strength / god.MaxStrength : 0f;
        strengthBarFill.transform.localScale = new Vector3(percent, 1f, 1f);
        strengthBarFill.transform.localPosition = new Vector3((percent - 1f) / 2f, 0, 0);
    }
    
    private void UpdateFavorPips()
    {
        int filledPips = Mathf.Min(maxDisplayPips, Mathf.CeilToInt((float)god.Favor / favorPipValue));
        
        for (int i = 0; i < favorPips.Count; i++)
        {
            if (i < filledPips)
            {
                // Calculate partial fill for the last pip
                int pipStartValue = i * favorPipValue;
                int pipEndValue = (i + 1) * favorPipValue;
                
                if (god.Favor >= pipEndValue)
                {
                    favorPips[i].color = favorColor;
                }
                else if (god.Favor > pipStartValue)
                {
                    // Partial pip
                    float fillAmount = (float)(god.Favor - pipStartValue) / favorPipValue;
                    favorPips[i].color = Color.Lerp(favorEmptyColor, favorColor, fillAmount);
                }
                else
                {
                    favorPips[i].color = favorEmptyColor;
                }
            }
            else
            {
                favorPips[i].color = favorEmptyColor;
            }
        }
    }
    
    private void UpdateMoneyPips()
    {
        if (cult == null) return;
        
        int maxPips = Mathf.Min(maxDisplayPips, cult.MaxMoney / moneyPipValue);
        int filledPips = Mathf.Min(maxPips, cult.Money / moneyPipValue);
        
        for (int i = 0; i < moneyPips.Count; i++)
        {
            if (i < maxPips)
            {
                // Show pip (either filled or empty based on current money)
                if (i < filledPips)
                {
                    moneyPips[i].color = moneyColor;
                }
                else
                {
                    moneyPips[i].color = moneyEmptyColor;
                }
            }
            else
            {
                // Beyond max capacity - hide pip
                moneyPips[i].color = new Color(0, 0, 0, 0);
            }
        }
    }
    
    private void UpdateMaskSlots()
    {
        var masks = god.StoredMasks;
        
        for (int i = 0; i < maskSlots.Count; i++)
        {
            if (i < masks.Count && masks[i] != null)
            {
                maskSlots[i].SetMask(masks[i], maskSlotColor);
            }
            else
            {
                maskSlots[i].SetEmpty(maskSlotEmptyColor);
            }
        }
    }
    
    // === Sprite Creation Helpers ===
    
    private Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
    
    private Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, dist < radius ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Helper class for managing mask slot visuals with cost pips.
    /// </summary>
    private class MaskSlotVisual
    {
        private SpriteRenderer background;
        private SpriteRenderer maskIcon;
        private List<SpriteRenderer> favorCostPips = new();
        private List<SpriteRenderer> moneyCostPips = new();
        
        private Color favorCostColor = new Color(0.6f, 0.3f, 0.8f);
        private Color moneyCostColor = new Color(1f, 0.85f, 0.3f);
        
        public void Create(Transform parent, float slotSize, float costPipSize)
        {
            // Background
            var bgObj = new GameObject("SlotBg");
            bgObj.transform.SetParent(parent);
            bgObj.transform.localPosition = Vector3.zero;
            background = bgObj.AddComponent<SpriteRenderer>();
            background.sprite = CreateSlotSprite();
            background.transform.localScale = new Vector3(slotSize, slotSize, 1f);
            background.sortingOrder = 10;
            
            // Mask icon (shows mask type color)
            var iconObj = new GameObject("MaskIcon");
            iconObj.transform.SetParent(parent);
            iconObj.transform.localPosition = Vector3.zero;
            maskIcon = iconObj.AddComponent<SpriteRenderer>();
            maskIcon.sprite = CreateMaskSprite();
            maskIcon.transform.localScale = new Vector3(slotSize * 0.7f, slotSize * 0.7f, 1f);
            maskIcon.sortingOrder = 11;
            
            // Cost pips (top-left corner)
            float costStartX = -slotSize / 2 + costPipSize / 2 + 0.02f;
            float costStartY = slotSize / 2 - costPipSize / 2 - 0.02f;
            
            // Favor cost pips (up to 3)
            for (int i = 0; i < 3; i++)
            {
                var pipObj = new GameObject($"FavorCost_{i}");
                pipObj.transform.SetParent(parent);
                pipObj.transform.localPosition = new Vector3(
                    costStartX + i * (costPipSize + 0.02f),
                    costStartY,
                    0
                );
                var pip = pipObj.AddComponent<SpriteRenderer>();
                pip.sprite = CreateSquareSprite();
                pip.transform.localScale = new Vector3(costPipSize, costPipSize, 1f);
                pip.color = favorCostColor;
                pip.sortingOrder = 12;
                pip.enabled = false;
                favorCostPips.Add(pip);
            }
            
            // Money cost pips (below favor, up to 3)
            for (int i = 0; i < 3; i++)
            {
                var pipObj = new GameObject($"MoneyCost_{i}");
                pipObj.transform.SetParent(parent);
                pipObj.transform.localPosition = new Vector3(
                    costStartX + i * (costPipSize + 0.02f),
                    costStartY - costPipSize - 0.02f,
                    0
                );
                var pip = pipObj.AddComponent<SpriteRenderer>();
                pip.sprite = CreateCircleSprite();
                pip.transform.localScale = new Vector3(costPipSize, costPipSize, 1f);
                pip.color = moneyCostColor;
                pip.sortingOrder = 12;
                pip.enabled = false;
                moneyCostPips.Add(pip);
            }
        }
        
        public void SetMask(Mask mask, Color slotColor)
        {
            background.color = slotColor;
            maskIcon.enabled = true;
            maskIcon.color = GetMaskTypeColor(mask.Type);
            
            // Show favor cost pips (each pip = 10 favor)
            int favorPips = Mathf.Min(3, Mathf.CeilToInt(mask.FavorCost / 10f));
            for (int i = 0; i < favorCostPips.Count; i++)
            {
                favorCostPips[i].enabled = i < favorPips;
            }
            
            // Show money cost pips (each pip = 20 money)
            int moneyPipsCount = Mathf.Min(3, Mathf.CeilToInt(mask.MoneyCost / 20f));
            for (int i = 0; i < moneyCostPips.Count; i++)
            {
                moneyCostPips[i].enabled = i < moneyPipsCount;
            }
        }
        
        public void SetEmpty(Color emptyColor)
        {
            background.color = emptyColor;
            maskIcon.enabled = false;
            
            foreach (var pip in favorCostPips) pip.enabled = false;
            foreach (var pip in moneyCostPips) pip.enabled = false;
        }
        
        private Color GetMaskTypeColor(MaskType type)
        {
            return type switch
            {
                MaskType.Smiting => new Color(1f, 0.3f, 0.3f),      // Red
                MaskType.Wrath => new Color(1f, 0.2f, 0.5f),        // Magenta
                MaskType.Whispers => new Color(0.5f, 0.3f, 0.8f),   // Purple
                MaskType.Sanctuary => new Color(0.3f, 0.8f, 0.5f),  // Green
                MaskType.Plenty => new Color(1f, 0.85f, 0.3f),      // Gold
                MaskType.Sacrifice => new Color(0.8f, 0.2f, 0.2f),  // Dark red
                _ => Color.white
            };
        }
        
        private static Sprite CreateSlotSprite()
        {
            int size = 32;
            int borderWidth = 2;
            Texture2D tex = new Texture2D(size, size);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    bool isBorder = x < borderWidth || x >= size - borderWidth ||
                                   y < borderWidth || y >= size - borderWidth;
                    tex.SetPixel(x, y, isBorder ? new Color(0.6f, 0.6f, 0.6f) : Color.white);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
        
        private static Sprite CreateMaskSprite()
        {
            // Simple mask shape (oval with eye holes)
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            float centerX = size / 2f;
            float centerY = size / 2f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // Oval face shape
                    float dx = (x - centerX) / (size * 0.45f);
                    float dy = (y - centerY) / (size * 0.35f);
                    bool inFace = dx * dx + dy * dy < 1f;
                    
                    // Eye holes
                    float eyeY = centerY + 2;
                    float leftEyeX = centerX - 5;
                    float rightEyeX = centerX + 5;
                    float eyeRadius = 3f;
                    bool inLeftEye = Vector2.Distance(new Vector2(x, y), new Vector2(leftEyeX, eyeY)) < eyeRadius;
                    bool inRightEye = Vector2.Distance(new Vector2(x, y), new Vector2(rightEyeX, eyeY)) < eyeRadius;
                    
                    if (inFace && !inLeftEye && !inRightEye)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
        
        private static Sprite CreateSquareSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        
        private static Sprite CreateCircleSprite()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size);
            float radius = size / 2f;
            Vector2 center = new Vector2(radius, radius);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist < radius ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
