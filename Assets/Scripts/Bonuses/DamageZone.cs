using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    enum DamageType { OnTriggerEnter, OnTriggerStay };

    [Header("Properties")]
    [SerializeField] private LayerMask acceptEntitiesFromLayers;
    [SerializeField] private MovableComponent movableComponent;
    [SerializeField] private SpecialEffects specialEffects;
    [SerializeField] private DamageType damageType = DamageType.OnTriggerEnter;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private float damagePerTick = 5f;
    private float damageTimestamp;
    private Transform thisTransform;
    private Vector3 moveDir;

    private void Start()
    {
        thisTransform = transform;

        switch(specialEffects.specialEffect)
        {
            case SpecialEffects.PossibleSpecialEffects.Freeze:
                Collider[] firstHitColliders = Physics.OverlapSphere(thisTransform.position, specialEffects.specialEffectDetectionRange, specialEffects.specialEffectDetectionMask);
                if (firstHitColliders.Length > 0)
                {
                    foreach(Collider col in firstHitColliders)
                    {
                        col.gameObject?.GetComponent<IDamageable>().Freeze(2f, 1f);
                    }
                }
                Destroy(this.gameObject, 3f);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damageType == DamageType.OnTriggerEnter)
        {
            if (acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
                return;

            other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damagePerTick, null);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (damageType == DamageType.OnTriggerStay)
        {
            if (Time.time < damageTimestamp || acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
                return;

            damageTimestamp = Time.time + damageCooldown;
            other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damagePerTick, null);
        }
    }

    private void Update()
    {
        if(movableComponent.isMovable)
        {
            thisTransform.Translate(moveDir * movableComponent.moveSpeed * Time.deltaTime, Space.World);

            if(moveDir == Vector3.zero)
            {
                DetermineMoveDirection();
            }
        }
    }

    [System.Serializable]
    public class SpecialEffects
    {
        public enum PossibleSpecialEffects { None, Freeze };
        public PossibleSpecialEffects specialEffect;

        public float specialEffectDetectionRange;
        public LayerMask specialEffectDetectionMask;
    }

    [System.Serializable]
    public class MovableComponent
    {
        public bool isMovable;
        public float targetDetectionRange;
        public LayerMask detectionMask;
        public float moveSpeed;
        public enum PossibleMoveDirections { LocalForward, LocalBackwards, LocalLeft, LocalRight, LocalUp };
        public PossibleMoveDirections defaultMoveDirIfNoTarget;
    }

    private void OnBecameInvisible()
    {
        if(movableComponent.isMovable)
        {
            Destroy(this.gameObject, 1f);
        }
    }

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
}
