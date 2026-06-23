using System.Collections.Generic;
using UnityEngine;

public static class Solver
{
    // Very small reachability check assuming boxes are static obstacles.
    public static bool CanReachGoal(GridManager grid, Vector2Int start, Vector2Int goal)
    {
        var visited = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();
        q.Enqueue(start);
        visited.Add(start);
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == goal) return true;
            foreach (var dir in directions)
            {
                GameObject blocker;
                var end = grid.SlideUntilBlocked(cur, dir, out blocker);
                if (!visited.Contains(end))
                {
                    visited.Add(end);
                    q.Enqueue(end);
                }
            }
        }
        return false;
    }

    private static Vector2Int[] directions = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };
}
