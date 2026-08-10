using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform portalTransform;
    [SerializeField] private new Camera camera;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private float tileSpeed= 5.0f;
    [SerializeField] private float tileAmount= 2.0f;
    [SerializeField] private float spawnDistanceFromCamera=10.0f;
    [SerializeField] private float spawnRadius = 10.0f;
    [SerializeField] private float minHeightOffset;
    [SerializeField] private float maxHeightOffset;

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
        ReactionLogger.Instance.RegisterTileSpawned(newTile, portalTransform);
    }

    private Vector3 GetRandomSpawnPositionNearCamera()
    {
        float viewportX = Random.Range(viewportXBounds.x, viewportXBounds.y);
        float viewportY = Random.Range(viewportYBounds.x, viewportYBounds.y);

        Vector3 viewportPoint = new Vector3(
            viewportX,
            viewportY,
            spawnDistanceFromCamera
        );
        Vector3 spawnPosition = camera.ViewportToWorldPoint(viewportPoint);

        spawnPosition.y += Random.Range(minHeightOffset, maxHeightOffset);
        return spawnPosition;
            
    }
    private void OnDrawGizmosSelected()
    {
        if (camera == null) camera = Camera.main;
        if (camera == null) return;

        Gizmos.color = Color.cyan;
        Vector3 camPos = camera.transform.position;

        // Projected horizontal backward direction
        Vector3 camForward = camera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 backDir = -camForward;

        // Draw the 180-degree arc bounds behind the camera
        Vector3 leftBound = Quaternion.AngleAxis(-90, Vector3.up) * backDir * spawnRadius;
        Vector3 rightBound = Quaternion.AngleAxis(90, Vector3.up) * backDir * spawnRadius;

        Gizmos.DrawRay(camPos, leftBound);
        Gizmos.DrawRay(camPos, rightBound);
        Gizmos.DrawWireSphere(camPos, spawnRadius);
    }
}
