using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Collision")]
    public float collisionDistanceX = 200f;
    public float collisionDistanceY = 300f;

    [HideInInspector] public RectTransform playerObject;
    [HideInInspector] public List<RectTransform> playerCPoints;
    [HideInInspector] public GameObject gameOverPanel;
    [HideInInspector] public GameObject crashEffect;
    [HideInInspector] public Action onCarFinished;
    [HideInInspector] public Action onCarPassedPlayer;

    private RectTransform rectTransform;
    private List<RectTransform> carCPoints = new List<RectTransform>();
    private float progress = 0f;
    private bool isGameOver = false;
    private bool hasPassedPlayer = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void ResetCar()
    {
        progress        = 0f;
        isGameOver      = false;
        hasPassedPlayer = false;

        rectTransform.position = startSpawner.position;
        transform.localScale   = Vector3.one * startScale;

        carCPoints.Clear();
        FindAllCPoints(transform, carCPoints);
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

        if (progress > 0.3f)
            CheckCollision();

        if (progress >= 1f)
        {
            if (!hasPassedPlayer)
            {
                hasPassedPlayer = true;
                onCarPassedPlayer?.Invoke();
            }
            onCarFinished?.Invoke();
        }
    }

    void CheckCollision()
    {
        if (playerCPoints == null || playerCPoints.Count == 0) return;
        if (carCPoints == null || carCPoints.Count == 0) return;

        foreach (RectTransform carC in carCPoints)
        {
            if (carC == null) continue;

            foreach (RectTransform playerC in playerCPoints)
            {
                if (playerC == null) continue;

                float dx = Mathf.Abs(carC.position.x - playerC.position.x);
                float dy = Mathf.Abs(carC.position.y - playerC.position.y);

                if (dx <= collisionDistanceX && dy <= collisionDistanceY)
                {
                    Vector3 crashPos = (carC.position + playerC.position) / 2f;
                    crashPos.z = 0f;
                    TriggerGameOver(crashPos);
                    return;
                }
            }
        }
    }

    void TriggerGameOver(Vector3 crashPos)
    {
        if (isGameOver) return;
        isGameOver = true;
        StartCoroutine(ShowCrashThenGameOver(crashPos));
    }

    IEnumerator ShowCrashThenGameOver(Vector3 crashPos)
    {
        if (crashEffect != null)
        {
            RectTransform crashRT = crashEffect.GetComponent<RectTransform>();
            if (crashRT != null)
                crashRT.position = crashPos;
            else
                crashEffect.transform.position = crashPos;

            crashEffect.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(2f);

        if (crashEffect != null)
            crashEffect.SetActive(false);

        if (gameOverPanel != null)
        {
            RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}