using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RhythmDataManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject tilePrefab;
    public Transform spawnPoint;
    public Transform safeZoneCenter;
    public Transform platform;

    [Header("Gameplay Settings")]
    public float tileSpeed = 5.0f;
    public float spawnInterval = 2.0f;

    private List<GameObject> tilePool = new List<GameObject> ();
    private List<Tile> activeTiles = new List<Tile> ();
    private float spawnTimer;
    private string csvFilePath;

    private void Start()
    {
        csvFilePath = Path.Combine(Application.persistentDataPath, "ADHD_Task_Data.csv");
        if (!File.Exists(csvFilePath))
        {
            File.WriteAllText(csvFilePath, "Timestamp,TileType,InputPressed,ResponseTime-ms,Result\n");
        }

        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(tilePrefab);
            obj.SetActive(false);
            tilePool.Add(obj);
        }
    }
    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if(spawnTimer >= spawnInterval)
        {
            SpawnNewTile();
            spawnTimer = 0f;
        }
        if (Input.GetKeyDown(KeyCode.Space)){
            HandlePlayerInput();
        }
    }

    private void HandlePlayerInput()
    {
        activeTiles.RemoveAll(item => item==null || !item.gameObject.activeInHierarchy);
        Tile targetTile = null;

        foreach(Tile tile in activeTiles)
        {
            if (tile.isSafeZone)
            {
                targetTile = tile;
                break;
            }
        }
        if (targetTile != null)
        {
            Debug.Log("There is tile in zone!");
            // Calculate Response Time (Current Time minus the exact moment it hit the center line)
            // Value is in seconds, multiply by 1000 to get miliseconds (ms)
            float responseTimeMs = (Time.time - targetTile.targetHitTime) * 1000f;
            string result = "";
            if (targetTile.isGoTile)
            {
                result = "Correct Hit (Comission)";
                Debug.Log($"<color=green>Go Hit!</color> Response Time: {responseTimeMs:F2} ms");   
            }
            else
            {
                result = "False Alarm (Inhibition Error)";
                Debug.Log($"<color=red>NO-GO Error!</color> Child pressed on a Bomb! Penalty: {responseTimeMs:F2} ms");
            }
            LogData(targetTile.isGoTile ? "Go" : "No-Go", "True", responseTimeMs, result);

            targetTile.gameObject.SetActive(false);
            activeTiles.Remove(targetTile);
        }
        else
        {
            Debug.Log("<color=yellow>Missed Press!</color> Pressed when no tile was close.");
            LogData("None", "True", -1f, "Spam Press Error");
        }
    }

    private void LogData(string tileType, string inputPressed, float responseTimeMs, string result)
    {
        string timeStamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string row = $"{timeStamp}, {tileType}, {inputPressed},{responseTimeMs:F2}, {result}\n";

        File.AppendAllText(csvFilePath, row);
    }

    void SpawnNewTile()
    {
        foreach (GameObject obj in tilePool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.SetParent(platform);
                obj.transform.position = spawnPoint.position;
                obj.SetActive(true);

                Tile tileScript = obj.GetComponent<Tile>();

                // %75 Go tiles, %25 No-Go tiles
                bool isGo = Random.value > 0.25f;

                // Calculate the exact time tile should reach the center line
                float distance = Vector3.Distance(spawnPoint.position, safeZoneCenter.position);
                float expectedHitTime = Time.time + (distance / tileSpeed);

                tileScript.Initialize(tileSpeed, isGo, expectedHitTime, platform);
                activeTiles.Add(tileScript);
                break;
            }
        }
    }
}
