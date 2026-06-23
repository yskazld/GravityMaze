using System.Collections.Generic;
using UnityEngine;

public static class StageCatalog
{
    public static StageData GetStage(int stageId)
    {
        int clampedId = Mathf.Clamp(stageId, 1, StageSelectionManager.StageCount);
        StageData data = BuildStage(clampedId);
        data.stageId = clampedId;
        return EnsurePlayable(data);
    }

    private static StageData BuildStage(int stageId)
    {
        if (stageId <= 10)
        {
            return BuildFromTemplates(stageId, 3, Create3x3Templates(), 2);
        }

        if (stageId <= 30)
        {
            return BuildFromTemplates(stageId - 10, 4, Create4x4Templates(), 2);
        }

        return BuildFromTemplates(stageId - 30, 5, Create5x5Templates(), 2);
    }

    private static StageData BuildFromTemplates(int localIndex, int size, List<StageData> templates, int variantsPerTemplate)
    {
        int templateIndex = (localIndex - 1) / variantsPerTemplate;
        int variantIndex = (localIndex - 1) % variantsPerTemplate;
        StageData baseData = CloneStageData(templates[templateIndex]);
        ApplyVariant(baseData, size, variantIndex);
        baseData.width = size;
        baseData.height = size;
        return baseData;
    }

    private static void ApplyVariant(StageData stage, int size, int variantIndex)
    {
        bool mirrorX = (variantIndex & 1) == 1;
        int rotations = variantIndex / 2;

        if (mirrorX)
        {
            stage.playerStart = MirrorX(stage.playerStart, size);
            stage.goalPosition = MirrorX(stage.goalPosition, size);
            TransformPositions(stage.wallPositions, size, MirrorX);
            TransformPositions(stage.boxPositions, size, MirrorX);
            foreach (EnemyData enemy in stage.enemies)
            {
                enemy.startPosition = MirrorX(enemy.startPosition, size);
                enemy.moveDirection = new Vector2Int(-enemy.moveDirection.x, enemy.moveDirection.y);
            }
        }

        for (int i = 0; i < rotations; i++)
        {
            stage.playerStart = Rotate90(stage.playerStart, size);
            stage.goalPosition = Rotate90(stage.goalPosition, size);
            TransformPositions(stage.wallPositions, size, Rotate90);
            TransformPositions(stage.boxPositions, size, Rotate90);
            foreach (EnemyData enemy in stage.enemies)
            {
                enemy.startPosition = Rotate90(enemy.startPosition, size);
                enemy.moveDirection = Rotate90Direction(enemy.moveDirection);
            }
        }
    }

