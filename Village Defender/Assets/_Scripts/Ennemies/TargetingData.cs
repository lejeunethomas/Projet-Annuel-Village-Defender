using UnityEngine;
using Unity.Mathematics;

public struct EnemyTargetingData
{
    public int id;
    public float3 position;
    public int targetPriority;
}

public struct TowerTargetingData
{
    public int id;
    public float3 position;
}

public struct ResultTarget
{
    public int idTarget;
}
