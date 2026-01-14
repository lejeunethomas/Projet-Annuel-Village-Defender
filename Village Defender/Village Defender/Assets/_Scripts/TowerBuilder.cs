using UnityEngine;

public class TowerBuilder : MonoBehaviour
{
    public GameObject towerPrefab;
    public int TowerCost = 15;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryBuild();
        }
    }


    void TryBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if (GameManager.Instance.gold >= TowerCost)
            {
                if(hit.collider.gameObject.GetComponent<TowerCombat>() == null)
                {
                    BuildTower(hit.point);
                }
            }
            else
            {
                Debug.Log("Pas assez d'or, Il faut" + TowerCost);
            }
        }
        else
        {
            Debug.Log("Le rayon n'a RIEN touché ! Vérifie le Collider du sol."); // Mouchard 3
        }
    }

    void BuildTower(Vector3 position)
    {
        Vector3 spawnposition = position;
        spawnposition.y = 0.5f;

        Instantiate(towerPrefab,spawnposition,Quaternion.identity);

        GameManager.Instance.gold -= TowerCost;
    }
}
