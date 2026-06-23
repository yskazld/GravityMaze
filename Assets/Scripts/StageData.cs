using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public EnemyType type;
    public Vector2Int startPosition;
    public Vector2Int moveDirection;
}

public enum EnemyType { Orthogonal, Diagonal }

[System.Serializable]
public class StageData
{
    public int stageId;
    public int width = 3;
    public int height = 3;
    public int seed;
    public Vector2Int playerStart;
    public Vector2Int goalPosition;
    public List<Vector2Int> wallPositions = new List<Vector2Int>();
    public List<Vector2Int> boxPositions = new List<Vector2Int>();
    public List<EnemyData> enemies = new List<EnemyData>();
    public int targetMoveCount = 5;
}
