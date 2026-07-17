using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;
    
    public List<EnemyMovement> ennemisActifs = new List<EnemyMovement>();
    public List<Transform> toursActives = new List<Transform>();
    public Transform baseTransform;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (ennemisActifs.Count == 0) return;
        
        NativeArray<EnemyTargetingData> donneesEnnemis = new NativeArray<EnemyTargetingData>(ennemisActifs.Count, Allocator.TempJob);
        NativeArray<TowerTargetingData> donneesTours = new NativeArray<TowerTargetingData>(toursActives.Count, Allocator.TempJob);
        NativeArray<int> resultats = new NativeArray<int>(ennemisActifs.Count, Allocator.TempJob);

        for (int i = 0; i < ennemisActifs.Count; i++)
        {
            donneesEnnemis[i] = new EnemyTargetingData
            {
                id = ennemisActifs[i].gameObject.GetInstanceID(),
                position = ennemisActifs[i].transform.position,
                targetPriority = (int)ennemisActifs[i].data.priority
            };
        }

        for (int i = 0; i < toursActives.Count; i++)
        {
            donneesTours[i] = new TowerTargetingData {
                id = toursActives[i].gameObject.GetInstanceID(),
                position = toursActives[i].transform.position
            };
        }

        FindTargetJob job = new FindTargetJob {
            listeEnnemis = donneesEnnemis,
            listeTours = donneesTours,
            resultats = resultats
        };

        JobHandle handle = job.Schedule(donneesEnnemis.Length, 64);
        handle.Complete();
        
        for (int i = 0; i < ennemisActifs.Count; i++)
        {
            int idCible = resultats[i];
            
            if (idCible != -1)
            {
                Transform cibleTour = toursActives.Find(t => t.gameObject.GetInstanceID() == idCible);
                if (cibleTour != null)
                {
                    ennemisActifs[i].RecevoirNouvelleCible(cibleTour);
                }
            }
            else 
            {
                if (baseTransform != null)
                    ennemisActifs[i].RecevoirNouvelleCible(baseTransform);
            }
        }

        donneesEnnemis.Dispose();
        donneesTours.Dispose();
        resultats.Dispose();
    }
}