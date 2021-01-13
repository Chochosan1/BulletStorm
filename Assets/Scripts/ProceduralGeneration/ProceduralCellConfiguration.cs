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
        [Tooltip("The type of this prefab. The same type can be assigned to more than one prefab. NOTE: Placement limit is tied to prefab type, not only the current prefab object.")]
        [SerializeField] private PrefabType prefabType;

        [SerializeField] private PlacementLimit placementLimitType;
        [Tooltip("Maximum amount of times the current PREFAB TYPE will get spawned at the world.")]
        [SerializeField] private int placementLimitNumber = 0;

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
    }
}
