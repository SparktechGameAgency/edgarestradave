using UnityEngine;
using System;

public class ObstacleMover : MonoBehaviour
{
    [Header("Points")]
    public RectTransform startSpawner;
    public RectTransform endSpawner;

    [Header("Size (Scale)")]
    public float startScale = 0.3f;
    public float endScale   = 1.0f;

    [Header("Speed")]
    public float speed = 0.3f;

    [Header("Player")]
    public RectTransform playerObject;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    // Callback to notify CarSpawnManager when car finishes
    [HideInInspector] public Action onCarFinished;

    private RectTransform rectTransform;
    private float progress = 0f;
    private bool isGameOver = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void ResetCar()
    {
        progress               = 0f;
        isGameOver             = false;
        rectTransform.position = startSpawner.position;
        transform.localScale   = Vector3.one * startScale;
    }

    void Update()
    {
        if (isGameOver) return;
        if (startSpawner == null || endSpawner == null) return;

        progress += Time.deltaTime * speed;
        progress  = Mathf.Clamp01(progress);

        rectTransform.position = Vector3.Lerp(
            startSpawner.position,
            endSpawner.position,
            progress
        );

        float currentScale   = Mathf.Lerp(startScale, endScale, progress);
        transform.localScale = Vector3.one * currentScale;

        CheckCollision();

        // Car finished — notify manager
        if (progress >= 1f)
        {
            onCarFinished?.Invoke();
        }
    }

    void CheckCollision()
    {
        if (playerObject == null) return;

        Rect carRect    = GetWorldRect(rectTransform);
        Rect playerRect = GetWorldRect(playerObject);

        if (carRect.Overlaps(playerRect))
            TriggerGameOver();
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(
            corners[0].x,
            corners[0].y,
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );
    }

    void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }
}