using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 3;
    public int height = 3;
    public Vector2 origin = Vector2.zero;
    public float cellSize = 1f;

    public LayerMask wallLayer;
    public bool hasGoal;
    public Vector2Int goalCell;

    private Dictionary<Vector2Int, GameObject> occupants = new Dictionary<Vector2Int, GameObject>();

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(origin.x + cell.x * cellSize, 0f, origin.y + cell.y * cellSize);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        int x = Mathf.RoundToInt((world.x - origin.x) / cellSize);
        int y = Mathf.RoundToInt((world.z - origin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    public bool InBounds(Vector2Int c)
    {
        return c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;
    }

    public void SetOccupant(Vector2Int c, GameObject go)
    {
        if (!InBounds(c)) return;
        if (go == null) occupants.Remove(c);
        else occupants[c] = go;
    }

    public GameObject GetOccupant(Vector2Int c)
    {
        occupants.TryGetValue(c, out var go);
        return go;
    }

    public void SetGoalCell(Vector2Int c)
    {
        hasGoal = true;
        goalCell = c;
    }

    public void ClearGoalCell()
    {
        hasGoal = false;
    }

    public void ClearOccupants()
    {
        occupants.Clear();
    }

    public bool IsGoalCell(Vector2Int c)
    {
        return hasGoal && c == goalCell;
    }

    public bool TryGetGoalOnPath(Vector2Int start, Vector2Int end, out Vector2Int hitGoalCell)
    {
        hitGoalCell = goalCell;
        if (!hasGoal)
        {
            return false;
        }

        if (start.x == end.x && goalCell.x == start.x)
        {
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            return goalCell.y >= minY && goalCell.y <= maxY;
        }

        if (start.y == end.y && goalCell.y == start.y)
        {
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            return goalCell.x >= minX && goalCell.x <= maxX;
        }

        return false;
    }

    public bool IsBlocked(Vector2Int c)
    {
        if (!InBounds(c)) return true;
        var occ = GetOccupant(c);
        if (occ == null) return false;
        return occ.CompareTag("Wall") || occ.CompareTag("Box") || occ.CompareTag("Enemy");
    }

    // Slides from start in dir until next cell would be blocked; returns final cell and the blocking occupant (may be null if edge)
    public Vector2Int SlideUntilBlocked(Vector2Int start, Vector2Int dir, out GameObject blocker)
    {
        return SlideUntilBlocked(start, dir, out blocker, false);
    }

    public Vector2Int SlideUntilBlocked(Vector2Int start, Vector2Int dir, out GameObject blocker, bool stopBeforeGoal)
    {
        Vector2Int pos = start;
        blocker = null;
        while (true)
        {
            Vector2Int next = pos + dir;
            if (!InBounds(next))
            {
                blocker = null;
                return pos; // stop at edge cell
            }
            if (stopBeforeGoal && IsGoalCell(next))
            {
                blocker = null;
                return pos;
            }
            var occ = GetOccupant(next);
            if (occ != null)
            {
                blocker = occ;
                return pos; // stop before occupied
            }
            pos = next;
        }
    }
}
