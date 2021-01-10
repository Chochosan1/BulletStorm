using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(RangedEnemy))]
public class RangedBossComponent : MonoBehaviour
{
    public enum BossAbilities { None, SpawnUnits, MarkedGroundProjectile, Dash }

    [Header("Abilities")]
    [SerializeField] private BossAbilities firstBossAbility;
    [SerializeField] private BossAbilities secondBossAbility;
    [SerializeField] private BossAbilities thirdBossAbility;

    [Header("References")]
    [SerializeField] private GameObject castSpellParticle;


    [Header("Properties")]
    [SerializeField] private float abilityCooldown = 5f;
    [SerializeField] private int maxNumberOfAbilitiesToCast = 10;
    private float abilityTimestamp;

    [Header("Abilities settings")]
    [SerializeField] private SpawnUnitsAbility spawnUnitsAbility;
    [SerializeField] private MarkedGroundProjectile markedGroundProjectile;
    [SerializeField] private DashAbility dashAbility;

    private RangedEnemy rangedEnemyComponent;
    private Collider thisColl;
    private bool isSpellsGoSequentially = true;
    private int currentSpellToCastSequentially = 0;

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
        thisColl = GetComponent<Collider>();
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

            int spellToCast = 1;

            //first choose the spells sequentially one after another, after casting each spell once then start rotating them randomly
            if(isSpellsGoSequentially)
            {
                currentSpellToCastSequentially++;
                spellToCast = currentSpellToCastSequentially;

                //choose random spell if choosing sequentially has already chosen 3 spells
                if (currentSpellToCastSequentially > 3)
                {
                    isSpellsGoSequentially = false;
                    spellToCast = Random.Range(1, 4);
                }
                  
            }
            else
            {
                spellToCast = Random.Range(1, 4);
            }
               
            DetermineSpellToCast(spellToCast);
        }
    }

    private void DetermineSpellToCast(int spellToCast)
    {
        castSpellParticle.SetActive(true);
        StartCoroutine(DisableObjectAfter(castSpellParticle, 1.5f));

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
            case BossAbilities.MarkedGroundProjectile:
                CastMarkedGroundProjectile();
                break;
            case BossAbilities.Dash:
                Dash(transform.forward);
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

    private void CastMarkedGroundProjectile()
    {
        GameObject projectileCopy = Instantiate(markedGroundProjectile.projectileToShoot, rangedEnemyComponent.GetShootSpawnPointTransform().position, markedGroundProjectile.projectileToShoot.transform.rotation);
        projectileCopy.transform.forward = rangedEnemyComponent.GetShootSpawnPointTransform().forward;
        projectileCopy.GetComponent<GroundProjectile>().SetTarget(rangedEnemyComponent.GetCurrentTargetTransform().position);
    }

    private void OnBossKilled()
    {
        Debug.Log("BOSS SLAIN! PORTAL UNLOCKED");
    }

    private IEnumerator DisableObjectAfter(GameObject objectToDisable, float duration)
    {
        yield return new WaitForSeconds(duration);
        objectToDisable.SetActive(false);
    }

    private void Dash(Vector3 dashDir)
    {
        dashAbility.dashEffect.SetActive(true);
        StartCoroutine(DisableObjectAfter(dashAbility.dashEffect, 0.75f));
        float hitObjectDistance = CheckDistanceToObjectInFront();

        if (hitObjectDistance > 0.1f)
        {
            transform.position += dashDir * (hitObjectDistance - thisColl.bounds.extents.magnitude);
        }
        else
        {
            transform.position += dashDir * dashAbility.dashDistance;
        }
    }

    private float CheckDistanceToObjectInFront()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, dashAbility.dashDistance))
        {
            return hit.distance;
        }
        else
        {
            return 0;
        }
    }

    [System.Serializable]
    class SpawnUnitsAbility
    {
        public List<GameObject> unitSpawnPoints;
        public GameObject unitToSpawn;
    }

    [System.Serializable]
    class MarkedGroundProjectile
    {
        public GameObject projectileToShoot;
    }

    [System.Serializable]
    class DashAbility
    {
        public GameObject dashEffect;
        public float dashDistance;
    }

}
