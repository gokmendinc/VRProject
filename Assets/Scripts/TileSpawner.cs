using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform portalTransform;
    [SerializeField] private Camera camera;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private float tileSpeed= 5.0f;
    [SerializeField] private float tileAmount= 2.0f;
    [SerializeField] private float spawnDistanceFromCamera=10.0f;

    [Header("Viewport Spawn Offsets")]
    [Tooltip("Min/Max Screen Bounds(0 to 1)")]
    [SerializeField] private Vector2 viewportXBounds = new Vector2(0.1f, 0.9f);
    [SerializeField] private Vector2 viewportYBounds = new Vector2(0.1f, 0.9f);

    [Header("Tile Properties")]
    [SerializeField] [Range(0f,1f)] private float goTileChance = 0.5f;

    private float nextSpawnTime;
    void Awake()
    {
        if (camera == null)
        {
            camera= Camera.main;
        }
    }
    void Update()
    {
        if (Time.time > nextSpawnTime)
        {
            SpawnTile();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnTile()
    {
        if(tilePrefab==null || portalTransform == null || camera==null)
        {
            Debug.LogWarning("TileSpawner: Please assign all the variables in the inspector.");
            return;
        }
        Vector3 spawnPosition = GetRandomSpawnPositionNearCamera();

        Tile newTile = Instantiate(tilePrefab, spawnPosition , Quaternion.identity);
        bool isGoState = Random.value < goTileChance;

        float distanceToPortal = Vector3.Distance(spawnPosition, portalTransform.position);
        float expectedHitTime = Time.time + (distanceToPortal/tileSpeed);

        newTile.Initialize(
            tileSpeed:tileSpeed,
            goState:isGoState,
            expectedHitTime: expectedHitTime,
            platformTransform: portalTransform,
            safeZoneTransform: portalTransform);
    }

    private Vector3 GetRandomSpawnPositionNearCamera()
    {
        float randomX = Random.Range(viewportXBounds.x, viewportXBounds.y);
        float randomY = Random.Range(viewportYBounds.x, viewportYBounds.y);

        Vector3 viewportPoint = new Vector3(randomX, randomY, spawnDistanceFromCamera);
        
        return camera.ViewportToWorldPoint(viewportPoint);
    }
}
