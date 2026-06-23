using System.Collections.Generic;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    public GridManager grid;
    public GameObject wallPrefab;
    public GameObject boundaryWallPrefab;
    public GameObject insideWallPrefab;
    public GameObject boxPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public GameObject goalPrefab;

    public StageData stageData;
    private const float BoundaryWallHeight = 0.5f;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            GenerateStage(StageCatalog.GetStage(StageSelectionManager.GetSelectedStage()));
        }
    }

    public void GenerateDefault3x3()
    {
        GenerateStage(StageCatalog.GetStage(1));
    }

    public void GenerateStage(StageData newStageData)
    {
        if (grid == null)
        {
            grid = FindObjectOfType<GridManager>();
        }
        if (grid == null || newStageData == null)
        {
            return;
        }

        stageData = CloneStageData(newStageData);
        grid.width = stageData.width;
        grid.height = stageData.height;
        grid.ClearGoalCell();
        grid.ClearOccupants();

        // clear existing objects
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            if (go.CompareTag("Wall") || go.CompareTag("Box") || go.CompareTag("Player") || go.CompareTag("Goal") || go.CompareTag("Enemy"))
            {
                DestroyImmediate(go);
            }
        }

        // instantiate
        InstantiateAt(stageData.playerStart, playerPrefab, "Player");
        InstantiateAt(stageData.goalPosition, goalPrefab, "Goal");
        foreach (Vector2Int boxPosition in stageData.boxPositions)
        {
            InstantiateAt(boxPosition, boxPrefab, "Box");
        }

        foreach (EnemyData enemyData in stageData.enemies)
        {
            InstantiateEnemy(enemyData);
        }

        // apply walls
        foreach (Vector2Int w in stageData.wallPositions)
        {
            InstantiateAt(w, GetInsideWallPrefab(), "Wall");
        }

        InstantiateBoundaryWalls();
    }

    private void InstantiateAt(Vector2Int cell, GameObject prefab, string tag)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, grid.CellToWorld(cell), Quaternion.identity, transform);
        go.tag = tag;

        if (tag == "Goal")
        {
            grid.SetGoalCell(cell);
            return;
        }

        var occ = go.GetComponent<BoxController>();
        if (tag == "Box" && occ != null)
        {
            occ.cell = cell;
            occ.grid = grid;
        }

        var player = go.GetComponent<PlayerController>();
        if (tag == "Player" && player != null)
        {
            player.cell = cell;
            player.grid = grid;
        }

        // set grid occupancy
        grid.SetOccupant(cell, go);
    }

    private void InstantiateEnemy(EnemyData enemyData)
    {
        if (enemyPrefab == null) return;
        GameObject go = Instantiate(enemyPrefab, grid.CellToWorld(enemyData.startPosition), Quaternion.identity, transform);
        go.tag = "Enemy";
        var enemy = go.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.cell = enemyData.startPosition;
            enemy.type = enemyData.type;
            enemy.moveDir = enemyData.moveDirection;
            enemy.grid = grid;
        }

        grid.SetOccupant(enemyData.startPosition, go);
    }

    private void InstantiateBoundaryWalls()
    {
        if (GetBoundaryWallPrefab() == null || grid == null)
        {
            return;
        }

        for (int x = -1; x <= grid.width; x++)
        {
            CreateBoundaryWall(new Vector2Int(x, -1));
            CreateBoundaryWall(new Vector2Int(x, grid.height));
        }

        for (int y = 0; y < grid.height; y++)
        {
            CreateBoundaryWall(new Vector2Int(-1, y));
            CreateBoundaryWall(new Vector2Int(grid.width, y));
        }
    }

    private void CreateBoundaryWall(Vector2Int cell)
    {
        var wall = Instantiate(GetBoundaryWallPrefab(), grid.CellToWorld(cell), Quaternion.identity, transform);
        wall.name = $"BoundaryWall_{cell.x}_{cell.y}";
        wall.tag = "Wall";
        wall.transform.localScale = new Vector3(1f, BoundaryWallHeight, 1f);
    }

    private GameObject GetInsideWallPrefab()
    {
        return insideWallPrefab != null ? insideWallPrefab : wallPrefab;
    }

    private GameObject GetBoundaryWallPrefab()
    {
        if (boundaryWallPrefab != null)
        {
            return boundaryWallPrefab;
        }

        return insideWallPrefab != null ? insideWallPrefab : wallPrefab;
    }

    private static StageData CloneStageData(StageData source)
    {
        StageData clone = new StageData();
        clone.stageId = source.stageId;
        clone.width = source.width;
        clone.height = source.height;
        clone.seed = source.seed;
        clone.playerStart = source.playerStart;
        clone.goalPosition = source.goalPosition;
        clone.targetMoveCount = source.targetMoveCount;
        clone.wallPositions.AddRange(source.wallPositions);
        clone.boxPositions.AddRange(source.boxPositions);

        foreach (EnemyData enemy in source.enemies)
        {
            clone.enemies.Add(new EnemyData
            {
                type = enemy.type,
                startPosition = enemy.startPosition,
                moveDirection = enemy.moveDirection
            });
        }

        return clone;
    }
}
