using UnityEngine;

public class WorldRotate : MonoBehaviour
{
    public Transform headset;
    public float distance = 2f;
    private void LateUpdate()
    {
        Vector3 forward = headset.forward;
        forward.y = 0;
        forward.Normalize();
        transform.position = headset.position + forward * distance;
        transform.LookAt(headset.position);
        transform.Rotate(0, 180, 0);
    }
}
