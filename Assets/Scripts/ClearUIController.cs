using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearUIController : MonoBehaviour
{
    public GameObject clearPanel;
    public string gameSceneName = "Game";
    public string mainSceneName = "Main";

    private void Awake()
    {
        if (clearPanel == null)
        {
            clearPanel = gameObject;
        }

        Hide();
    }

    public void Show()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }
    }

    public void Hide()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToMain()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}
