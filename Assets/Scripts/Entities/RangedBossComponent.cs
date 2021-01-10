using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(RangedEnemy))]
public class RangedBossComponent : MonoBehaviour
{
    public enum BossAbilities { None, SpawnUnits, Dash }

    [SerializeField] private BossAbilities firstBossAbility;
    [SerializeField] private BossAbilities secondBossAbility;
    [SerializeField] private BossAbilities thirdBossAbility;

    [SerializeField] private float abilityCooldown = 5f;
    private float abilityTimestamp;

    [SerializeField] private SpawnUnitsAbility spawnUnitsAbility;

    private RangedEnemy rangedEnemyComponent;

    private void Awake()
    {
        Chochosan.CustomEventManager.OnBossKilled += OnBossKilled;
    }

    private void OnDisable()
    {
        Chochosan.CustomEventManager.OnBossKilled -= OnBossKilled;
    }

    private void Start()
    {
        rangedEnemyComponent = GetComponent<RangedEnemy>();

        rangedEnemyComponent.SetEnemyAsBoss();
        this.gameObject.transform.localScale *= 1.5f;
    }

    private void Update()
    {
        if(Time.time >= abilityTimestamp)
        {
            //cast spell
            abilityTimestamp = Time.time + abilityCooldown;

            int spellToCast = Random.Range(1, 4);

            DetermineSpellToCast(spellToCast);
        }
    }

    private void DetermineSpellToCast(int spellToCast)
    {
        switch(spellToCast)
        {
            case 1:
                CastSpell(firstBossAbility);
                break;
            case 2:
                CastSpell(secondBossAbility);
                break;
            case 3:
                CastSpell(thirdBossAbility);
                break;
        }
    }

    private void CastSpell(BossAbilities abilityToCast)
    {
        switch(abilityToCast)
        {
            case BossAbilities.SpawnUnits:
                CastSpawnUnits();
                break;
        }
    }

    private void CastSpawnUnits()
    {
        for(int i = 0; i < spawnUnitsAbility.unitSpawnPoints.Count; i++)
        {
            Instantiate(spawnUnitsAbility.unitToSpawn, spawnUnitsAbility.unitSpawnPoints[i].transform.position, spawnUnitsAbility.unitToSpawn.transform.rotation);
        }
    }

    private void OnBossKilled()
    {
        Debug.Log("BOSS SLAIN! PORTAL UNLOCKED");
    }

    [System.Serializable]
    class SpawnUnitsAbility
    {
        public List<GameObject> unitSpawnPoints;

        public GameObject unitToSpawn;
    }
}
