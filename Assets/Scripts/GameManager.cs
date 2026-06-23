using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GridManager grid;
    public PlayerController player;
    public List<BoxController> boxes = new List<BoxController>();
    public List<EnemyController> enemies = new List<EnemyController>();

    public StageGenerator stageGenerator;
    public ClearUIController clearUI;

    private bool isBusy = false;

    public void Start()
    {
        if (grid == null) grid = FindObjectOfType<GridManager>();
        if (stageGenerator == null) stageGenerator = FindObjectOfType<StageGenerator>();
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (boxes == null || boxes.Count == 0)
        {
            boxes = new List<BoxController>(FindObjectsOfType<BoxController>());
        }
        if (enemies == null || enemies.Count == 0)
        {
            enemies = new List<EnemyController>(FindObjectsOfType<EnemyController>());
        }
        if (clearUI == null)
        {
            clearUI = FindObjectOfType<ClearUIController>(true);
        }

        Debug.Log($"GameManager start: player={(player!=null)}, boxes={boxes.Count}, enemies={enemies.Count}, grid={(grid!=null)}, stageGenerator={(stageGenerator!=null)}");
    }

    public void MoveUp() => TryMove(Vector2Int.up);
    public void MoveDown() => TryMove(Vector2Int.down);
    public void MoveLeft() => TryMove(Vector2Int.left);
    public void MoveRight() => TryMove(Vector2Int.right);

    public void TryMove(Vector2Int dir)
    {
        if (isBusy || player == null) return;
        StartCoroutine(MoveSequence(dir));
    }

    private IEnumerator MoveSequence(Vector2Int dir)
    {
        isBusy = true;
        try
        {
            boxes = new List<BoxController>(FindObjectsOfType<BoxController>());
            enemies = new List<EnemyController>(FindObjectsOfType<EnemyController>());

            if (player != null)
            {
                yield return StartCoroutine(player.Slide(dir));
            }

            if (TryHandleClear())
            {
                yield break;
            }

            foreach (BoxController box in GetBoxesInMoveOrder(dir))
            {
                yield return StartCoroutine(box.Slide(dir));
            }

            if (TryHandleClear())
            {
                yield break;
            }

            foreach (EnemyController enemy in enemies)
            {
                if (player != null && enemy.cell == player.cell)
                {
                    Debug.Log("Failed: Player moved into enemy");
                    yield break;
                }
            }

            foreach (EnemyController enemy in enemies)
            {
                enemy.TryMoveOne();
            }
        }
        finally
        {
            isBusy = false;
        }
    }

    private IEnumerable<BoxController> GetBoxesInMoveOrder(Vector2Int dir)
    {
        if (dir == Vector2Int.right)
        {
            return boxes.OrderByDescending(box => box.cell.x).ThenBy(box => box.cell.y);
        }

        if (dir == Vector2Int.left)
        {
            return boxes.OrderBy(box => box.cell.x).ThenBy(box => box.cell.y);
        }

        if (dir == Vector2Int.up)
        {
            return boxes.OrderByDescending(box => box.cell.y).ThenBy(box => box.cell.x);
        }

        return boxes.OrderBy(box => box.cell.y).ThenBy(box => box.cell.x);
    }

    private bool TryHandleClear()
    {
        if (player == null || stageGenerator == null || stageGenerator.stageData == null)
        {
            return false;
        }

        if (player.cell != stageGenerator.stageData.goalPosition)
        {
            return false;
        }

        Debug.Log("Clear!");
        StageSelectionManager.MarkStageCleared(stageGenerator.stageData.stageId);
        if (clearUI != null)
        {
            clearUI.Show();
        }
        else
        {
            Debug.LogWarning("GameManager: ClearUIController is not assigned");
        }

        return true;
    }
}
