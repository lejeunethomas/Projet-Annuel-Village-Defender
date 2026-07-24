using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewWave", menuName = "VillageDefender/WaveData")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class EnemyGroup
    {
        public EnemyData enemyType;
        public int count;
        public float rate;
    }

    public List<EnemyGroup> groups;

    [FormerlySerializedAs("Boss")]
    public bool boss = false;
}
