
//Ќа данный момент в игре не реализован
//–аботает с системой unity.rig котора€ будет полностью добавленна в будущих верси€х проекта

using Code.Global.Units.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class kick : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private AudioClip _soundEffect;


    [Header("Settings")]
    [SerializeField] private float _damage = 45f;
    [SerializeField] private float _distance = 10f;
    [SerializeField] private float _radius = 0.5f;
    [SerializeField] private float strength = 15f;  //сила пинка

    [SerializeField] private LayerMask _mask;
    [SerializeField] private LayerMask _DontKickmask;
    [SerializeField] private Transform _kickPoint;

    [SerializeField] private bool _useDestroyObj = false;



    private RagdollController rg;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            UseKick(); 
    }

    public void UseKick()
    {
        if (_kickPoint == null)
            return;

        int mask = _mask.value & ~_DontKickmask.value;

        RaycastHit[] hits = Physics.SphereCastAll(
            _kickPoint.position,
            _radius,
            _kickPoint.forward,
            _distance,
            mask,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0)
            return;


        HashSet<IDamageable> damagedObjects = new();
        HashSet<RagdollController> ragdolledObjects = new();
        HashSet<Rigidbody> pushedRigidbodies = new();
        HashSet<DeastroyObj> destroyedObjects = new();

        Transform myRoot = transform.root;

        Vector3 kickDirection = _kickPoint.forward.normalized;

        bool hitSomething = false;


        foreach (RaycastHit hit in hits)
        {
            Collider targetCollider = hit.collider;

            if (targetCollider == null)
                continue;

            if (targetCollider.transform.root == myRoot)
                continue;


            IDamageable damageable = targetCollider.GetComponentInParent<IDamageable>();


            if (damageable != null && damagedObjects.Add(damageable))
            {
                damageable.ApplyDamage(_damage);
                hitSomething = true;
            }


            if (_useDestroyObj)
            {
                DeastroyObj destroyed = targetCollider.GetComponentInParent<DeastroyObj>();

                if (destroyed != null && destroyedObjects.Add(destroyed))
                {
                    destroyed.SliseMash();
                    hitSomething = true;
                }
            }

            RagdollController ragdoll = targetCollider.GetComponentInParent<RagdollController>();

            if (ragdoll != null)
            {
                if (ragdolledObjects.Add(ragdoll))
                {
                    ragdoll.EnableRagdoll();
                    Vector3 hitPoint = hit.distance > 0f ? hit.point : targetCollider.ClosestPoint(_kickPoint.position);

                    ragdoll.AddForce(kickDirection * strength, hitPoint);

                    hitSomething = true;
                }
                continue;
            }


            Rigidbody rb = targetCollider.attachedRigidbody;

            if (rb != null && !rb.isKinematic && pushedRigidbodies.Add(rb))
            {
                rb.AddForceAtPosition(
                    kickDirection * strength,
                    hit.point,
                    ForceMode.Impulse
                );

                hitSomething = true;
            }
        }


        if (hitSomething)
            PlaySoundEffect();
    }


    //стара€ верси€ UseKick
    //public void UseKick()
    //{
    //    if (_kickPoint == null)
    //        return;

    //    if (!Physics.SphereCast(_kickPoint.position, _radius, _kickPoint.forward, out RaycastHit hit, _distance, _mask, QueryTriggerInteraction.Ignore))
    //        return;


    //    HashSet<IDamageable> damageabledObj = new HashSet<IDamageable>();
    //    HashSet<DeastroyObj> desroyedObj = new HashSet<DeastroyObj>();
    //    HashSet<Rigidbody> rbObj = new HashSet<Rigidbody>();

    //    IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();


    //    if (damageable != null && damageabledObj.Add(damageable))
    //    {
    //        damageable.ApplyDamage(_damage);
    //    }

    //    rg = hit.collider.GetComponentInParent<RagdollController>();

    //    if (rg != null)
    //    {
    //        rg.EnableRagdoll();

    //        Vector3 direction = _kickPoint.forward.normalized;

    //        rg.AddForce(direction * strength, hit.point);
    //    }
    //    else
    //    {
    //        Rigidbody rb = hit.collider.attachedRigidbody;

    //        if (rb != null && !rb.isKinematic)
    //        {
    //            rb.AddForceAtPosition(_kickPoint.forward * strength, hit.point, ForceMode.Impulse);
    //        }
    //    }

    //    PlaySoundEffect();
    //}



    private void PlaySoundEffect()
    {
        if (_soundEffect == null) return;
        AudioSource.PlayClipAtPoint(_soundEffect, transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (_kickPoint == null)
            return;

        Gizmos.DrawWireSphere(_kickPoint.position + _kickPoint.forward * _distance,  _radius);
    }
}
