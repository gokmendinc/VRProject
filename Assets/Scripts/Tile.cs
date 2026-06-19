using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isGoTile;
    public float speed;
    private Transform target;

    [HideInInspector] public float targetHitTime;
    [HideInInspector] public bool isSafeZone;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }
    public void Initialize(float tileSpeed, bool goState, float expectedHitTime, Transform platformTransform, Transform safeZoneTransform)
    {
        speed = tileSpeed;
        isGoTile = goState;
        targetHitTime = expectedHitTime;
        isSafeZone = false;

        transform.rotation = platformTransform.rotation;
        target = safeZoneTransform;

        if (rend != null)
        {
            rend.material.color = isGoTile ? Color.green : Color.red;
        }
    }
    private void Update()
    {
        if (target == null)
        
            return;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        if(Vector3.Distance(transform.position, target.position)<0.1f)
        {
            gameObject.SetActive(false);
        }
    }
}
