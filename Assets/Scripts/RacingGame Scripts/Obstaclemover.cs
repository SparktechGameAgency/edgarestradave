using UnityEngine;
using System;
using System.Collections;

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

    [HideInInspector] public RectTransform playerObject;
    [HideInInspector] public RectTransform pImage;
    [HideInInspector] public RectTransform cImage;
    [HideInInspector] public GameObject gameOverPanel;
    [HideInInspector] public GameObject crashEffect;
    [HideInInspector] public Action onCarFinished;
    [HideInInspector] public Action onCarPassedPlayer;

    private RectTransform rectTransform;
    private float progress = 0f;
    private bool isGameOver = false;
    private bool hasPassedPlayer = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void ResetCar()
    {
        progress          = 0f;
        isGameOver        = false;
        hasPassedPlayer   = false;

        rectTransform.position = startSpawner.position;
        transform.localScale   = Vector3.one * startScale;

        // Find CImage recursively
        Transform found = FindDeep(transform, "CImage");
        if (found != null)
        {
            cImage = found.GetComponent<RectTransform>();
            Debug.Log(gameObject.name + " → CImage found at: " + GetPath(found));
        }
        else
        {
            cImage = null;
            Debug.LogWarning(gameObject.name + " → CImage NOT FOUND — skipping collision.");
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

        if (progress > 0.75f)
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
        if (pImage == null || cImage == null) return;

        Rect playerRect = GetFullRect(pImage);
        Rect carRect    = GetFullRect(cImage);

        if (carRect.Overlaps(playerRect))
            TriggerGameOver();
    }

    Rect GetFullRect(RectTransform rt)
    {
        float w = rt.sizeDelta.x * rt.lossyScale.x;
        float h = rt.sizeDelta.y * rt.lossyScale.y;
        float x = rt.position.x - w * rt.pivot.x;
        float y = rt.position.y - h * rt.pivot.y;
        return new Rect(x, y, w, h);
    }

    void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Vector3 crashPos = (pImage.position + cImage.position) / 2f;
        crashPos.z = 0f;

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

        yield return new WaitForSecondsRealtime(0.8f);

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

    Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeep(child, name);
            if (result != null) return result;
        }
        return null;
    }

    string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}