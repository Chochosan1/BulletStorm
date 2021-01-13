using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralGeneration
{
    /// <summary>
    /// Configuration settings for each prefab that is used to spawn at a world cell.
    /// </summary>
    public class ProceduralCellConfiguration : MonoBehaviour
    {  
        [Header("Settings")]
        [Tooltip("The type of this prefab. The same type can be assigned to more than one prefab. NOTE: Placement limit is tied to prefab type, not only the current prefab object.")]
        [SerializeField] private PrefabType prefabType;

        [SerializeField] private PlacementLimit placementLimitType;
        [Tooltip("Maximum amount of times the current PREFAB TYPE will get spawned at the world.")]
        [SerializeField] private int placementLimitNumber = 0;
        [Tooltip("Should the object have a random Y rotation on spawn?")]
        [SerializeField] private bool isRotateRandomly = true;

        [Header("References")]
        [SerializeField] private GameObject[] objectsToRandomlyToggle;
        [SerializeField] private float disableObjectsChance = 0.6f;
        [SerializeField] private GameObject[] bigUpgradeOrbs;
        [SerializeField] private float enableBigOrbsChance = 0.1f;

        private int[] possibleY_Rotations = { 0, 90, 180, 270 };

        private void Start()
        {
            RandomlyToggleObjects();
            DetermineObjectRotation();
            ToggleBigUpgradeOrbs();

            Destroy(this);
        }

        private void RandomlyToggleObjects()
        {
            if (objectsToRandomlyToggle.Length <= 0)
                return; 

            foreach(GameObject GO in objectsToRandomlyToggle)
            {
                float chanceRolled = Random.Range(0f, 1f);

                if (chanceRolled >= disableObjectsChance)
                {
                    GO.SetActive(false);
                }
            }
            
        }

        private void DetermineObjectRotation()
        {
            if (!isRotateRandomly)
                return;

            int rotationChosen = Random.Range(0, possibleY_Rotations.Length);
            gameObject.transform.Rotate(transform.up, possibleY_Rotations[rotationChosen]);
        }

        private void ToggleBigUpgradeOrbs()
        {
            foreach(GameObject orb in bigUpgradeOrbs)
            {
                float chanceRolled = Random.Range(0f, 1f);

                if (chanceRolled >= enableBigOrbsChance)
                {
                    orb.SetActive(false);
                }
            }
        }

        #region Getters
        ///<summary>Maximum amount of prefabs of this type allowed to place.</summary>
        public int GetPlacementLimitNumber()
        {
            return placementLimitNumber;
        }

        ///<summary>The current prefab's type.</summary>
        public PrefabType GetPrefabType()
        {
            return prefabType;
        }

        ///<summary>The current prefab's limit type.</summary>
        public PlacementLimit GetPlacementLimitType()
        {
            return placementLimitType;
        }
        #endregion
    }
}