    private static void TransformPositions(List<Vector2Int> positions, int size, System.Func<Vector2Int, int, Vector2Int> transform)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            positions[i] = transform(positions[i], size);
        }
    }

    private static Vector2Int MirrorX(Vector2Int cell, int size)
    {
        return new Vector2Int(size - 1 - cell.x, cell.y);
    }

    private static Vector2Int Rotate90(Vector2Int cell, int size)
    {
        return new Vector2Int(size - 1 - cell.y, cell.x);
    }

    private static Vector2Int Rotate90Direction(Vector2Int direction)
    {
        return new Vector2Int(-direction.y, direction.x);
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

        foreach (Vector2Int wall in source.wallPositions)
        {
            clone.wallPositions.Add(wall);
        }

        foreach (Vector2Int box in source.boxPositions)
        {
            clone.boxPositions.Add(box);
        }

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

    private static List<StageData> Create3x3Templates()
    {
        return new List<StageData>
        {
            CreateStageTemplate(3, new Vector2Int(0, 0), new Vector2Int(2, 2), null, new[] { new Vector2Int(1, 0) }, null, 4),
            CreateStageTemplate(3, new Vector2Int(0, 1), new Vector2Int(2, 2), new[] { new Vector2Int(1, 0) }, new[] { new Vector2Int(2, 0) }, null, 5),
            CreateStageTemplate(3, new Vector2Int(0, 0), new Vector2Int(1, 2), new[] { new Vector2Int(1, 1) }, new[] { new Vector2Int(2, 0), new Vector2Int(2, 1) }, null, 5),
            CreateStageTemplate(3, new Vector2Int(1, 0), new Vector2Int(2, 2), new[] { new Vector2Int(0, 1), new Vector2Int(1, 1) }, new[] { new Vector2Int(2, 0) }, null, 6),
            CreateStageTemplate(3, new Vector2Int(0, 2), new Vector2Int(2, 1), new[] { new Vector2Int(1, 0) }, new[] { new Vector2Int(0, 1), new Vector2Int(2, 2) }, null, 5)
        };
    }

    private static List<StageData> Create4x4Templates()
    {
        return new List<StageData>
        {
            CreateStageTemplate(4, new Vector2Int(0, 0), new Vector2Int(3, 3), new[] { new Vector2Int(1, 1), new Vector2Int(2, 1) }, new[] { new Vector2Int(3, 0), new Vector2Int(0, 2), new Vector2Int(2, 3) }, null, 7),
            CreateStageTemplate(4, new Vector2Int(0, 1), new Vector2Int(3, 2), new[] { new Vector2Int(1, 0), new Vector2Int(1, 2), new Vector2Int(2, 2) }, new[] { new Vector2Int(3, 0), new Vector2Int(0, 3), new Vector2Int(2, 0), new Vector2Int(3, 1) }, null, 8),
            CreateStageTemplate(4, new Vector2Int(1, 0), new Vector2Int(3, 3), new[] { new Vector2Int(0, 2), new Vector2Int(2, 1) }, new[] { new Vector2Int(0, 1), new Vector2Int(3, 0), new Vector2Int(1, 3) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(3, 1), Vector2Int.left) }, 8),
            CreateStageTemplate(4, new Vector2Int(0, 0), new Vector2Int(2, 3), new[] { new Vector2Int(1, 1), new Vector2Int(3, 1), new Vector2Int(2, 2), new Vector2Int(0, 2) }, new[] { new Vector2Int(3, 0), new Vector2Int(2, 0), new Vector2Int(1, 3), new Vector2Int(3, 2) }, null, 9),
            CreateStageTemplate(4, new Vector2Int(1, 1), new Vector2Int(3, 3), new[] { new Vector2Int(0, 2), new Vector2Int(1, 3), new Vector2Int(2, 1) }, new[] { new Vector2Int(3, 0), new Vector2Int(0, 0), new Vector2Int(2, 3) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(3, 1), Vector2Int.down) }, 8),
            CreateStageTemplate(4, new Vector2Int(0, 2), new Vector2Int(3, 0), new[] { new Vector2Int(1, 1), new Vector2Int(2, 2), new Vector2Int(0, 1) }, new[] { new Vector2Int(1, 0), new Vector2Int(3, 2), new Vector2Int(2, 3), new Vector2Int(0, 3) }, null, 8),
            CreateStageTemplate(4, new Vector2Int(1, 0), new Vector2Int(2, 3), new[] { new Vector2Int(0, 1), new Vector2Int(2, 1), new Vector2Int(3, 2) }, new[] { new Vector2Int(3, 0), new Vector2Int(0, 2), new Vector2Int(1, 3) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(2, 2), Vector2Int.left) }, 8),
            CreateStageTemplate(4, new Vector2Int(0, 0), new Vector2Int(3, 2), new[] { new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(2, 3) }, new[] { new Vector2Int(1, 0), new Vector2Int(3, 0), new Vector2Int(0, 3), new Vector2Int(3, 3) }, null, 9),
            CreateStageTemplate(4, new Vector2Int(1, 1), new Vector2Int(3, 3), new[] { new Vector2Int(0, 2), new Vector2Int(2, 2), new Vector2Int(3, 1), new Vector2Int(1, 3) }, new[] { new Vector2Int(0, 0), new Vector2Int(2, 0), new Vector2Int(3, 2) }, null, 9),
            CreateStageTemplate(4, new Vector2Int(0, 1), new Vector2Int(2, 3), new[] { new Vector2Int(1, 0), new Vector2Int(2, 2), new Vector2Int(3, 1) }, new[] { new Vector2Int(0, 3), new Vector2Int(1, 2), new Vector2Int(3, 0), new Vector2Int(2, 0) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(3, 3), Vector2Int.left) }, 9)
        };
    }

    private static List<StageData> Create5x5Templates()
    {
        return new List<StageData>
        {
            CreateStageTemplate(5, new Vector2Int(0, 0), new Vector2Int(4, 4), new[] { new Vector2Int(1, 1), new Vector2Int(3, 1), new Vector2Int(2, 2), new Vector2Int(1, 3) }, new[] { new Vector2Int(4, 0), new Vector2Int(0, 3), new Vector2Int(3, 4), new Vector2Int(2, 0) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 2), Vector2Int.left) }, 10),
            CreateStageTemplate(5, new Vector2Int(0, 1), new Vector2Int(4, 3), new[] { new Vector2Int(1, 0), new Vector2Int(1, 2), new Vector2Int(3, 2), new Vector2Int(2, 3), new Vector2Int(4, 1) }, new[] { new Vector2Int(4, 0), new Vector2Int(0, 4), new Vector2Int(2, 0), new Vector2Int(3, 4), new Vector2Int(1, 4) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(3, 1), Vector2Int.down) }, 10),
            CreateStageTemplate(5, new Vector2Int(1, 0), new Vector2Int(4, 4), new[] { new Vector2Int(0, 2), new Vector2Int(2, 1), new Vector2Int(4, 2), new Vector2Int(2, 3) }, new[] { new Vector2Int(0, 1), new Vector2Int(4, 0), new Vector2Int(1, 3), new Vector2Int(3, 4), new Vector2Int(4, 3) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(2, 4), Vector2Int.left), CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 1), Vector2Int.down) }, 11),
            CreateStageTemplate(5, new Vector2Int(0, 0), new Vector2Int(3, 4), new[] { new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(4, 1), new Vector2Int(0, 3), new Vector2Int(3, 2) }, new[] { new Vector2Int(4, 0), new Vector2Int(1, 2), new Vector2Int(2, 4), new Vector2Int(3, 0), new Vector2Int(4, 3), new Vector2Int(0, 4) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 2), Vector2Int.up) }, 11),
            CreateStageTemplate(5, new Vector2Int(1, 1), new Vector2Int(4, 4), new[] { new Vector2Int(0, 2), new Vector2Int(1, 3), new Vector2Int(3, 1), new Vector2Int(4, 2) }, new[] { new Vector2Int(0, 0), new Vector2Int(2, 0), new Vector2Int(4, 0), new Vector2Int(1, 4), new Vector2Int(2, 4), new Vector2Int(0, 4) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 1), Vector2Int.down), CreateEnemy(EnemyType.Orthogonal, new Vector2Int(3, 4), Vector2Int.left) }, 12),
            CreateStageTemplate(5, new Vector2Int(0, 2), new Vector2Int(4, 1), new[] { new Vector2Int(1, 1), new Vector2Int(2, 2), new Vector2Int(3, 3), new Vector2Int(1, 4) }, new[] { new Vector2Int(0, 4), new Vector2Int(2, 0), new Vector2Int(4, 0), new Vector2Int(3, 1), new Vector2Int(4, 4) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(2, 4), Vector2Int.left) }, 11),
            CreateStageTemplate(5, new Vector2Int(1, 0), new Vector2Int(3, 4), new[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(3, 1), new Vector2Int(4, 3), new Vector2Int(2, 3) }, new[] { new Vector2Int(0, 1), new Vector2Int(2, 0), new Vector2Int(4, 1), new Vector2Int(1, 4), new Vector2Int(3, 4) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 2), Vector2Int.down), CreateEnemy(EnemyType.Orthogonal, new Vector2Int(3, 0), Vector2Int.left) }, 12),
            CreateStageTemplate(5, new Vector2Int(0, 0), new Vector2Int(4, 3), new[] { new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(3, 2), new Vector2Int(2, 4) }, new[] { new Vector2Int(1, 0), new Vector2Int(4, 0), new Vector2Int(0, 3), new Vector2Int(3, 4), new Vector2Int(4, 2), new Vector2Int(0, 4) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(2, 3), Vector2Int.left) }, 11),
            CreateStageTemplate(5, new Vector2Int(1, 1), new Vector2Int(4, 4), new[] { new Vector2Int(0, 3), new Vector2Int(2, 2), new Vector2Int(3, 1), new Vector2Int(4, 2), new Vector2Int(1, 4) }, new[] { new Vector2Int(0, 0), new Vector2Int(2, 0), new Vector2Int(3, 3), new Vector2Int(4, 0), new Vector2Int(0, 2) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 1), Vector2Int.down), CreateEnemy(EnemyType.Orthogonal, new Vector2Int(2, 4), Vector2Int.left) }, 12),
            CreateStageTemplate(5, new Vector2Int(0, 1), new Vector2Int(3, 4), new[] { new Vector2Int(1, 0), new Vector2Int(2, 2), new Vector2Int(4, 2), new Vector2Int(1, 3) }, new[] { new Vector2Int(0, 4), new Vector2Int(2, 0), new Vector2Int(3, 1), new Vector2Int(4, 4), new Vector2Int(1, 4), new Vector2Int(4, 0) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(3, 2), Vector2Int.down) }, 11),
            CreateStageTemplate(5, new Vector2Int(2, 0), new Vector2Int(4, 4), new[] { new Vector2Int(0, 2), new Vector2Int(1, 3), new Vector2Int(3, 2), new Vector2Int(4, 1), new Vector2Int(2, 3) }, new[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(4, 0), new Vector2Int(3, 4), new Vector2Int(4, 3) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(2, 4), Vector2Int.left), CreateEnemy(EnemyType.Orthogonal, new Vector2Int(0, 4), Vector2Int.right) }, 12),
            CreateStageTemplate(5, new Vector2Int(0, 0), new Vector2Int(4, 2), new[] { new Vector2Int(1, 1), new Vector2Int(2, 3), new Vector2Int(3, 1), new Vector2Int(1, 4) }, new[] { new Vector2Int(2, 0), new Vector2Int(4, 0), new Vector2Int(0, 3), new Vector2Int(3, 4), new Vector2Int(4, 4), new Vector2Int(1, 2) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 1), Vector2Int.down) }, 11)
        };
    }

    private static StageData CreateStageTemplate(int size, Vector2Int player, Vector2Int goal, Vector2Int[] walls, Vector2Int[] boxes, EnemyData[] enemies, int targetMoves)
    {
        StageData data = new StageData();
        data.width = size;
        data.height = size;
        data.playerStart = player;
        data.goalPosition = goal;
        data.targetMoveCount = targetMoves;

        if (walls != null)
        {
            data.wallPositions.AddRange(walls);
        }

        if (boxes != null)
        {
            data.boxPositions.AddRange(boxes);
        }

        if (enemies != null)
        {
            data.enemies.AddRange(enemies);
        }

        return data;
    }

    private static EnemyData CreateEnemy(EnemyType type, Vector2Int start, Vector2Int direction)
    {
        return new EnemyData
        {
            type = type,
            startPosition = start,
            moveDirection = direction
        };
    }

    private static StageData EnsurePlayable(StageData source)
    {
        StageData candidate = CloneStageData(source);
        Normalize(candidate);
        if (IsPlayable(candidate))
        {
            return candidate;
        }

        StageData fallback = CreateFallbackStage(candidate.width);
        fallback.stageId = source.stageId;
        return fallback;
    }

    private static void Normalize(StageData stage)
    {
        HashSet<Vector2Int> used = new HashSet<Vector2Int> { stage.playerStart, stage.goalPosition };

        stage.wallPositions = FilterCells(stage.wallPositions, stage.width, stage.height, used);
        used.UnionWith(stage.wallPositions);

        stage.boxPositions = FilterCells(stage.boxPositions, stage.width, stage.height, used);
        used.UnionWith(stage.boxPositions);

        List<EnemyData> filteredEnemies = new List<EnemyData>();
        foreach (EnemyData enemy in stage.enemies)
        {
            if (!InBounds(enemy.startPosition, stage.width, stage.height) || used.Contains(enemy.startPosition))
            {
                continue;
            }

            filteredEnemies.Add(enemy);
            used.Add(enemy.startPosition);
        }

        stage.enemies = filteredEnemies;
    }

    private static List<Vector2Int> FilterCells(List<Vector2Int> cells, int width, int height, HashSet<Vector2Int> used)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (Vector2Int cell in cells)
        {
            if (!InBounds(cell, width, height) || used.Contains(cell))
            {
                continue;
            }

            result.Add(cell);
            used.Add(cell);
        }

        return result;
    }

    private static bool IsPlayable(StageData stage)
    {
        if (!InBounds(stage.playerStart, stage.width, stage.height) || !InBounds(stage.goalPosition, stage.width, stage.height))
        {
            return false;
        }

        HashSet<Vector2Int> blocked = new HashSet<Vector2Int>(stage.wallPositions);
        foreach (Vector2Int box in stage.boxPositions)
        {
            blocked.Add(box);
        }
        foreach (EnemyData enemy in stage.enemies)
        {
            blocked.Add(enemy.startPosition);
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(stage.playerStart);
        visited.Add(stage.playerStart);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == stage.goalPosition)
            {
                return true;
            }

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int end = Slide(current, direction, stage.width, stage.height, blocked, stage.goalPosition);
                if (visited.Add(end))
                {
                    queue.Enqueue(end);
                }
            }
        }

        return false;
    }

    private static Vector2Int Slide(Vector2Int start, Vector2Int direction, int width, int height, HashSet<Vector2Int> blocked, Vector2Int goal)
    {
        Vector2Int current = start;
        while (true)
        {
            Vector2Int next = current + direction;
            if (!InBounds(next, width, height))
            {
                return current;
            }

            if (next == goal)
            {
                return next;
            }

            if (blocked.Contains(next))
            {
                return current;
            }

            current = next;
        }
    }

    private static bool InBounds(Vector2Int cell, int width, int height)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private static StageData CreateFallbackStage(int size)
    {
        if (size <= 3)
        {
            return CreateStageTemplate(3, new Vector2Int(0, 0), new Vector2Int(2, 2), null, new[] { new Vector2Int(2, 0) }, null, 4);
        }

        if (size == 4)
        {
            return CreateStageTemplate(4, new Vector2Int(0, 0), new Vector2Int(3, 3), new[] { new Vector2Int(1, 1), new Vector2Int(2, 1) }, new[] { new Vector2Int(3, 0), new Vector2Int(0, 2), new Vector2Int(2, 3) }, null, 7);
        }

        return CreateStageTemplate(5, new Vector2Int(0, 0), new Vector2Int(4, 4), new[] { new Vector2Int(1, 1), new Vector2Int(3, 1) }, new[] { new Vector2Int(4, 0), new Vector2Int(0, 3), new Vector2Int(3, 4), new Vector2Int(2, 0) }, new[] { CreateEnemy(EnemyType.Orthogonal, new Vector2Int(4, 2), Vector2Int.left) }, 10);
    }

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
}
