using UnityEngine;
using System;

public class TireLineMover : MonoBehaviour
{
    [Header("Points")]
    public RectTransform startSpawner;
    public RectTransform endSpawner;

    [Header("Scale XYZ")]
    public Vector3 startScaleXYZ = new Vector3(0.3f, 0.3f, 0.3f);
    public Vector3 endScaleXYZ   = new Vector3(1.0f, 1.0f, 1.0f);

    [Header("Speed")]
    public float speed = 0.3f;

    [HideInInspector] public Action onTirePassedPlayer;

    private RectTransform rectTransform;
    private float progress = 0f;
    private bool hasPassedPlayer = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // Reset and start moving as soon as object is enabled
        progress        = 0f;
        hasPassedPlayer = false;

        rectTransform.position   = startSpawner.position;
        rectTransform.localScale = startScaleXYZ;
    }

    public void StopSpawning()
    {
        enabled = false;

        // Reset back to start so it's hidden/ready for next time
        rectTransform.position   = startSpawner.position;
        rectTransform.localScale = startScaleXYZ;
    }

    void Update()
    {
        if (startSpawner == null || endSpawner == null) return;

        progress += Time.deltaTime * speed;
        progress  = Mathf.Clamp01(progress);

        rectTransform.position   = Vector3.Lerp(startSpawner.position, endSpawner.position, progress);
        rectTransform.localScale = Vector3.Lerp(startScaleXYZ, endScaleXYZ, progress);

        if (!hasPassedPlayer && progress >= 0.5f)
        {
            hasPassedPlayer = true;
            onTirePassedPlayer?.Invoke();
        }

        if (progress >= 1f)
        {
            // Reset back to start — loops forever, no delay, no prefab
            progress        = 0f;
            hasPassedPlayer = false;

            rectTransform.position   = startSpawner.position;
            rectTransform.localScale = startScaleXYZ;
        }
    }
}