using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityViewController : MonoBehaviour
{
    public Camera targetCamera;
    public GridManager grid;
    public StageGenerator stageGenerator;
    public float rotationDuration = 0.2f;
    public float rotationSpeedMultiplier = 0.25f;
    public float overshootDegrees = 20f;
    public float zoomOutDistance = 0.6f;
    public float shakeDistance = 0.3f;
    public float shakeFrequency = 32f;
    public bool applyToAllStages = true;
    public List<int> enabledStageIds = new List<int> { 1 };

    private Quaternion baseRotation;
    private Vector3 basePosition;
    private bool hasBaseRotation;
    private int currentQuarterTurns;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        CacheBaseRotation();
    }

    public IEnumerator RotateForGravity(Vector2Int dir)
    {
        ResolveReferences();
        CacheBaseRotation();

        if (targetCamera == null)
        {
            yield break;
        }

        int previousQuarterTurns = currentQuarterTurns;
        bool applyGravityView = ShouldApplyToCurrentStage();
        if (applyGravityView)
        {
            currentQuarterTurns = GetQuarterTurnsForWorldDirection(dir);
        }
        else
        {
            currentQuarterTurns = 0;
        }

        Quaternion targetRotation = applyGravityView
            ? GetGravityRotation(currentQuarterTurns)
            : baseRotation;

        Quaternion startRotation = targetCamera.transform.rotation;
        if (Quaternion.Angle(startRotation, targetRotation) <= 0.01f)
        {
            targetCamera.transform.rotation = targetRotation;
            targetCamera.transform.position = basePosition;
            yield break;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, rotationDuration / Mathf.Max(0.01f, rotationSpeedMultiplier));
        float signedAngleDelta = Mathf.DeltaAngle(GetZAngleFromQuarterTurns(previousQuarterTurns), GetZAngleFromQuarterTurns(currentQuarterTurns));
        if (Mathf.Approximately(signedAngleDelta, 0f))
        {
            signedAngleDelta = GetSignedOvershootFallback(startRotation, targetRotation);
        }
        float overshoot = Mathf.Sign(signedAngleDelta) * Mathf.Abs(overshootDegrees);
        Quaternion overshootRotationA = Quaternion.AngleAxis(overshoot, targetRotation * Vector3.forward) * targetRotation;
        Quaternion overshootRotationB = Quaternion.AngleAxis(-overshoot, targetRotation * Vector3.forward) * targetRotation;
        Vector3 pullBackPosition = basePosition - targetRotation * Vector3.forward * zoomOutDistance;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Quaternion currentRotation = EvaluateRotation(startRotation, overshootRotationA, overshootRotationB, targetRotation, t);
            targetCamera.transform.rotation = currentRotation;

            Vector3 zoomPosition = Vector3.Lerp(basePosition, pullBackPosition, Mathf.Sin(t * Mathf.PI));
            Vector3 shakeOffset = EvaluateShakeOffset(currentRotation, elapsed, t);
            targetCamera.transform.position = zoomPosition + shakeOffset;
            yield return null;
        }

        targetCamera.transform.rotation = targetRotation;
        targetCamera.transform.position = basePosition;
    }

    public Vector2Int ResolveMoveDirection(Vector2Int inputDir)
    {
        if (!ShouldApplyToCurrentStage())
        {
            return inputDir;
        }

        int nextQuarterTurns = Mod(currentQuarterTurns + GetRelativeQuarterTurnDelta(inputDir), 4);
        return GetWorldDirectionForQuarterTurns(nextQuarterTurns);
    }

    private void ResolveReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (grid == null)
        {
            grid = FindObjectOfType<GridManager>();
        }

        if (stageGenerator == null)
        {
            stageGenerator = FindObjectOfType<StageGenerator>();
        }
    }

    private void CacheBaseRotation()
    {
        if (hasBaseRotation || targetCamera == null)
        {
            return;
        }

        RefreshBasePoseFromCamera();
    }

    private bool ShouldApplyToCurrentStage()
    {
        if (applyToAllStages)
        {
            return true;
        }

        int currentStageId = stageGenerator != null && stageGenerator.stageData != null
            ? stageGenerator.stageData.stageId
            : StageSelectionManager.GetSelectedStage();

        return enabledStageIds.Contains(currentStageId);
    }

    private Quaternion GetGravityRotation(int quarterTurns)
    {
        float zAngle = quarterTurns * 90f;
        return baseRotation * Quaternion.AngleAxis(zAngle, Vector3.forward);
    }

    private static int GetRelativeQuarterTurnDelta(Vector2Int inputDir)
    {
        if (inputDir == Vector2Int.right)
        {
            return 1;
        }

        if (inputDir == Vector2Int.up)
        {
            return 2;
        }

        if (inputDir == Vector2Int.left)
        {
            return 3;
        }

        return 0;
    }

    private static Vector2Int GetWorldDirectionForQuarterTurns(int quarterTurns)
    {
        switch (Mod(quarterTurns, 4))
        {
            case 1:
                return Vector2Int.right;
            case 2:
                return Vector2Int.up;
            case 3:
                return Vector2Int.left;
            default:
                return Vector2Int.down;
        }
    }

    private static int GetQuarterTurnsForWorldDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.right)
        {
            return 1;
        }

        if (dir == Vector2Int.up)
        {
            return 2;
        }

        if (dir == Vector2Int.left)
        {
            return 3;
        }

        return 0;
    }

    private static int Mod(int value, int divisor)
    {
        int remainder = value % divisor;
        return remainder < 0 ? remainder + divisor : remainder;
    }

    private static Quaternion EvaluateRotation(Quaternion startRotation, Quaternion overshootRotationA, Quaternion overshootRotationB, Quaternion targetRotation, float t)
    {
        if (t < 0.5f)
        {
            return Quaternion.Slerp(startRotation, overshootRotationA, t / 0.5f);
        }

        if (t < 0.8f)
        {
            return Quaternion.Slerp(overshootRotationA, overshootRotationB, (t - 0.5f) / 0.3f);
        }

        return Quaternion.Slerp(overshootRotationB, targetRotation, (t - 0.8f) / 0.2f);
    }

    private static float GetSignedOvershootFallback(Quaternion startRotation, Quaternion targetRotation)
    {
        Vector3 startUp = startRotation * Vector3.up;
        Vector3 targetUp = targetRotation * Vector3.up;
        float sign = Mathf.Sign(Vector3.SignedAngle(startUp, targetUp, Vector3.forward));
        return Mathf.Approximately(sign, 0f) ? 1f : sign;
    }

    private static float GetZAngleFromQuarterTurns(int quarterTurns)
    {
        return Mod(quarterTurns, 4) * 90f;
    }

    private Vector3 EvaluateShakeOffset(Quaternion rotation, float elapsed, float normalizedTime)
    {
        float envelope = Mathf.Sin(normalizedTime * Mathf.PI);
        float phaseA = elapsed * shakeFrequency;
        float phaseB = elapsed * shakeFrequency * 1.37f;
        Vector3 screenRight = rotation * Vector3.right;
        Vector3 screenUp = rotation * Vector3.up;
        return (screenRight * Mathf.Sin(phaseA) + screenUp * Mathf.Cos(phaseB)) * (shakeDistance * envelope);
    }

    public void RefreshBasePoseFromCamera()
    {
        ResolveReferences();
        if (targetCamera == null)
        {
            return;
        }

        baseRotation = targetCamera.transform.rotation;
        basePosition = targetCamera.transform.position;
        hasBaseRotation = true;
    }
}
