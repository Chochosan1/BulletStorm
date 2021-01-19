using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralGeneration
{
    public enum PrefabType { Houses1, StonePark1, Church1, Guillotine }
    public enum PlacementLimit { Unlimited, Limited, DontPlace };
    /// <summary>
    /// Controls what prefab goes to each world cell.
    /// </summary>
    public class ProceduralGeneration : MonoBehaviour
    {
        /// <summary> A static dictionary to keep track of all placed prefabTypes and how many times they have been placed.</summary>
        public static Dictionary<PrefabType, int> prefabTypeTimesPlaced;

        [Tooltip("All world cells. Prefabs will be spawned on top of them to fill the world procedurally.")]
        [SerializeField] private GameObject[] cells;
        [Tooltip("All prefabs used to spawn at the world. Each cell will be assigned one of these randomly.")]
        [SerializeField] private GameObject[] prefabsToSpawn;

        [SerializeField] private GameObject startCell;
        [SerializeField] private GameObject levelEndCell;

        [SerializeField] private GameObject[] startPrefabsToSpawn;
        [SerializeField] private GameObject[] levelEndPrefabsToSpawn;

        [SerializeField] private GameObject[] bossesToSpawn;

        void Start()
        {
            prefabTypeTimesPlaced = new Dictionary<PrefabType, int>();

            GenerateNormalCells();
            GenerateStartCell();
            GenerateEndCell();
        }

        private void GenerateEndCell()
        {
            int prefabChosen = Random.Range(0, levelEndPrefabsToSpawn.Length);

            Instantiate(levelEndPrefabsToSpawn[prefabChosen], levelEndCell.transform.position, levelEndPrefabsToSpawn[prefabChosen].transform.rotation);
        }

        private void GenerateStartCell()
        {
            int prefabChosen = Random.Range(0, startPrefabsToSpawn.Length);

            Instantiate(startPrefabsToSpawn[prefabChosen], startCell.transform.position, startPrefabsToSpawn[prefabChosen].transform.rotation);
        }

        private void GenerateNormalCells()
        {
            foreach (GameObject cell in cells)
            {
                int prefabChosen = Random.Range(0, prefabsToSpawn.Length);

                for (int i = 0; i < prefabsToSpawn.Length; i++)
                {
                    ProceduralCellConfiguration pccCache = prefabsToSpawn[prefabChosen].GetComponent<ProceduralCellConfiguration>();
                    if (!CanPrefabBePlaced(pccCache.GetPrefabType(), pccCache.GetPlacementLimitNumber(), pccCache.GetPlacementLimitType()))
                    {
                        prefabChosen++;

                        if (prefabChosen >= prefabsToSpawn.Length)
                            prefabChosen = 0;
                    }
                    else
                    {
                        Instantiate(prefabsToSpawn[prefabChosen], cell.transform.position, prefabsToSpawn[prefabChosen].transform.rotation);
                        break;
                    }

                }
            }
        }

        /// <summary>
        /// Will check if the current type can be used to spawn at the current cell.
        /// Parameters should be taken from <see cref="ProceduralCellConfiguration"/>
        /// </summary>
        private bool CanPrefabBePlaced(PrefabType prefabType, int prefabPlacementLimit, PlacementLimit placementLimitType)
        {
            if (prefabTypeTimesPlaced.ContainsKey(prefabType))
            {
                if (placementLimitType == PlacementLimit.Limited && prefabTypeTimesPlaced[prefabType] >= prefabPlacementLimit)
                {
                //    Debug.Log("PREFAB TYPE " + prefabType + " CAN NOT BE PLACED. LIMIT REACHED!");
                    return false;
                }

                prefabTypeTimesPlaced[prefabType]++;
              //  Debug.Log($"PREFAB TYPE {prefabType} BEING PLACED. ({prefabTypeTimesPlaced[prefabType]})");
            }
            else
            {
                if (placementLimitType == PlacementLimit.DontPlace)
                    return false;

                prefabTypeTimesPlaced.Add(prefabType, 1);
            //    Debug.Log("PREFAB TYPE " + prefabType + " ADDED TO THE DICTIONARY.");
            }

            return true;
        }
    }
}
