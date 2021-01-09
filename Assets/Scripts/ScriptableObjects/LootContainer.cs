using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Creates a loot table and determines which one item from it should drop based on a weight system.
/// </summary>
[CreateAssetMenu(fileName = "LootAsset", menuName = "Chochosan/LootTable/LootAsset", order = 1)]
public class LootContainer : ScriptableObject
{
    [Range(0f, 1f)]
    [Tooltip("The chance to drop an item normalized to a [0,1] range.")]
    public float chanceToDropLoot = 0.5f;
    public List<LootItem> lootTable;

    [System.Serializable]
    public class LootItem
    {
        [Tooltip("The prefab that will drop.")]
        public GameObject lootObject;
        [Range(0, 100)]
        [Tooltip("The chance to drop this specific item. Weight should be between 0-100. A higher number results in a higher probability for the item to drop.")]
        public float lootWeight;
    }

    public GameObject DetermineLoot()
    {
        if(IsDropLoot())
        {
            float weightSum = 0f;
            int roll = Random.Range(0, 101);
           // Debug.Log("WEIGHT ROLL IS: " + roll);
            foreach(LootItem lootItem in lootTable)
            {
                weightSum += lootItem.lootWeight;
                if(weightSum >= roll)
                {
                    return lootItem.lootObject;
                }
            }
        }

        return null;
    }

    private bool IsDropLoot()
    {
        if (chanceToDropLoot >= Random.Range(0f, 1f))
            return true;

        return false;
    }
}
