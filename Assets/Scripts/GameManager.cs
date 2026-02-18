using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<GameObject> allPanel;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();
    public void ShowPanel(GameObject panelToShow)
    {

        foreach (GameObject panel in allPanel)
            panel.SetActive(false);

        if (panelToShow != null)
        {
            AnimatePanelFade(panelToShow);
            panelHistory.Push(panelToShow);
        }
    }

    public void AnimatePanelFade(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        panel.transform.localScale = Vector3.one * 0.4f;
        canvasGroup.alpha = 1f;
        panel.SetActive(true);

        // Animate both scale and fade
        panel.transform.DOScale(1f, .4f).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, 0.3f);
    }

    public void CloseCurrectPanel()
    {
        if (panelHistory.Count > 0)
        {
            GameObject closePanel = panelHistory.Pop();

            // Animate closing before deactivating
            closePanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => closePanel.SetActive(false));

            if (panelHistory.Count > 0)
            {
                GameObject previosPanel = panelHistory.Peek();
                AnimatePanelFade(previosPanel);
            }
            else
            {
                //AnimatePanelFade(mainMenuPanel);
                //panelHistory.Push(mainMenuPanel);
            }
        }
    }

    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
