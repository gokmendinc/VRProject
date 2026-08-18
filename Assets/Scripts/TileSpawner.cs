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
    [SerializeField] private float minHeightOffset;
    [SerializeField] private float maxHeightOffset;

    [Header("Viewport Spawn Offsets")]
    [Tooltip("Min/Max Screen Bounds(0 to 1)")]
    [Header("Corner Spawn Settings")]
    [SerializeField] private float minHorizontalDistance = 14f;
    [SerializeField] private float maxHorizontalDistance = 20f;

    [SerializeField] private float minVerticalDistance = 8f;
    [SerializeField] private float maxVerticalDistance = 13f;


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
        
        Vector3 spawnPosition = GetRandomSpawnPosition();

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

    private Vector3 GetRandomSpawnPosition()
    {
        // 0 = Top Left
        // 1 = Top Right
        // 2 = Bottom Left
        // 3 = Bottom Right
        int spawnCorner = Random.Range(0, 4);

        Vector3 portalPosition = portalTransform.position;

        Vector3 right = camera.transform.right;
        Vector3 up = camera.transform.up;

        float horizontalDistance = Random.Range(
            minHorizontalDistance,
            maxHorizontalDistance
        );

        float verticalDistance = Random.Range(
            minVerticalDistance,
            maxVerticalDistance
        );

        Vector3 spawnPosition = portalPosition - new Vector3(0,0,1);

        switch (spawnCorner)
        {
            case 0: // TOP LEFT
                spawnPosition += -right * horizontalDistance;
                spawnPosition += up * verticalDistance;
                break;

            case 1: // TOP RIGHT
                spawnPosition += right * horizontalDistance;
                spawnPosition += up * verticalDistance;
                break;

            case 2: // BOTTOM LEFT
                spawnPosition += -right * horizontalDistance;
                spawnPosition += -up * verticalDistance;
                break;

            case 3: // BOTTOM RIGHT
                spawnPosition += right * horizontalDistance;
                spawnPosition += -up * verticalDistance;
                break;
        }

        return spawnPosition;
    }
    
}
