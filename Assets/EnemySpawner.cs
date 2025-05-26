using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs; // Array of different enemy types
    public Transform player; // Reference to player
    public int maxEnemies = 20; // Maximum enemies on map at once
    public float spawnInterval = 3f; // Time between spawns
    
    [Header("Distance Settings")]
    public float minDistanceFromPlayer = 15f; // Minimum spawn distance from player
    public float maxDistanceFromPlayer = 50f; // Maximum spawn distance from player
    public float despawnDistance = 80f; // Remove enemies beyond this distance
    
    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(100f, 20f, 100f); // Size of spawn area
    public LayerMask groundLayerMask = 1; // What counts as ground
    public float groundCheckDistance = 10f; // How far to raycast for ground
    
    [Header("Difficulty Scaling")]
    public bool scaleDifficulty = true;
    public float difficultyIncreaseInterval = 60f; // Increase difficulty every 60 seconds
    public float spawnRateIncrease = 0.1f; // How much faster spawning gets
    public int maxEnemiesIncrease = 2; // How many more enemies per difficulty increase
    
    [Header("Debug")]
    public bool showSpawnArea = true;
    public bool debugLogging = false;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float nextSpawnTime;
    private float gameStartTime;
    private int currentDifficultyLevel = 0;
    
    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        gameStartTime = Time.time;
        nextSpawnTime = Time.time + spawnInterval;
        
        // Start spawning coroutine
        StartCoroutine(SpawnEnemies());
        StartCoroutine(CleanupEnemies());
        
        if (scaleDifficulty)
        {
            StartCoroutine(IncreaseDifficulty());
        }
    }
    
    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (activeEnemies.Count < maxEnemies && player != null)
            {
                TrySpawnEnemy();
            }
        }
    }
    
    void TrySpawnEnemy()
    {
        int maxAttempts = 10; // Prevent infinite loops
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // Check distance from player
            float distanceToPlayer = Vector3.Distance(spawnPosition, player.position);
            
            if (distanceToPlayer >= minDistanceFromPlayer && distanceToPlayer <= maxDistanceFromPlayer)
            {
                // Check if position is on ground
                Vector3 groundPosition = FindGroundPosition(spawnPosition);
                
                if (groundPosition != Vector3.zero)
                {
                    SpawnEnemyAt(groundPosition);
                    break;
                }
            }
            
            if (debugLogging)
            {
                Debug.Log($"Spawn attempt {attempt + 1} failed. Distance: {distanceToPlayer:F1}");
            }
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Get random position around player within spawn area
        Vector3 playerPos = player.position;
        
        float randomX = Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
        float randomZ = Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f);
        float randomY = Random.Range(0, spawnAreaSize.y); // Above ground level
        
        return playerPos + new Vector3(randomX, randomY, randomZ);
    }
    
    Vector3 FindGroundPosition(Vector3 startPosition)
    {
        // Raycast downward to find ground
        RaycastHit hit;
        Vector3 rayStart = startPosition + Vector3.up * 2f; // Start slightly above
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayerMask))
        {
            // Check if the slope is reasonable (not too steep)
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle <= 45f) // Reasonable slope
            {
                return hit.point + Vector3.up * 0.1f; // Slightly above ground
            }
        }
        
        return Vector3.zero; // Invalid position
    }
    
    void SpawnEnemyAt(Vector3 position)
    {
        if (enemyPrefabs.Length == 0) return;
        
        // Choose random enemy type
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        // Spawn enemy
        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        // Add to active enemies list
        activeEnemies.Add(newEnemy);
        
        if (debugLogging)
        {
            Debug.Log($"Spawned {enemyPrefab.name} at {position}. Total enemies: {activeEnemies.Count}");
        }
    }
    
    IEnumerator CleanupEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // Check every 2 seconds
            
            CleanupDestroyedEnemies();
            DespawnDistantEnemies();
        }
    }
    
    void CleanupDestroyedEnemies()
    {
        // Remove null references (destroyed enemies)
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
    
    void DespawnDistantEnemies()
    {
        if (player == null) return;
        
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null) continue;
            
            float distance = Vector3.Distance(activeEnemies[i].transform.position, player.position);
            
            if (distance > despawnDistance)
            {
                if (debugLogging)
                {
                    Debug.Log($"Despawning {activeEnemies[i].name} - too far from player ({distance:F1})");
                }
                
                Destroy(activeEnemies[i]);
                activeEnemies.RemoveAt(i);
            }
        }
    }
    
    IEnumerator IncreaseDifficulty()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);
            
            currentDifficultyLevel++;
            
            // Decrease spawn interval (faster spawning)
            spawnInterval = Mathf.Max(0.5f, spawnInterval - spawnRateIncrease);
            
            // Increase max enemies
            maxEnemies += maxEnemiesIncrease;
            
            Debug.Log($"Difficulty increased! Level: {currentDifficultyLevel}, " +
                     $"Spawn Interval: {spawnInterval:F1}s, Max Enemies: {maxEnemies}");
        }
    }
    
    // Public methods for external control
    public void SetSpawnRate(float newInterval)
    {
        spawnInterval = Mathf.Max(0.1f, newInterval);
    }
    
    public void SetMaxEnemies(int newMax)
    {
        maxEnemies = Mathf.Max(1, newMax);
    }
    
    public void SpawnEnemyNow()
    {
        if (activeEnemies.Count < maxEnemies)
        {
            TrySpawnEnemy();
        }
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        activeEnemies.Clear();
    }
    
    public int GetActiveEnemyCount()
    {
        CleanupDestroyedEnemies();
        return activeEnemies.Count;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!showSpawnArea || player == null) return;
        
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(player.position, spawnAreaSize);
        
        // Draw min/max distance circles
        Gizmos.color = Color.red;
        DrawCircle(player.position, minDistanceFromPlayer);
        
        Gizmos.color = Color.green;
        DrawCircle(player.position, maxDistanceFromPlayer);
        
        Gizmos.color = Color.blue;
        DrawCircle(player.position, despawnDistance);
    }
    
    void DrawCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
            
            Gizmos.DrawLine(point1, point2);
        }
    }
    
    // Display info in inspector
    void OnGUI()
    {
        if (!debugLogging) return;
        
        GUILayout.BeginArea(new Rect(10, 100, 300, 150));
        GUILayout.Label($"Active Enemies: {GetActiveEnemyCount()} / {maxEnemies}");
        GUILayout.Label($"Spawn Interval: {spawnInterval:F1}s");
        GUILayout.Label($"Difficulty Level: {currentDifficultyLevel}");
        GUILayout.Label($"Game Time: {(Time.time - gameStartTime):F0}s");
        GUILayout.EndArea();
    }
}