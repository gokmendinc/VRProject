using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReactionLogger : MonoBehaviour
{
    public static ReactionLogger Instance { get; private set; }

    [Header("Subject Data")]
    [SerializeField] private string participantId = "P001";
    [SerializeField] private string sessionId = "S001";

    [Header("Input Action (VR Controller)")]
    [SerializeField] private InputActionProperty vrInteractionAction;
    private bool responseWindowActive = false;
    private float responseStartTime = -1f;

    [Header("Sound Effects")]
    [SerializeField] AudioClip SuccesClip;
    [SerializeField] AudioClip FailClip;


    [SerializeField] private float responseWindowDuration = 0.3f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Renderer portalColor;
    public struct TrialData
    {
        public int trialID;
        public string participantID;
        public string sessionID;
        public string tileType;
        public float spawnTime;
        public float expectedHitTime;
        public float interactionTime;
        public float reactionTime;
        public string outcome;
        public Vector3 spawnPos;
        public Vector3 portalPos;
    }
    private List<TrialData> trialLogs = new List<TrialData>();
    private Tile currentActiveTile;
    private TrialData currentTrial;
    private bool trialInProgress = false;
    private int trialCounter = 0;

    private string filePath;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        filePath = Path.Combine(Application.persistentDataPath, $"GoNoGo_Data.csv");
    }

    private void OnEnable()
    {
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Enable();
        }
    }
    private void OnDisable()
    {
        if (vrInteractionAction.action != null)
        {
            vrInteractionAction.action.Disable();
        }
    }
    private void Update()
    {
        bool inputPressed = ((vrInteractionAction != null && vrInteractionAction.action.WasPerformedThisFrame()) || Input.GetKeyDown(KeyCode.Space));
        if (trialInProgress && inputPressed)
        {
            OnUserInteracted();
        }
    }

    private void HandleNoResponse()
    {
        if (currentActiveTile == null)
            return;

        if (currentActiveTile.isGoTile)
        {
            currentTrial.outcome = "OmissionError";
        }
        else
        {
            currentTrial.outcome = "CorrectRejection";
        }

        currentTrial.interactionTime = -1f;
        currentTrial.reactionTime = -1f;
        Debug.Log("Handling no response...");
        EndTrial();
    }

    private void OnUserInteracted()
    {
        if (!trialInProgress ||
            currentActiveTile == null)
            return;

        float interactionTime = Time.time;
        currentTrial.interactionTime = interactionTime;
        if (!responseWindowActive)
        {
            //button pressed before it reached center
            currentTrial.reactionTime = (interactionTime - currentTrial.spawnTime) * 1000f;
            currentTrial.outcome = "Premature Response";
            audioSource.PlayOneShot(FailClip);
        }
        else
        {
            // Button pressed after it reached the center
            currentTrial.reactionTime = (interactionTime - responseStartTime) * 1000f;
            if (currentActiveTile.isGoTile)
            {
                currentTrial.outcome = "Hit";
                audioSource.PlayOneShot(SuccesClip);
            }
            else
            {
                currentTrial.outcome = "CommissionError";
                audioSource.PlayOneShot(FailClip);
            }
        }
        EndTrial();
    }

    private void EndTrial()
    {
        Tile tileToDisable = currentActiveTile;
        trialInProgress = false;
        responseWindowActive = false;
        responseStartTime = -1f;
        trialLogs.Add(currentTrial);

        if (tileToDisable != null)
        {
            tileToDisable.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("There is no tile to disable.");
        }
        Debug.Log($"[Trial {currentTrial.trialID}] Type: {currentTrial.tileType} | Outcome: {currentTrial.outcome} | RT: {currentTrial.reactionTime:F2} ms");
        currentActiveTile = null;
    }
    public void OnTileReachedPortal(Tile tile)
    {
        if (!trialInProgress || currentActiveTile != tile)
            return;
        responseWindowActive = true;
        responseStartTime = Time.time;

        Debug.Log(
            $"[Trial {currentTrial.trialID}] " +
            $"Tile reached response zone. " +
            $"Waiting for response."
        );
        StartCoroutine(ResponseWindowCoroutine());
    }
    private IEnumerator ResponseWindowCoroutine()
    {
        portalColor.material.color = Color.green;
        yield return new WaitForSeconds(responseWindowDuration);
        portalColor.material.color = Color.white;
        if(!trialInProgress)
        {
            yield break;
        }

        if (!responseWindowActive)
        {
            yield break;
        }
        Debug.Log("--- NO RESPONSE ---");
        HandleNoResponse();
    }
    public void RegisterTileSpawned(Tile tile, Transform portalTransform)
    {
        Debug.Log(
        $"REGISTER TILE → Name: {tile.gameObject.name} | " +
        $"isGoTile: {tile.isGoTile}"
        );
        trialCounter++;
        currentActiveTile = tile;
        trialInProgress = true;

        currentTrial = new TrialData
        {
            trialID = trialCounter,
            participantID = participantId,
            sessionID = sessionId,
            tileType = tile.isGoTile ? "Go" : "No-Go",
            spawnTime = Time.time,
            expectedHitTime = tile.targetHitTime,
            interactionTime = -1f,
            reactionTime = -1f,
            outcome = "Pending",
            spawnPos = tile.transform.position,
            portalPos = portalTransform.position
        };
        Debug.Log(
        $"TRIAL CREATED → Trial {currentTrial.trialID} | " +
        $"Tile Type: {currentTrial.tileType}"
        );
    }

    private void OnApplicationQuit()
    {
        SaveDataToCSV();
    }

    private void SaveDataToCSV()
    {
        StringBuilder csvContent = new StringBuilder();

        csvContent.AppendLine("TrialID, ParticipantID,SessionID,SpawnTime,ExpectedHitTime,InteractionTime,ReactionTime_ms,Outcome,SpawnX,SpawnY,SpawnZ,PortalX,PortalY,PortalZ");

        foreach (var trial in trialLogs)
        {
            string line = string.Format("{0},{1},{2},{3},{4:F4},{5:F4},{6:F4},{7:F2},{8},{9:F2},{10:F2},{11:F2},{12:F2},{13:F2},{14:F2}",
                trial.trialID,
                trial.participantID,
                trial.sessionID,
                trial.tileType,
                trial.spawnTime,
                trial.expectedHitTime,
                trial.interactionTime,
                trial.reactionTime,
                trial.outcome,
                trial.spawnPos.x, trial.spawnPos.y, trial.spawnPos.z,
                trial.portalPos.x, trial.portalPos.y, trial.portalPos.z
            );
            csvContent.AppendLine(line);
        }
        File.WriteAllText(filePath, csvContent.ToString());
        Debug.Log($"<color=green>Data successfully saved to CSV at: {filePath}</color>");
    }

}
