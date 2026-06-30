using System.Collections.Generic;
using UnityEngine;

public static class StageCatalog
{
    private static readonly List<StageDefinition> Definitions = CreateDefinitions();

    private sealed class StageDefinition
    {
        public int targetMoves;
        public string[] rows;
        public EnemySpec[] enemies;
    }

    private sealed class EnemySpec
    {
        public Vector2Int cell;
        public Vector2Int moveDirection;
        public EnemyType type;
    }

    public static StageData GetStage(int stageId)
    {
        int clampedId = Mathf.Clamp(stageId, 1, StageSelectionManager.StageCount);
        StageData data = BuildStage(Definitions[clampedId - 1], clampedId);
        return FinalizeStage(data);
    }

    private static StageData BuildStage(StageDefinition definition, int stageId)
    {
        int size = definition.rows.Length;
        StageData data = new StageData
        {
            stageId = stageId,
            width = size,
            height = size,
            targetMoveCount = definition.targetMoves
        };

        Dictionary<Vector2Int, EnemySpec> enemyMap = new Dictionary<Vector2Int, EnemySpec>();
        if (definition.enemies != null)
        {
            foreach (EnemySpec enemy in definition.enemies)
            {
                enemyMap[enemy.cell] = enemy;
            }
        }

        for (int rowIndex = 0; rowIndex < size; rowIndex++)
        {
            string row = definition.rows[rowIndex];
            for (int x = 0; x < size; x++)
            {
                Vector2Int cell = new Vector2Int(x, size - 1 - rowIndex);
                switch (row[x])
                {
                    case '1':
                        data.wallPositions.Add(cell);
                        break;
                    case '2':
                        data.boxPositions.Add(cell);
                        break;
                    case '3':
                        data.goalPosition = cell;
                        break;
                    case '4':
                        data.playerStart = cell;
                        break;
                    case '5':
                    case '6':
                        EnemyType enemyType = row[x] == '6' ? EnemyType.Diagonal : EnemyType.Orthogonal;
                        EnemySpec spec = enemyMap.TryGetValue(cell, out EnemySpec configured)
                            ? configured
                            : CreateEnemy(cell.x, cell.y, enemyType == EnemyType.Diagonal ? new Vector2Int(1, 1) : Vector2Int.left, enemyType);
                        data.enemies.Add(new EnemyData
                        {
                            type = spec.type,
                            startPosition = cell,
                            moveDirection = spec.moveDirection
                        });
                        break;
                }
            }
        }

        return data;
    }

    private static StageData FinalizeStage(StageData source)
    {
        StageData candidate = CloneStageData(source);
        Normalize(candidate);
        if (HasRequiredActors(candidate))
        {
            return candidate;
        }

        StageData fallback = CreateFallbackStage(candidate.width);
        fallback.stageId = source.stageId;
        return fallback;
    }

