using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isGoTile;
    public float speed;
    private Transform target;

    [HideInInspector] public float targetHitTime;
    [HideInInspector] public bool isSafeZone;

    private Renderer rend;
    private TrailRenderer trailRenderer;
    [SerializeField] private float responseRadius = 0.1f;

    private bool reachedPortal = false;

    private void Awake()
    {
        rend = GetComponentInChildren<SkinnedMeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }
    public void Initialize(float tileSpeed, bool goState, float expectedHitTime, Transform platformTransform, Transform safeZoneTransform)
    {
        speed = tileSpeed;
        isGoTile = goState;
        targetHitTime = expectedHitTime;
        isSafeZone = false;

        FaceTarget();
        target = safeZoneTransform;

        if (rend != null)
        {
            rend.material.color = isGoTile ? Color.green : Color.red;
            trailRenderer.material.SetColor("_EmissionColor", isGoTile ? Color.green : Color.red);
        }
    }
    public void FaceTarget()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation * Quaternion.Euler(0f,90f,0f);
        }
    }
    private void Update()
    {
        if (target == null)
            return;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        FaceTarget();
        if(Vector3.Distance(transform.position, target.position)<=responseRadius)
        {
            reachedPortal = true;
            if (ReactionLogger.Instance != null)
            {
                ReactionLogger.Instance.OnTileReachedPortal(this);
            }   
        }
    }
}
