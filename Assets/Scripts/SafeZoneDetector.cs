using UnityEngine;

public class SafeZoneDetector : MonoBehaviour
{
    private Renderer rendererMat;
    private void Awake()
    {
        rendererMat = GetComponent<Renderer>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile"))
        {
            rendererMat.material.SetColor("_EmissionColor", Color.green);
            Tile tile = other.GetComponent<Tile>();
            if (tile != null) tile.isSafeZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tile"))
        {
            rendererMat.material.SetColor("_EmissionColor", Color.darkRed);
            Tile tile = other.GetComponent<Tile>();
            tile.isSafeZone = false;
        }
    }
}
