using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    void OnEnable()
    {
        // Pause game when panel shows up
        Time.timeScale = 0f;
    }

    // Assign this to your Restart Button OnClick
    public void OnRestartButton()
    {
        // Resume time before reloading
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}