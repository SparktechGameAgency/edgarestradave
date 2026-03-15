using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using DG.Tweening;

public class ScreenshotManager : MonoBehaviour
{
    [Header("Assign the PaintableImage RectTransform")]
    public RectTransform paintableArea;

    [Header("Download Button")]
    public Button downloadButton;

    [Header("Feedback UI (optional)")]
    public GameObject savedFeedbackText; // "Image Saved!" text

    void Start()
    {
        if (downloadButton != null)
            downloadButton.onClick.AddListener(CaptureAndSave);

        if (savedFeedbackText != null)
            savedFeedbackText.SetActive(false);
    }

    public void CaptureAndSave()
    {
        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        // Wait for end of frame so everything is rendered
        yield return new WaitForEndOfFrame();

        // Get pixel bounds of the paintable area
        Vector3[] corners = new Vector3[4];
        paintableArea.GetWorldCorners(corners);

        // Convert world corners to screen points
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        int x = Mathf.RoundToInt(bottomLeft.x);
        int y = Mathf.RoundToInt(bottomLeft.y);
        int width = Mathf.RoundToInt(topRight.x - bottomLeft.x);
        int height = Mathf.RoundToInt(topRight.y - bottomLeft.y);

        // Clamp to screen bounds
        x = Mathf.Clamp(x, 0, Screen.width);
        y = Mathf.Clamp(y, 0, Screen.height);
        width = Mathf.Clamp(width, 1, Screen.width - x);
        height = Mathf.Clamp(height, 1, Screen.height - y);

        // Capture screen area
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(x, y, width, height), 0, 0);
        screenshot.Apply();

        // Encode to PNG
        byte[] pngBytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        // Save to device
        string filePath = GetSavePath();
        File.WriteAllBytes(filePath, pngBytes);

        Debug.Log($"[Screenshot] Saved to: {filePath}");

        // Show feedback
        ShowSavedFeedback();

        // On mobile - refresh gallery
#if UNITY_ANDROID
            RefreshAndroidGallery(filePath);
#endif
    }

    string GetSavePath()
    {
        string fileName = "ColoringImage_" +
                          System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") +
                          ".png";

#if UNITY_ANDROID
            // Android: save to Pictures folder
            string folder = "/storage/emulated/0/Pictures/ColoringApp/";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return Path.Combine(folder, fileName);

#elif UNITY_IOS
            // iOS: save to app documents
            return Path.Combine(Application.persistentDataPath, fileName);

#else
        // PC/Editor: save to Desktop
        string desktop = System.Environment.GetFolderPath(
                         System.Environment.SpecialFolder.Desktop);
        return Path.Combine(desktop, fileName);
#endif
    }

    void ShowSavedFeedback()
    {
        if (savedFeedbackText == null) return;

        savedFeedbackText.SetActive(true);

        // Animate feedback text
        savedFeedbackText.transform.localScale = Vector3.zero;
        savedFeedbackText.transform
            .DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // Hide after 2 seconds
                DOVirtual.DelayedCall(2f, () =>
                {
                    savedFeedbackText.transform
                        .DOScale(0f, 0.2f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => savedFeedbackText.SetActive(false));
                });
            });
    }

#if UNITY_ANDROID
    void RefreshAndroidGallery(string filePath)
    {
        using (AndroidJavaClass mediaScannerConnection = 
               new AndroidJavaClass("android.media.MediaScannerConnection"))
        {
            using (AndroidJavaClass unityPlayer = 
                   new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = 
                    unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                mediaScannerConnection.CallStatic("scanFile",
                    currentActivity,
                    new string[] { filePath },
                    null,
                    null);
            }
        }
        Debug.Log("[Screenshot] Android gallery refreshed!");
    }
#endif
}
//```

//---

//## Unity Setup
//```
//Hierarchy:
//Canvas
//    └── CongratsPanel
//          ├── CongratsImage       (pulsing image)
//          ├── DownloadButton      ← Button to save image
//          └── SavedText           ← "Image Saved!" text (hidden)

//ScreenshotManager (on any GameObject):
//  Paintable Area  → PaintableImage RectTransform
//  Download Button → DownloadButton
//  Saved Feedback  → SavedText
//```

//---

//## Android Permissions

//In **Edit → Project Settings → Player → Android → Other Settings**:
//```
//✅ Write External Storage Permission → Auto or Required
//```

//---

//## Save Locations
//| Platform | Save Location |
//|---|---|
//| Android | Pictures/ColoringApp/ folder → visible in Gallery |
//| iOS | App Documents folder |
//| PC/Editor | Desktop |

//---

//## How It Works
//```
//All 15 colored → CongratsPanel appears
//User clicks Download button
//  → Captures PaintableImage area
//  → Saves as PNG with timestamp
//  → "Image Saved!" feedback appears
//  → On Android: refreshes gallery ✅
//  → Image visible in Photos/Gallery ✅