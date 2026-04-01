using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
    public GameObject crashEffect;

    [Header("Road Lines")]
    public GameObject[] roadLines;

    [Header("Win Settings")]
    public GameObject finishLineObject;
    public GameObject finishLineMover;
    public RectTransform finishLineTarget;
    public GameObject youWinPanel;
    public float moveToFinishSpeed = 0.5f;
    public Vector3 playerEndScale = new Vector3(0.1f, 0.1f, 0.1f);
    public float youWinDelay = 5f;

    [Header("BGHouses Grow")]
    public RectTransform bgHouses;
    public Vector3 bgHousesStartScale  = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 bgHousesEndScale    = new Vector3(1f, 1f, 1f);
    public float bgHousesSmoothSpeed   = 2f;

    [Header("Trees")]
    public RectTransform[] treeObjects;    // Drag your tree RectTransforms here
    public RectTransform treeStartPoint;   // Start point (e.g. right side)
    public RectTransform treeEndPoint;     // End point (e.g. left side)

    [Header("Position UI")]
    public TextMeshProUGUI positionText;

    private float spawnTimer = 0f;
    private bool isGameOver = false;
    private int spawnedCount = 0;
    private int finishedCount = 0;
    private bool allSpawned = false;
    private bool winTriggered = false;
    private int[] carSpawnerIndex;
    private int carsPassed = 0;
    private int nextCarIndex = 0;

    private List<RectTransform> playerCPoints = new List<RectTransform>();
    private Coroutine bgHousesCoroutine;

    void Start()
    {
        carSpawnerIndex = new int[cars.Length];
        for (int i = 0; i < carSpawnerIndex.Length; i++)
            carSpawnerIndex[i] = -1;

        foreach (GameObject car in cars)
            car.SetActive(false);

        if (finishLineObject != null)
            finishLineObject.SetActive(false);

        if (finishLineMover != null)
            finishLineMover.SetActive(false);

        if (youWinPanel != null)
            youWinPanel.SetActive(false);

        if (crashEffect != null)
            crashEffect.SetActive(false);

        if (bgHouses != null)
        {
            bgHouses.localScale = bgHousesStartScale;
            SmoothBGHousesToScale(bgHousesEndScale);
        }

        InitTrees();
        CollectPlayerCPoints();
        UpdatePositionText();
    }

    void InitTrees()
    {
        if (treeObjects == null) return;
        foreach (RectTransform tree in treeObjects)
        {
            if (tree == null) continue;
            TreeMover mover = tree.GetComponent<TreeMover>();
            if (mover != null)
            {
                mover.startPoint = treeStartPoint;
                mover.endPoint   = treeEndPoint;
                mover.enabled    = true;
            }
        }
    }

    void CollectPlayerCPoints()
    {
        playerCPoints.Clear();
        FindAllCPoints(playerObject, playerCPoints);

        if (playerCPoints.Count == 0)
            Debug.LogWarning("CarSpawnManager → No C points found on PlayerObject!");
        else
            Debug.Log("CarSpawnManager → Found " + playerCPoints.Count + " player C points.");
    }

    void FindAllCPoints(Transform parent, List<RectTransform> result)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("C"))
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt != null) result.Add(rt);
            }
            FindAllCPoints(child, result);
        }
    }

    void Update()
    {
        if (isGameOver) return;

        UpdatePositionText();

        if (allSpawned) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnNextCar();
        }
    }

    public void OnCarPassedPlayer()
    {
        carsPassed++;
    }

    void UpdatePositionText()
    {
        if (positionText == null) return;
        int playerPosition = Mathf.Max(1, totalCarsToSpawn - carsPassed);
        positionText.text = playerPosition + "/" + totalCarsToSpawn;
    }

    void SmoothBGHousesToScale(Vector3 targetScale)
    {
        if (bgHouses == null) return;
        if (bgHousesCoroutine != null)
            StopCoroutine(bgHousesCoroutine);
        bgHousesCoroutine = StartCoroutine(BGHousesScaleCoroutine(targetScale));
    }

    IEnumerator BGHousesScaleCoroutine(Vector3 targetScale)
    {
        Vector3 fromScale = bgHouses.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * bgHousesSmoothSpeed;
            bgHouses.localScale = Vector3.Lerp(fromScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        bgHouses.localScale = targetScale;
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

        int freeCar = GetNextFreeCar();
        if (freeCar == -1) return;

        carSpawnerIndex[freeCar] = freeSpawner;

        RectTransform start = startSpawners[freeSpawner];
        RectTransform end   = endSpawners[freeSpawner];
        GameObject car      = cars[freeCar];

        car.SetActive(true);
        spawnedCount++;

        ObstacleMover mover = car.GetComponent<ObstacleMover>();
        mover.startSpawner      = start;
        mover.endSpawner        = end;
        mover.playerObject      = playerObject;
        mover.playerCPoints     = playerCPoints;
        mover.gameOverPanel     = gameOverPanel;
        mover.crashEffect       = crashEffect;
        mover.onCarFinished     = () => OnCarFinished(freeCar);
        mover.onCarPassedPlayer = () => OnCarPassedPlayer();
        mover.onGameOver         = () => OnGameOver();
        mover.ResetCar();
    }

    int GetNextFreeCar()
    {
        int total = cars.Length;
        for (int i = 0; i < total; i++)
        {
            int index = (nextCarIndex + i) % total;
            if (!cars[index].activeSelf)
            {
                nextCarIndex = (index + 1) % total;
                return index;
            }
        }
        return -1;
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

    void OnGameOver()
    {
        // Stop BGHouses grow coroutine instantly
        if (bgHousesCoroutine != null)
        {
            StopCoroutine(bgHousesCoroutine);
            bgHousesCoroutine = null;
        }

        // Pause all trees
        foreach (TreeMover tree in FindObjectsOfType<TreeMover>(true))
            tree.PauseTree();
    }

    public void SetGameOver()
    {
        isGameOver = true;
    }

    void StartWinSequence()
    {
        Debug.Log("StartWinSequence called!");

        foreach (GameObject line in roadLines)
        {
            if (line != null)
                line.SetActive(false);
        }

        // Pause all TireLineMover scripts on win
        foreach (TireLineMover tire in FindObjectsOfType<TireLineMover>(true))
            tire.enabled = false;

        // Pause all trees on win
        foreach (TreeMover tree in FindObjectsOfType<TreeMover>(true))
            tree.PauseTree();

        LaneController lane = playerObject.GetComponent<LaneController>();
        if (lane != null)
            lane.enabled = false;

        if (finishLineMover != null)
        {
            finishLineMover.SetActive(true);

            FinishLineMover flMover = finishLineMover.GetComponent<FinishLineMover>();
            if (flMover != null)
            {
                flMover.ResetFinishLine();

                flMover.onFinishLineReachedEnd = () =>
                {
                    flMover.onFinishLineReachedEnd = null;

                    if (finishLineObject != null)
                        finishLineObject.SetActive(true);

                    StartCoroutine(MovePlayerToFinish());
                };
            }
            else
            {
                if (finishLineObject != null)
                    finishLineObject.SetActive(true);
                StartCoroutine(MovePlayerToFinish());
            }
        }
        else
        {
            if (finishLineObject != null)
                finishLineObject.SetActive(true);
            StartCoroutine(MovePlayerToFinish());
        }
    }

    IEnumerator MovePlayerToFinish()
    {
        yield return null;

        Vector3 startPos   = playerObject.position;
        Vector3 targetPos  = finishLineTarget.position;
        Vector3 startScale = playerObject.localScale;

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

        yield return new WaitForSeconds(youWinDelay);

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
}