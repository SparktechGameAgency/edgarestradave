using UnityEngine;

public class CarSpawnManager : MonoBehaviour
{
    [Header("Cars (5 Car GameObjects)")]
    public GameObject[] cars;

    [Header("Spawners")]
    public RectTransform[] startSpawners;
    public RectTransform[] endSpawners;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;

    [Header("Player")]
    public RectTransform playerObject;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    private float spawnTimer = 0f;
    private bool isGameOver = false;

    // Track which spawner each car is using
    private int[] carSpawnerIndex;

    void Start()
    {
        carSpawnerIndex = new int[cars.Length];

        // Fill with -1 meaning no spawner assigned
        for (int i = 0; i < carSpawnerIndex.Length; i++)
            carSpawnerIndex[i] = -1;

        foreach (GameObject car in cars)
            car.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnNextCar();
        }
    }

    void SpawnNextCar()
    {
        if (cars.Length == 0) return;

        // Find a free spawner (no active car in it)
        int freeSpawner = GetFreeSpawner();
        if (freeSpawner == -1)
        {
            Debug.Log("No free spawner available");
            return;
        }

        // Find a free car (not active)
        int freeCar = GetFreeCar();
        if (freeCar == -1)
        {
            Debug.Log("No free car available");
            return;
        }

        // Assign spawner to this car
        carSpawnerIndex[freeCar] = freeSpawner;

        RectTransform start = startSpawners[freeSpawner];
        RectTransform end   = endSpawners[freeSpawner];
        GameObject car      = cars[freeCar];

        car.SetActive(true);

        ObstacleMover mover = car.GetComponent<ObstacleMover>();
        mover.startSpawner  = start;
        mover.endSpawner    = end;
        mover.playerObject  = playerObject;
        mover.gameOverPanel = gameOverPanel;
        mover.onCarFinished = () => OnCarFinished(freeCar); // Callback when done
        mover.ResetCar();
    }

    // Called when a car finishes its run
    void OnCarFinished(int carIndex)
    {
        carSpawnerIndex[carIndex] = -1;
        cars[carIndex].SetActive(false);
    }

    int GetFreeSpawner()
    {
        // Collect all busy spawners
        bool[] busySpawners = new bool[startSpawners.Length];
        for (int i = 0; i < carSpawnerIndex.Length; i++)
        {
            if (carSpawnerIndex[i] != -1)
                busySpawners[carSpawnerIndex[i]] = true;
        }

        // Find a free one randomly
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