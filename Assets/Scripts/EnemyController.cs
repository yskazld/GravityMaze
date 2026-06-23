using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Vector2Int cell;
    public Vector2Int moveDir = Vector2Int.up; // for Orthogonal use, for Diagonal use (1,1) etc
    public EnemyType type = EnemyType.Orthogonal;
    public GridManager grid;

    private void Start()
    {
        if (grid == null) grid = FindObjectOfType<GridManager>();
        transform.position = grid.CellToWorld(cell);
        gameObject.tag = "Enemy";
        grid.SetOccupant(cell, gameObject);
    }

    // Called by GameManager after player move. Moves one cell if possible.
    public void TryMoveOne()
    {
        Vector2Int[] directions =
        {
            moveDir,
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (Vector2Int direction in directions)
        {
            if (direction == Vector2Int.zero)
            {
                continue;
            }

            Vector2Int target = cell + direction;
            if (!grid.InBounds(target) || grid.IsGoalCell(target))
            {
                continue;
            }

            GameObject occ = grid.GetOccupant(target);
            if (occ == null)
            {
                moveDir = direction;
                grid.SetOccupant(cell, null);
                cell = target;
                transform.position = grid.CellToWorld(cell);
                grid.SetOccupant(cell, gameObject);
                return;
            }
        }
    }
}
