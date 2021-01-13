using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ProceduralGeneration
{
    /// <summary>
    /// Attached to a cell prefab in order to check out its dimensions and adjust accordingly.
    /// </summary>
    public class ProceduralCellHelper : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showCellDimensions = true;
        [SerializeField] private Vector3 cellDimensions;

        private void OnDrawGizmos()
        {
            if (!showCellDimensions)
                return;

            Gizmos.DrawCube(transform.position, cellDimensions);
        }
    }
}
