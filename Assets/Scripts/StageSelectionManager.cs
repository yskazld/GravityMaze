using UnityEngine;
using UnityEngine.SceneManagement;

public static class StageSelectionManager
{
    public const int StageCount = 50;

    private const string SelectedStageKey = "selected_stage";
    private const string HighestClearedStageKey = "highest_cleared_stage";
    private const string GameSceneName = "Game";

    public static int GetSelectedStage()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(SelectedStageKey, 1), 1, GetHighestUnlockedStage());
    }

    public static void SetSelectedStage(int stageId)
    {
        PlayerPrefs.SetInt(SelectedStageKey, Mathf.Clamp(stageId, 1, GetHighestUnlockedStage()));
        PlayerPrefs.Save();
    }

    public static int GetHighestClearedStage()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(HighestClearedStageKey, 0), 0, StageCount);
    }

    public static void MarkStageCleared(int stageId)
    {
        int current = GetHighestClearedStage();
        if (stageId > current)
        {
            PlayerPrefs.SetInt(HighestClearedStageKey, Mathf.Clamp(stageId, 0, StageCount));
            PlayerPrefs.Save();
        }
    }

    public static int GetHighestUnlockedStage()
    {
        return Mathf.Clamp(GetHighestClearedStage() + 1, 1, StageCount);
    }

    public static bool IsStageUnlocked(int stageId)
    {
        return stageId >= 1 && stageId <= GetHighestUnlockedStage();
    }

    public static void LoadStage(int stageId)
    {
        if (!IsStageUnlocked(stageId))
        {
            return;
        }

        SetSelectedStage(stageId);
        SceneManager.LoadScene(GameSceneName);
    }

    public static void LoadSelectedStage()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public static void ResetProgress()
    {
        PlayerPrefs.SetInt(SelectedStageKey, 1);
        PlayerPrefs.SetInt(HighestClearedStageKey, 0);
        PlayerPrefs.Save();
    }
}
