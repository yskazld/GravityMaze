using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageIntroController : MonoBehaviour
{
    [Header("Sequence")]
    public bool playOnStart = true;
    public float introDelay = 0.2f;
    public float playerShowDuration = 1.6f;
    public float goalShowDuration = 1.2f;
    public float orbitDuration = 3.2f;
    public float transitionDuration = 0.75f;

    [Header("Camera Framing")]
    public Vector3 playerCameraOffset = new Vector3(0f, 1.9f, -3.2f);
    public Vector3 goalCameraOffset = new Vector3(2.2f, 2.6f, -2.2f);
    public float orbitHeight = 4.2f;
    public float orbitRadiusPadding = 3.3f;
    public float lookHeightOffset = 0.35f;

    private Camera targetCamera;
    private GameManager gameManager;
    private StageGenerator stageGenerator;
    private GravityViewController gravityView;
    private PlayerController player;
    private Transform goal;
    private bool introPlayed;
    private bool isPlaying;
    private string overlayText = string.Empty;
    private Transform overlayTarget;
    private GUIStyle labelStyle;
    private IntroTrigger introTrigger = IntroTrigger.None;

    private enum IntroTrigger
    {
        None,
        FirstStage,
        BoardExpanded,
        BoxIntroduced,
        EnemyIntroduced
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(PlayIntroIfReady());
        }
    }

    private IEnumerator PlayIntroIfReady()
    {
        yield return null;
        ResolveReferences();

        if (introPlayed || targetCamera == null || gameManager == null || player == null || goal == null)
        {
            yield break;
        }

        introTrigger = DetermineIntroTrigger();
        if (introTrigger == IntroTrigger.None)
        {
            yield break;
        }

        introPlayed = true;
        isPlaying = true;
        gameManager.inputLocked = true;

        yield return new WaitForSeconds(introDelay);

        Vector3 basePosition = targetCamera.transform.position;
        Quaternion baseRotation = targetCamera.transform.rotation;

        yield return FocusOnTarget(player.transform, "PLAYER: Piggy Bank", playerCameraOffset, playerShowDuration);
        yield return FocusOnTarget(goal, "GOAL: Reach Here", goalCameraOffset, goalShowDuration);
        yield return OrbitStage(GetOverviewLabel());
        yield return MoveCamera(basePosition, baseRotation, transitionDuration);

        overlayText = string.Empty;
        overlayTarget = null;
        gravityView?.RefreshBasePoseFromCamera();
        gameManager.inputLocked = false;
        isPlaying = false;
    }

    private IntroTrigger DetermineIntroTrigger()
    {
        StageData current = stageGenerator != null ? stageGenerator.stageData : null;
        if (current == null)
        {
            return IntroTrigger.None;
        }

        if (current.stageId <= 1)
        {
            return IntroTrigger.FirstStage;
        }

        if (IsFirstStageOfBoardSize(current.stageId, current.width))
        {
            return IntroTrigger.BoardExpanded;
        }

        if (HasBoxes(current) && !WasFeatureSeenBefore(current.stageId, HasBoxes))
        {
            return IntroTrigger.BoxIntroduced;
        }

        if (HasNewEnemyType(current.stageId, current))
        {
            return IntroTrigger.EnemyIntroduced;
        }

        return IntroTrigger.None;
    }

    private static bool HasBoxes(StageData stage)
    {
        return stage != null && stage.boxPositions != null && stage.boxPositions.Count > 0;
    }

    private static bool HasEnemies(StageData stage)
    {
        return stage != null && stage.enemies != null && stage.enemies.Count > 0;
    }

    private static bool WasFeatureSeenBefore(int stageId, System.Func<StageData, bool> predicate)
    {
        for (int id = 1; id < stageId; id++)
        {
            if (predicate(StageCatalog.GetStage(id)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsFirstStageOfBoardSize(int stageId, int boardSize)
    {
        for (int id = 1; id < stageId; id++)
        {
            StageData previous = StageCatalog.GetStage(id);
            if (previous.width >= boardSize || previous.height >= boardSize)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasNewEnemyType(int stageId, StageData current)
    {
        if (!HasEnemies(current))
        {
            return false;
        }

        HashSet<EnemyType> seenTypes = new HashSet<EnemyType>();
        for (int id = 1; id < stageId; id++)
        {
            foreach (EnemyData enemy in StageCatalog.GetStage(id).enemies)
            {
                seenTypes.Add(enemy.type);
            }
        }

        foreach (EnemyData enemy in current.enemies)
        {
            if (!seenTypes.Contains(enemy.type))
            {
                return true;
            }
        }

        return false;
    }

    private string GetOverviewLabel()
    {
        switch (introTrigger)
        {
            case IntroTrigger.FirstStage:
                return "Stage 1 Overview";
            case IntroTrigger.BoardExpanded:
                return $"{stageGenerator.stageData.width}x{stageGenerator.stageData.height} Stage";
            case IntroTrigger.BoxIntroduced:
                return "New Gimmick: Box";
            case IntroTrigger.EnemyIntroduced:
                return HasDiagonalEnemy(stageGenerator.stageData) ? "New Enemy: Diagonal" : "New Enemy";
            default:
                return "Stage Overview";
        }
    }

    private static bool HasDiagonalEnemy(StageData stage)
    {
        if (!HasEnemies(stage))
        {
            return false;
        }

        foreach (EnemyData enemy in stage.enemies)
        {
            if (enemy.type == EnemyType.Diagonal)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator FocusOnTarget(Transform target, string text, Vector3 cameraOffset, float holdDuration)
    {
        if (target == null)
        {
            yield break;
        }

        overlayText = text;
        overlayTarget = target;

        Vector3 lookPosition = target.position + Vector3.up * lookHeightOffset;
        Vector3 cameraPosition = lookPosition + cameraOffset;
        Quaternion cameraRotation = Quaternion.LookRotation(lookPosition - cameraPosition, Vector3.up);

        yield return MoveCamera(cameraPosition, cameraRotation, transitionDuration);
        yield return new WaitForSeconds(holdDuration);
    }

    private IEnumerator OrbitStage(string text)
    {
        if (stageGenerator == null || stageGenerator.stageData == null)
        {
            yield break;
        }

        overlayText = text;
        overlayTarget = null;

        StageData stageData = stageGenerator.stageData;
        float centerX = stageGenerator.grid.origin.x + ((stageData.width - 1) * stageGenerator.grid.cellSize * 0.5f);
        float centerZ = stageGenerator.grid.origin.y + ((stageData.height - 1) * stageGenerator.grid.cellSize * 0.5f);
        Vector3 center = new Vector3(centerX, 0.35f, centerZ);
        float orbitRadius = Mathf.Max(stageData.width, stageData.height) + orbitRadiusPadding;

        float elapsed = 0f;
        while (elapsed < orbitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / orbitDuration);
            float angle = Mathf.Lerp(0f, 360f, t);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 orbitOffset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * orbitRadius;
            Vector3 cameraPosition = center + orbitOffset + Vector3.up * orbitHeight;

            targetCamera.transform.position = cameraPosition;
            targetCamera.transform.rotation = Quaternion.LookRotation(center - cameraPosition, Vector3.up);
            yield return null;
        }
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        if (targetCamera == null)
        {
            yield break;
        }

        Vector3 startPosition = targetCamera.transform.position;
        Quaternion startRotation = targetCamera.transform.rotation;

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);
        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            targetCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, eased);
            targetCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, eased);
            yield return null;
        }

        targetCamera.transform.position = targetPosition;
        targetCamera.transform.rotation = targetRotation;
    }

    private void ResolveReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (stageGenerator == null)
        {
            stageGenerator = FindObjectOfType<StageGenerator>();
        }

        if (gravityView == null)
        {
            gravityView = FindObjectOfType<GravityViewController>();
        }

        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        if (goal == null)
        {
            GameObject goalObject = GameObject.FindGameObjectWithTag("Goal");
            if (goalObject != null)
            {
                goal = goalObject.transform;
            }
        }
    }

    private void OnGUI()
    {
        if (!isPlaying || string.IsNullOrEmpty(overlayText) || targetCamera == null)
        {
            return;
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                }
            };
            labelStyle.padding = new RectOffset(14, 14, 10, 10);
        }

        Rect rect = new Rect(Screen.width * 0.5f - 120f, 28f, 240f, 42f);
        GUI.Box(rect, overlayText, labelStyle);

        if (overlayTarget == null)
        {
            return;
        }

        Vector3 screenPoint = targetCamera.WorldToScreenPoint(overlayTarget.position + Vector3.up * 0.8f);
        if (screenPoint.z <= 0f)
        {
            return;
        }

        Rect markerRect = new Rect(screenPoint.x - 60f, Screen.height - screenPoint.y - 18f, 120f, 32f);
        GUI.Box(markerRect, overlayText, labelStyle);
    }
}
