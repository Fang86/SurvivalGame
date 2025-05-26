using UnityEngine;
using UnityEngine.UI;

public class CrosshairGenerator : MonoBehaviour
{
    [Header("Crosshair Style")]
    public CrosshairType crosshairType = CrosshairType.Plus;
    
    [Header("Size Settings")]
    public int textureSize = 64;
    public int lineThickness = 2;
    public int lineLength = 20;
    public int centerGap = 4;
    
    [Header("Colors")]
    public Color crosshairColor = Color.white;
    public Color outlineColor = Color.black;
    public bool useOutline = true;
    public int outlineThickness = 1;
    
    [Header("Dot Settings")]
    public bool showCenterDot = false;
    public int dotSize = 2;
    public Color dotColor = Color.red;
    
    public enum CrosshairType
    {
        Plus,
        Cross,
        Circle,
        Square,
        T_Shape,
        Brackets,
        Dot
    }
    
    void Start()
    {
        CreateCrosshair();
    }
    
    public void CreateCrosshair()
    {
        // Create or find canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        
        // Remove existing crosshair
        Transform existingCrosshair = canvas.transform.Find("Generated_Crosshair");
        if (existingCrosshair != null)
        {
            DestroyImmediate(existingCrosshair.gameObject);
        }
        
        // Create crosshair GameObject
        GameObject crosshairObj = new GameObject("Generated_Crosshair");
        crosshairObj.transform.SetParent(canvas.transform);
        
        Image crosshairImage = crosshairObj.AddComponent<Image>();
        
        // Position in center of screen
        RectTransform rect = crosshairObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(textureSize, textureSize);
        
        // Generate and apply the crosshair texture
        Texture2D crosshairTexture = GenerateCrosshairTexture();
        Sprite crosshairSprite = Sprite.Create(crosshairTexture, 
            new Rect(0, 0, textureSize, textureSize), 
            new Vector2(0.5f, 0.5f));
        
        crosshairImage.sprite = crosshairSprite;
    }
    
