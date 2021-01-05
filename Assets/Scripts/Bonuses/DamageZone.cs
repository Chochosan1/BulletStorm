using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    enum DamageType { OnTriggerEnter, OnTriggerStay };

    [Header("Properties")]
    [SerializeField] private LayerMask acceptEntitiesFromLayers;
    [SerializeField] private MovableComponent movableComponent;
    [Tooltip("The special effects trigger only once when the object spawns.")]
    [SerializeField] private SpecialEffectsOnSpawn specialEffectsOnSpawn;
    [Tooltip("This component is affected by the damage type property (OnTriggerEnter or OnTriggerStay). The special effect will get applied every time this object deals damage.")]
    [SerializeField] private SpecialEffectsOnTouch specialEffectsOnTouch;
    [SerializeField] private DamageType damageType = DamageType.OnTriggerEnter;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private float damagePerTick = 5f;
    private float damageTimestamp;
    private Transform thisTransform;
    private Vector3 moveDir;

    private void Start()
    {
        thisTransform = transform;

        DetermineSpecialEffectOnSpawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damageType == DamageType.OnTriggerEnter)
        {
            if (acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
                return;

            IDamageable tempDamageable = other.GetComponent<IDamageable>();
            tempDamageable?.TakeDamage(damagePerTick, null);

            DetermineSpecialEffectOnTouch(tempDamageable);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (damageType == DamageType.OnTriggerStay)
        {
            if (Time.time < damageTimestamp || acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
                return;

            damageTimestamp = Time.time + damageCooldown;
            IDamageable tempDamageable = other.GetComponent<IDamageable>();
            tempDamageable?.TakeDamage(damagePerTick, null);

            DetermineSpecialEffectOnTouch(tempDamageable);
        }
    }

    private void Update()
    {
        if (movableComponent.isMovable)
        {
            thisTransform.Translate(moveDir * movableComponent.moveSpeed * Time.deltaTime, Space.World);

            if (moveDir == Vector3.zero)
            {
                DetermineMoveDirection();
            }
        }
    }

    /// <summary>
    /// Determine what special effect to cast when the object deals damage (OnTriggerEnter or OnTriggerStay).
    /// </summary>
    private void DetermineSpecialEffectOnTouch(IDamageable tempDamageable)
    {
        if (specialEffectsOnTouch.specialEffect != SpecialEffectsOnTouch.PossibleSpecialEffects.None)
        {
            switch (specialEffectsOnTouch.specialEffect)
            {
                case SpecialEffectsOnTouch.PossibleSpecialEffects.Freeze:
                    tempDamageable.Freeze(2.5f, specialEffectsOnTouch.chanceToTriggerEffect);
                    break;
                case SpecialEffectsOnTouch.PossibleSpecialEffects.Slow:
                    tempDamageable.GetSlowed(1f, 0.5f, specialEffectsOnTouch.chanceToTriggerEffect);
                    break;
            }
        }
    }

    /// <summary>
    /// Determine what special effect to cast when the object first spawns.
    /// </summary>
    private void DetermineSpecialEffectOnSpawn()
    {
        switch (specialEffectsOnSpawn.specialEffect)
        {
            case SpecialEffectsOnSpawn.PossibleSpecialEffects.Freeze:
                Collider[] firstHitColliders = Physics.OverlapSphere(thisTransform.position, specialEffectsOnSpawn.specialEffectDetectionRange, acceptEntitiesFromLayers);
                if (firstHitColliders.Length > 0)
                {
                    foreach (Collider col in firstHitColliders)
                    {
                        col.gameObject?.GetComponent<IDamageable>()?.Freeze(2f, 1f);
                    }
                }
                if (specialEffectsOnSpawn.destroyAfterEffect)
                    Destroy(this.gameObject, 3f);
                break;
            case SpecialEffectsOnSpawn.PossibleSpecialEffects.Slow:
                firstHitColliders = Physics.OverlapSphere(thisTransform.position, specialEffectsOnSpawn.specialEffectDetectionRange, acceptEntitiesFromLayers);
                if (firstHitColliders.Length > 0)
                {
                    foreach (Collider col in firstHitColliders)
                    {
                        col.gameObject?.GetComponent<IDamageable>()?.GetSlowed(2f, 0.6f, 1f);
                    }
                }
                if (specialEffectsOnSpawn.destroyAfterEffect)
                    Destroy(this.gameObject, 3f);
                break;
        }
    }

    /// <summary>
    /// The object will move in the general direction of the closest enemy OR if no enemy available it will go into a preset default direction.
    /// </summary>
    private void DetermineMoveDirection()
    {
        if (movableComponent.isMovable)
        {
            Collider[] firstHitColliders = Physics.OverlapSphere(thisTransform.position, movableComponent.targetDetectionRange, movableComponent.detectionMask);
            if (firstHitColliders.Length > 0)
            {
                moveDir = (firstHitColliders[0].gameObject.transform.position - thisTransform.position).normalized;
            }
            else
            {
                switch (movableComponent.defaultMoveDirIfNoTarget)
                {
                    case MovableComponent.PossibleMoveDirections.LocalForward:
                        moveDir = thisTransform.forward;
                        break;
                    case MovableComponent.PossibleMoveDirections.LocalBackwards:
                        moveDir = -thisTransform.forward;
                        break;
                    case MovableComponent.PossibleMoveDirections.LocalLeft:
                        moveDir = -thisTransform.right;
                        break;
                    case MovableComponent.PossibleMoveDirections.LocalRight:
                        moveDir = thisTransform.right;
                        break;
                    case MovableComponent.PossibleMoveDirections.LocalUp:
                        moveDir = thisTransform.up;
                        break;
                }
            }
        }
    }

    [System.Serializable]
    public class SpecialEffectsOnSpawn
    {
        public enum PossibleSpecialEffects { None, Freeze, Slow };
        [Tooltip("The special effects trigger only once when the object spawns.")]
        public PossibleSpecialEffects specialEffect;

        [Tooltip("Area of effect range of the special effect upon spawning the object.")]
        public float specialEffectDetectionRange;
        public bool destroyAfterEffect = true;
    }

    [System.Serializable]
    public class MovableComponent
    {
        [Tooltip("Should this object move?")]
        public bool isMovable;
        [Tooltip("Initial target it should move towards. Takes only the general direction and will not follow the target if it moves.")]
        public float targetDetectionRange;
        [Tooltip("Layer to detect enemies in.")]
        public LayerMask detectionMask;
        public float moveSpeed;
        public enum PossibleMoveDirections { LocalForward, LocalBackwards, LocalLeft, LocalRight, LocalUp };
        public PossibleMoveDirections defaultMoveDirIfNoTarget;
    }

    [System.Serializable]
    public class SpecialEffectsOnTouch
    {
        public enum PossibleSpecialEffects { None, Freeze, Slow };
        [Tooltip("The special effects trigger whenever this object deals damage.")]
        public PossibleSpecialEffects specialEffect;

        [Tooltip("The chance to trigger the special effect when dealing damage.")]
        [Range(0f, 1f)]
        public float chanceToTriggerEffect;
    }

    private void OnBecameInvisible()
    {
        if (movableComponent.isMovable)
        {
            Destroy(this.gameObject, 1f);
        }
    }
}
