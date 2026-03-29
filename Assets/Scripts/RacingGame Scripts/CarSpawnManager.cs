using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CarSpawnManager : MonoBehaviour
{
    [Header("Cars (5 Car GameObjects)")]
    public GameObject[] cars;

    [Header("Spawners")]
    public RectTransform[] startSpawners;
    public RectTransform[] endSpawners;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public int totalCarsToSpawn = 10;

    [Header("Player")]
    public RectTransform playerObject;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    [Header("Road Lines")]
    public GameObject[] roadLines;

    [Header("Win Settings")]
    public GameObject finishLineObject;
    public RectTransform finishLineTarget;
    public GameObject youWinPanel;
    public float moveToFinishSpeed = 0.5f;
    public Vector3 playerEndScale = new Vector3(0.1f, 0.1f, 0.1f); // X Y Z from Inspector

    private float spawnTimer = 0f;
    private bool isGameOver = false;
    private int spawnedCount = 0;
    private int finishedCount = 0;
    private bool allSpawned = false;
    private bool winTriggered = false;
    private int[] carSpawnerIndex;

    void Start()
    {
        carSpawnerIndex = new int[cars.Length];
        for (int i = 0; i < carSpawnerIndex.Length; i++)
            carSpawnerIndex[i] = -1;

        foreach (GameObject car in cars)
            car.SetActive(false);

        if (finishLineObject != null)
            finishLineObject.SetActive(false);

        if (youWinPanel != null)
            youWinPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;
        if (allSpawned) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnNextCar();
        }
    }

    void SpawnNextCar()
    {
        if (spawnedCount >= totalCarsToSpawn)
        {
            allSpawned = true;
            return;
        }

        int freeSpawner = GetFreeSpawner();
        if (freeSpawner == -1) return;

        int freeCar = GetFreeCar();
        if (freeCar == -1) return;

        carSpawnerIndex[freeCar] = freeSpawner;

        RectTransform start = startSpawners[freeSpawner];
        RectTransform end   = endSpawners[freeSpawner];
        GameObject car      = cars[freeCar];

        car.SetActive(true);
        spawnedCount++;

        ObstacleMover mover = car.GetComponent<ObstacleMover>();
        mover.startSpawner  = start;
        mover.endSpawner    = end;
        mover.playerObject  = playerObject;
        mover.gameOverPanel = gameOverPanel;
        mover.onCarFinished = () => OnCarFinished(freeCar);
        mover.ResetCar();
    }

    void OnCarFinished(int carIndex)
    {
        carSpawnerIndex[carIndex] = -1;
        cars[carIndex].SetActive(false);

        finishedCount++;

        Debug.Log("Car finished. finishedCount: " + finishedCount + " / " + totalCarsToSpawn + " allSpawned: " + allSpawned);

        if (allSpawned && finishedCount >= totalCarsToSpawn && !winTriggered)
        {
            winTriggered = true;
            Debug.Log("Triggering win sequence!");
            StartWinSequence();
        }
    }

    void StartWinSequence()
    {
        Debug.Log("StartWinSequence called!");

        foreach (GameObject line in roadLines)
        {
            if (line != null)
                line.SetActive(false);
        }

        if (finishLineObject != null)
            finishLineObject.SetActive(true);

        LaneController lane = playerObject.GetComponent<LaneController>();
        if (lane != null)
            lane.enabled = false;

        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        yield return null;

        Vector3 startPos   = playerObject.position;
        Vector3 targetPos  = finishLineTarget.position;
        Vector3 startScale = playerObject.localScale;

        Debug.Log("Player start pos: " + startPos);
        Debug.Log("Target finish pos: " + targetPos);

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * moveToFinishSpeed;
            progress  = Mathf.Clamp01(progress);

            playerObject.position   = Vector3.Lerp(startPos, targetPos, progress);
            playerObject.localScale = Vector3.Lerp(startScale, playerEndScale, progress);

            yield return null;
        }

        Debug.Log("Player reached finish line!");

        yield return new WaitForSeconds(0.5f);

        if (youWinPanel != null)
            youWinPanel.SetActive(true);

        Debug.Log("Win sequence done!");
    }

    int GetFreeSpawner()
    {
        bool[] busySpawners = new bool[startSpawners.Length];
        for (int i = 0; i < carSpawnerIndex.Length; i++)
        {
            if (carSpawnerIndex[i] != -1)
                busySpawners[carSpawnerIndex[i]] = true;
        }

        int attempts = 0;
        while (attempts < 10)
        {
            int random = Random.Range(0, startSpawners.Length);
            if (!busySpawners[random])
                return random;
            attempts++;
        }
        return -1;
    }

    int GetFreeCar()
    {
        for (int i = 0; i < cars.Length; i++)
        {
            if (!cars[i].activeSelf)
                return i;
        }
        return -1;
    }

    public void SetGameOver()
    {
        isGameOver = true;
    }
}