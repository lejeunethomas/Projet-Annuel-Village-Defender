using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FindTargetJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<EnemyTargetingData> listeEnnemis;
    [ReadOnly] public NativeArray<TowerTargetingData> listeTours;
    public NativeArray<int> resultats;
    public void Execute(int index)
    {
        EnemyTargetingData ennemi = listeEnnemis[index];
        int target = -1; 
        
        if (ennemi.targetPriority == 1) 
        { 
            float rangemin = float.MaxValue; 
            for(int i = 0; i < listeTours.Length; i++) 
            { 
                TowerTargetingData tour = listeTours[i];
                float range = math.distancesq(ennemi.position, tour.position); 
                if (range < rangemin) 
                { 
                    rangemin = range; 
                    target = tour.id;
                }
            }
        }
        resultats[index] = target;
    }
}
