using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Size")]
    public int terrainWidth = 512;
    public int terrainHeight = 512;
    public int maxTerrainHeight = 30; // Max height in world units
    
    [Header("Generation Settings")]
    public int seed = 12345;
    
    [Header("Hill Settings")]
    [Range(0.5f, 5f)]
    public float hillScale = 2f; // Lower = larger hills
    [Range(0f, 1f)]
    public float hillHeight = 0.6f; // How tall hills can be (0-1)
    
    [Header("Large Hill Features")]
    [Range(0, 10)]
    public int numberOfLargeHills = 3; // How many large hills to generate
    [Range(0f, 1f)]
    public float largeHillHeight = 0.8f; // How tall the large hills are
    [Range(0.1f, 0.5f)]
    public float largeHillMinSize = 0.15f; // Minimum hill size
    [Range(0.2f, 0.8f)]
    public float largeHillMaxSize = 0.4f; // Maximum hill size
    [Range(0.2f, 1f)]
    public float mountainRoughness = 0.5f; // How jagged/natural the mountains look
    [Range(0f, 1f)]
    public float flatAreaThreshold = 0.3f; // Areas below this height stay flatter
    [Header("Detail Settings")]
    [Range(0f, 0.2f)]
    public float detailAmount = 0.1f; // Small surface variations
    [Range(5f, 20f)]
    public float detailScale = 10f; // Higher = smaller detail
    
    private float offsetX;
    private float offsetY;
    
    // Large hill data
    private Vector3[] largeHills; // x, y, size for each hill

    void Start()
    {
        GenerateOffsets();
        GenerateTerrain();
    }
    
    void GenerateOffsets()
    {
        Random.State originalState = Random.state;
        Random.InitState(seed);
        
        offsetX = Random.Range(0f, 10000f);
        offsetY = Random.Range(0f, 10000f);
        
        // Generate large hill positions and sizes
        GenerateLargeHills();
        
        Random.state = originalState;
    }
    
    void GenerateLargeHills()
    {
        largeHills = new Vector3[numberOfLargeHills];
        
        for (int i = 0; i < numberOfLargeHills; i++)
        {
            // Random position (with some border margin)
            float x = Random.Range(0.15f, 0.85f);
            float y = Random.Range(0.15f, 0.85f);
            
            // Random size within specified range
            float size = Random.Range(largeHillMinSize, largeHillMaxSize);
            
            largeHills[i] = new Vector3(x, y, size);
        }
    }

    void GenerateTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;
        
        // Set terrain size
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, maxTerrainHeight, terrainHeight);
        
        // Generate and apply heights
        float[,] heights = GenerateHeights();
        terrainData.SetHeights(0, 0, heights);
        
        Debug.Log($"Generated {terrainWidth}x{terrainHeight} terrain with max height {maxTerrainHeight}");
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[terrainWidth, terrainHeight];
        
        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < terrainHeight; y++)
            {
                // Normalize coordinates (0 to 1)
                float xCoord = (float)x / terrainWidth;
                float yCoord = (float)y / terrainHeight;
                
                // Generate height
                heights[x, y] = CalculateHeight(xCoord, yCoord);
            }
        }

        return heights;
    }

    float CalculateHeight(float x, float y)
    {
        // Main rolling hills
        float hillNoise = Mathf.PerlinNoise(
            x * hillScale + offsetX, 
            y * hillScale + offsetY
        );
        
        // Scale by hill height setting
        float mainHeight = hillNoise * hillHeight;
        
        // Add large hill features
        float largeHillContribution = CalculateLargeHills(x, y);
        mainHeight += largeHillContribution;
        
        // Add fine detail if desired
        if (detailAmount > 0)
        {
            float detailNoise = Mathf.PerlinNoise(
                x * detailScale + offsetX + 1000f, 
                y * detailScale + offsetY + 1000f
            );
            
            mainHeight += (detailNoise - 0.5f) * detailAmount;
        }
        
        // Ensure height stays in valid range
        return Mathf.Clamp01(mainHeight);
    }
    
    float CalculateLargeHills(float x, float y)
    {
        float totalContribution = 0f;
        
        // Add contribution from each large hill
        for (int i = 0; i < largeHills.Length; i++)
        {
            Vector3 hill = largeHills[i];
            float hillX = hill.x;
            float hillY = hill.y;
            float hillSize = hill.z;
            
            // Calculate distance from this hill center
            float distanceX = x - hillX;
            float distanceY = y - hillY;
            float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);
            
            // Add contribution if within hill radius
            if (distance < hillSize)
            {
                // Basic smooth falloff
                float falloffFactor = 1f - (distance / hillSize);
                falloffFactor = falloffFactor * falloffFactor; // Square for smoother falloff
                
                float baseContribution = falloffFactor * largeHillHeight;
                
                // Only add roughness to higher areas (not flat areas)
                if (baseContribution > flatAreaThreshold)
                {
                    // Use gentler noise for natural variation
                    float noiseScale = 4f / hillSize; // Less aggressive noise scale
                    float mountainNoise = Mathf.PerlinNoise(
                        x * noiseScale + offsetX + (i * 1000f),
                        y * noiseScale + offsetY + (i * 1000f)
                    );
                    
                    // Don't use ridged noise - just gentle variation
                    float noiseContribution = (mountainNoise - 0.5f) * mountainRoughness;
                    
                    // Only apply noise to the "mountain" part above flat threshold
                    float mountainPart = baseContribution - flatAreaThreshold;
                    float flatPart = Mathf.Min(baseContribution, flatAreaThreshold);
                    
                    baseContribution = flatPart + mountainPart * (1f + noiseContribution);
                }
                
                // Use maximum (don't stack unrealistically)
                totalContribution = Mathf.Max(totalContribution, baseContribution);
            }
        }
        
        return totalContribution;
    }
    
    [ContextMenu("Regenerate Terrain")]
    void RegenerateTerrain()
    {
        GenerateOffsets();
        GenerateTerrain();
    }
    
    void OnValidate()
    {
        // Clamp values to reasonable ranges
        terrainWidth = Mathf.Clamp(terrainWidth, 32, 2048);
        terrainHeight = Mathf.Clamp(terrainHeight, 32, 2048);
        maxTerrainHeight = Mathf.Max(maxTerrainHeight, 1);
    }
}