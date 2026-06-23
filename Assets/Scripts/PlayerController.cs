using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class PlayerController : MonoBehaviour
{
    public Vector2Int cell;
    public GridManager grid;
    public float moveSpeed = 6f;

    public System.Action<Vector2Int> OnMoved; // notify GameManager

    private void Start()
    {
        if (grid == null) grid = FindObjectOfType<GridManager>();
        transform.position = grid.CellToWorld(cell);
        gameObject.tag = "Player";
        grid.SetOccupant(cell, gameObject);
    }

    public IEnumerator Slide(Vector2Int dir)
    {
        grid.SetOccupant(cell, null);
        Vector2Int startCell = cell;
        GameObject blocker;
        Vector2Int target = grid.SlideUntilBlocked(cell, dir, out blocker);
        if (grid.TryGetGoalOnPath(startCell, target, out Vector2Int goalCell))
        {
            target = goalCell;
        }

        Vector3 start = transform.position;
        Vector3 end = grid.CellToWorld(target);
        float t = 0f;
        float duration = (start - end).magnitude / moveSpeed;
        if (duration <= 0f) duration = 0.01f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        transform.position = end;
        cell = target;
        grid.SetOccupant(cell, gameObject);
        OnMoved?.Invoke(dir);
    }
}
