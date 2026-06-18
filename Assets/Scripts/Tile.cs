using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isGoTile;
    public float speed;

    [HideInInspector] public float targetHitTime;
    [HideInInspector] public bool isSafeZone;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }
    public void Initialize(float tileSpeed, bool goState, float expectedHitTime, Transform platformTransform)
    {
        speed = tileSpeed;
        isGoTile = goState;
        targetHitTime = expectedHitTime;
        isSafeZone = false;

        transform.rotation = platformTransform.rotation;

        if (rend != null)
        {
            rend.material.color = isGoTile ? Color.green : Color.red;
        }
    }
    private void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.Self);
        if(transform.position.z < -3.0f)
        {
            gameObject.SetActive(false);
        }
    }
}