    Texture2D GenerateCrosshairTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];
        
        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        int center = textureSize / 2;
        
        switch (crosshairType)
        {
            case CrosshairType.Plus:
                DrawPlus(pixels, center);
                break;
            case CrosshairType.Cross:
                DrawCross(pixels, center);
                break;
            case CrosshairType.Circle:
                DrawCircle(pixels, center);
                break;
            case CrosshairType.Square:
                DrawSquare(pixels, center);
                break;
            case CrosshairType.T_Shape:
                DrawTShape(pixels, center);
                break;
            case CrosshairType.Brackets:
                DrawBrackets(pixels, center);
                break;
            case CrosshairType.Dot:
                DrawDot(pixels, center, lineLength);
                break;
        }
        
        // Add center dot if enabled
        if (showCenterDot)
        {
            DrawDot(pixels, center, dotSize, dotColor);
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    void DrawPlus(Color[] pixels, int center)
    {
        // Horizontal line
        for (int x = center - lineLength; x <= center + lineLength; x++)
        {
            if (x >= centerGap + center || x <= center - centerGap)
            {
                DrawThickLine(pixels, x, center, true, lineThickness);
            }
        }
        
        // Vertical line
        for (int y = center - lineLength; y <= center + lineLength; y++)
        {
            if (y >= centerGap + center || y <= center - centerGap)
            {
                DrawThickLine(pixels, center, y, false, lineThickness);
            }
        }
    }
    
    void DrawCross(Color[] pixels, int center)
    {
        // Draw diagonal lines
        for (int i = -lineLength; i <= lineLength; i++)
        {
            if (Mathf.Abs(i) >= centerGap)
            {
                // Top-left to bottom-right diagonal
                int x1 = center + i;
                int y1 = center + i;
                if (IsValidPixel(x1, y1))
                {
                    DrawThickPixel(pixels, x1, y1, lineThickness);
                }
                
                // Top-right to bottom-left diagonal
                int x2 = center + i;
                int y2 = center - i;
                if (IsValidPixel(x2, y2))
                {
                    DrawThickPixel(pixels, x2, y2, lineThickness);
                }
            }
        }
    }
    
    void DrawCircle(Color[] pixels, int center)
    {
        int radius = lineLength;
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (Mathf.Abs(distance - radius) <= lineThickness / 2f)
                {
                    SetPixelWithOutline(pixels, x, y);
                }
            }
        }
    }
    
    void DrawSquare(Color[] pixels, int center)
    {
        int halfSize = lineLength;
        
        // Top and bottom lines
        for (int x = center - halfSize; x <= center + halfSize; x++)
        {
            DrawThickLine(pixels, x, center - halfSize, true, lineThickness);
            DrawThickLine(pixels, x, center + halfSize, true, lineThickness);
        }
        
        // Left and right lines
        for (int y = center - halfSize; y <= center + halfSize; y++)
        {
            DrawThickLine(pixels, center - halfSize, y, false, lineThickness);
            DrawThickLine(pixels, center + halfSize, y, false, lineThickness);
        }
    }
    
    void DrawTShape(Color[] pixels, int center)
    {
        // Horizontal line (top of T)
        for (int x = center - lineLength; x <= center + lineLength; x++)
        {
            DrawThickLine(pixels, x, center + lineLength/2, true, lineThickness);
        }
        
        // Vertical line (stem of T)
        for (int y = center - lineLength; y <= center + lineLength/2; y++)
        {
            DrawThickLine(pixels, center, y, false, lineThickness);
        }
    }
    
    void DrawBrackets(Color[] pixels, int center)
    {
        int bracketSize = lineLength;
        int bracketThick = lineLength / 3;
        
        // Left bracket
        for (int y = center - bracketSize; y <= center + bracketSize; y++)
        {
            DrawThickLine(pixels, center - bracketSize, y, false, lineThickness);
            if (y <= center - bracketSize + bracketThick || y >= center + bracketSize - bracketThick)
            {
                for (int x = center - bracketSize; x <= center - bracketSize + bracketThick; x++)
                {
                    DrawThickLine(pixels, x, y, true, lineThickness);
                }
            }
        }
        
        // Right bracket
        for (int y = center - bracketSize; y <= center + bracketSize; y++)
        {
            DrawThickLine(pixels, center + bracketSize, y, false, lineThickness);
            if (y <= center - bracketSize + bracketThick || y >= center + bracketSize - bracketThick)
            {
                for (int x = center + bracketSize - bracketThick; x <= center + bracketSize; x++)
                {
                    DrawThickLine(pixels, x, y, true, lineThickness);
                }
            }
        }
    }
    
    void DrawDot(Color[] pixels, int center, int size, Color color = default)
    {
        if (color == default) color = crosshairColor;
        
        for (int x = center - size; x <= center + size; x++)
        {
            for (int y = center - size; y <= center + size; y++)
            {
                if (Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) <= size)
                {
                    if (IsValidPixel(x, y))
                    {
                        int index = y * textureSize + x;
                        pixels[index] = color;
                    }
                }
            }
        }
    }
    
    void DrawThickLine(Color[] pixels, int x, int y, bool horizontal, int thickness)
    {
        for (int t = 0; t < thickness; t++)
        {
            int offsetX = horizontal ? 0 : t - thickness/2;
            int offsetY = horizontal ? t - thickness/2 : 0;
            
            int pixelX = x + offsetX;
            int pixelY = y + offsetY;
            
            if (IsValidPixel(pixelX, pixelY))
            {
                SetPixelWithOutline(pixels, pixelX, pixelY);
            }
        }
    }
    
    void DrawThickPixel(Color[] pixels, int x, int y, int thickness)
    {
        for (int ox = -thickness/2; ox <= thickness/2; ox++)
        {
            for (int oy = -thickness/2; oy <= thickness/2; oy++)
            {
                int px = x + ox;
                int py = y + oy;
                if (IsValidPixel(px, py))
                {
                    SetPixelWithOutline(pixels, px, py);
                }
            }
        }
    }
    
    void SetPixelWithOutline(Color[] pixels, int x, int y)
    {
        if (!IsValidPixel(x, y)) return;
        
        int index = y * textureSize + x;
        
        if (useOutline)
        {
            // Draw outline first
            for (int ox = -outlineThickness; ox <= outlineThickness; ox++)
            {
                for (int oy = -outlineThickness; oy <= outlineThickness; oy++)
                {
                    int outlineX = x + ox;
                    int outlineY = y + oy;
                    if (IsValidPixel(outlineX, outlineY))
                    {
                        int outlineIndex = outlineY * textureSize + outlineX;
                        if (pixels[outlineIndex].a < outlineColor.a)
                        {
                            pixels[outlineIndex] = outlineColor;
                        }
                    }
                }
            }
        }
        
        // Draw main pixel on top
        pixels[index] = crosshairColor;
    }
    
    bool IsValidPixel(int x, int y)
    {
        return x >= 0 && x < textureSize && y >= 0 && y < textureSize;
    }
    
    // Call this to regenerate crosshair with new settings
    [ContextMenu("Regenerate Crosshair")]
    public void RegenerateCrosshair()
    {
        CreateCrosshair();
    }
}