    private static bool HasRequiredActors(StageData stage)
    {
        return InBounds(stage.playerStart, stage.width, stage.height) &&
               InBounds(stage.goalPosition, stage.width, stage.height);
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

    private static bool InBounds(Vector2Int cell, int width, int height)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private static StageData CloneStageData(StageData source)
    {
        StageData clone = new StageData
        {
            stageId = source.stageId,
            width = source.width,
            height = source.height,
            seed = source.seed,
            playerStart = source.playerStart,
            goalPosition = source.goalPosition,
            targetMoveCount = source.targetMoveCount
        };

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

    private static StageData CreateFallbackStage(int size)
    {
        if (size <= 3)
        {
            return BuildStage(Stage(4, "003", "020", "400"), 1);
        }

        if (size == 4)
        {
            return BuildStage(Stage(7, "0003", "0020", "0100", "4000"), 1);
        }

        return BuildStage(Stage(10, "00030", "00100", "00020", "01000", "40000"), 1);
    }

    private static StageDefinition Stage(int targetMoves, params string[] rows)
    {
        return new StageDefinition
        {
            targetMoves = targetMoves,
            rows = rows,
            enemies = null
        };
    }

    private static StageDefinition Stage(int targetMoves, string[] rows, params EnemySpec[] enemies)
    {
        return new StageDefinition
        {
            targetMoves = targetMoves,
            rows = rows,
            enemies = enemies
        };
    }

    private static EnemySpec CreateEnemy(int x, int y, Vector2Int direction, EnemyType type = EnemyType.Orthogonal)
    {
        return new EnemySpec
        {
            cell = new Vector2Int(x, y),
            moveDirection = direction,
            type = type
        };
    }

    private static List<StageDefinition> CreateDefinitions()
    {
        return new List<StageDefinition>
        {
            // Stage 1
            Stage(3, "003", "000", "400"),
            // Stage 2
            Stage(4, "010", "003", "400"),
            // Stage 3
            Stage(4, "300", "001", "400"),
            // Stage 4
            Stage(5, "003", "020", "400"),
            // Stage 5
            Stage(5, "030", "002", "401"),
            // Stage 6
            Stage(6, "103", "020", "401"),

            // Stage 7
            Stage(5, "0003", "0010", "0000", "4000"),
            // Stage 8
            Stage(5, "0300", "0001", "0000", "4010"),
            // Stage 9
            Stage(6, "0030", "0100", "0001", "4000"),
            // Stage 10
            Stage(6, "0003", "0020", "0100", "4000"),
            // Stage 11
            Stage(7, "0000", "2030", "0100", "4000"),
            // Stage 12
            Stage(7, "0003", "0100", "0020", "4001"),
            // Stage 13
            Stage(7, "0030", "1100", "0200", "4001"),
            // Stage 14
            Stage(8, "0003", "0120", "0001", "4000"),
            // Stage 15
            Stage(8, "0300", "0012", "0100", "4001"),
            // Stage 16
            Stage(8, new[] { "0003", "0510", "0100", "4000" }, CreateEnemy(1, 2, Vector2Int.left)),
            // Stage 17
            Stage(8, new[] { "0300", "0010", "5000", "4020" }, CreateEnemy(0, 1, Vector2Int.right)),
            // Stage 18
            Stage(9, new[] { "0003", "0105", "0200", "4010" }, CreateEnemy(3, 2, Vector2Int.down)),
            // Stage 19
            Stage(9, "0030", "0200", "0012", "4001"),
            // Stage 20
            Stage(10, "0300", "0021", "0102", "4000"),

            // Stage 21
            Stage(7, "00030", "00100", "00020", "01000", "40000"),
            // Stage 22
            Stage(7, "03000", "00010", "00200", "00001", "40000"),
            // Stage 23
            Stage(8, "00003", "01000", "02010", "00000", "40100"),
            // Stage 24
            Stage(8, "00300", "01100", "00020", "01000", "40001"),
            // Stage 25
            Stage(8, "00030", "01010", "02000", "00100", "40001"),
            // Stage 26
            Stage(9, "00000", "00120", "01300", "00010", "42000"),
            // Stage 27
            Stage(9, "00003", "02010", "00100", "01000", "40010"),
            // Stage 28
            Stage(9, new[] { "00300", "01050", "00000", "02010", "40000" }, CreateEnemy(3, 3, Vector2Int.left)),
            // Stage 29
            Stage(9, new[] { "03000", "00100", "50010", "02000", "40001" }, CreateEnemy(0, 2, Vector2Int.right)),
            // Stage 30
            Stage(10, new[] { "00003", "01000", "00150", "02010", "40000" }, CreateEnemy(3, 2, Vector2Int.up)),
            // Stage 31
            Stage(10, new[] { "00300", "00010", "02100", "00005", "41000" }, CreateEnemy(4, 1, Vector2Int.left)),
            // Stage 32
            Stage(10, "00030", "01200", "00020", "00100", "42001"),
            // Stage 33
            Stage(11, "03000", "00012", "01000", "00200", "40010"),
            // Stage 34
            Stage(11, "00300", "02010", "00120", "00000", "41000"),
            // Stage 35
            Stage(11, "00003", "01020", "00010", "00200", "40001"),
            // Stage 36
            Stage(12, "03000", "00100", "02001", "01020", "40000"),
            // Stage 37
            Stage(12, new[] { "00300", "01050", "02000", "00120", "40000" }, CreateEnemy(3, 3, Vector2Int.left)),
            // Stage 38
            Stage(12, new[] { "03000", "00100", "02060", "00010", "40002" }, CreateEnemy(3, 2, new Vector2Int(-1, -1), EnemyType.Diagonal)),
            // Stage 39
            Stage(12, new[] { "00003", "01200", "05010", "00020", "40000" }, CreateEnemy(1, 2, Vector2Int.right)),
            // Stage 40
            Stage(13, new[] { "00300", "00010", "02060", "00100", "41020" }, CreateEnemy(3, 2, new Vector2Int(-1, 1), EnemyType.Diagonal)),
            // Stage 41
            Stage(13, new[] { "03000", "01020", "00050", "00200", "40001" }, CreateEnemy(3, 2, Vector2Int.left)),
            // Stage 42
            Stage(13, new[] { "00003", "00120", "02010", "06000", "40010" }, CreateEnemy(0, 1, new Vector2Int(1, 1), EnemyType.Diagonal)),

            // Stage 43
            Stage(14, new[] { "00300", "01020", "05010", "00020", "40001" }, CreateEnemy(1, 2, Vector2Int.right)),
            // Stage 44
            Stage(14, new[] { "00030", "02100", "00150", "00020", "41000" }, CreateEnemy(3, 2, Vector2Int.up)),
            // Stage 45
            Stage(15, new[] { "03000", "00210", "05000", "00120", "40001" }, CreateEnemy(1, 2, Vector2Int.right)),
            // Stage 46
            Stage(15, new[] { "00300", "01060", "02000", "00120", "40001" }, CreateEnemy(3, 3, new Vector2Int(-1, -1), EnemyType.Diagonal)),
            // Stage 47
            Stage(15, new[] { "00003", "02010", "00150", "01020", "40000" }, CreateEnemy(3, 2, Vector2Int.left)),
            // Stage 48
            Stage(16, new[] { "03000", "00120", "06010", "02000", "40001" }, CreateEnemy(1, 2, new Vector2Int(1, 1), EnemyType.Diagonal)),
            // Stage 49
            Stage(16, new[] { "00300", "01020", "00050", "02100", "40001" }, CreateEnemy(3, 2, Vector2Int.down)),
            // Stage 50
            Stage(17, new[] { "00003", "01200", "05020", "00100", "42001" }, CreateEnemy(1, 2, Vector2Int.right))
        };
    }
}
