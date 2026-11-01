using Code.Global.Units.Interfaces;
using System.Collections.Generic;
using UnityEngine;


//Здесь использовался Uity AddExploisonForce, его сигнатура, а так же реализация записанны ниже
//Так же здесь использовалась система где для откидывания игрока используется AddKnockback из Mover, а для обычных предметов обычный AddExplosionForce от Unity
//В конце скрипта есть метод AddExploisonForce из Mover

//Сделалось это из за особенности работы передвижения в mover(FixedUpdate не весь):


//private void FixedUpdate()
//{
//    float speed = isCrouchingHeld ? crouchSpeed : normalSpeed;

//    Vector3 dir =
//        transform.right * inputX +
//        transform.forward * inputY;

//    Vector3 desired =
//        dir.normalized * speed;

//    Vector3 vel = rb.velocity;


//старая реализация
//    // vel.x = desired.x;
//    // vel.z = desired.z;


//    vel.x = desired.x + _externalVelocity.x;
//    vel.z = desired.z + _externalVelocity.z;


//    _externalVelocity = Vector3.MoveTowards(
//        _externalVelocity,
//        Vector3.zero,
//        knockbackDrag * Time.fixedDeltaTime
//    );
//}



public class Explosion : MonoBehaviour
{

    [Header("Effects")]
    [SerializeField] private ParticleSystem _Effect;
    [SerializeField] private AudioClip _soundEffect;


    [Header("Settings")]
    [SerializeField] private float _damage = 45f;
    [SerializeField] private float _distance = 10f;
    [SerializeField] private float _force = 100f;
    [SerializeField] private float _upwardsModifier = 0.3f;
    [SerializeField] private float _playerKnockbackForce = 15f;

    [Header("Important Settings")]
    [SerializeField] private bool _useDestroyObj = true;
    [SerializeField] private bool _useKnockBack = true;



    public void Explode()
    {
        Collider[] col = Physics.OverlapSphere(transform.position, _distance);

        HashSet<IDamageable> damageabledObj = new HashSet<IDamageable>();
        HashSet<DeastroyObj> desroyedObj = new HashSet<DeastroyObj>();
        HashSet<Rigidbody> rbObj = new HashSet<Rigidbody>();


        
        foreach(Collider colliders in col)
        {
            if (colliders.transform.root.IsChildOf(transform))
                continue;
            IDamageable damageable = colliders.GetComponentInParent<IDamageable>();

            //if (colliders.TryGetComponent<IDamageable>(out var damage))
            //{
            //    damage.ApplyDamage(_damage);
            //}


            if (damageable != null && damageabledObj.Add(damageable))
            {
                damageable.ApplyDamage(_damage);
            }


            DeastroyObj destroyed = colliders.GetComponentInParent<DeastroyObj>();

            if (_useDestroyObj == true && (destroyed != null && desroyedObj.Add(destroyed)))
            {
                destroyed.SliseMash();
            }

            Rigidbody rb = colliders.attachedRigidbody;


            if (_useKnockBack == true && (rb != null && rbObj.Add(rb)))
            {
                Mover mover = rb.GetComponent<Mover>();

                if (mover != null)
                {
                    Vector3 direction = rb.worldCenterOfMass - transform.position;
                    float distance = direction.magnitude;

                    direction.y += _upwardsModifier;
                    direction.Normalize();

                    float strength = 1f - Mathf.Clamp01(distance / _distance);

                    Vector3 knockback = direction * _playerKnockbackForce * strength;

                    mover.AddKnockback(knockback);
                }
                else
                {
                    rb.AddExplosionForce(_force, transform.position, _distance, _upwardsModifier, ForceMode.Impulse);
                }
            }

                //Сигнатура AddExploisonForce:
                //AddExplosionForce(

                //float force,
                //Vector3 position,
                //float radius,      
                //float upwardsModifier,
                //ForceMode mode

                //);

        }
        PlaySoundEffect();
        PlayEffect();
        Destroy(transform.root.gameObject);

    }

    private void PlayEffect()
    {
        if (_Effect == null)
            return;

        var ef = Instantiate(_Effect, transform.position, Quaternion.identity);
        ef.Play();

        Destroy(ef.gameObject, ef.main.duration + ef.main.startLifetime.constantMax);
    }
    private void PlaySoundEffect()
    {
        if (_soundEffect == null) return;
        AudioSource.PlayClipAtPoint(_soundEffect, transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _distance);
    }
}




//AddExploisonForce:

//public void AddKnockback(Vector3 force)
//{
//    // Горизонтальную часть храним отдельно.
//    _externalVelocity += new Vector3(
//        force.x,
//        0f,
//        force.z
//    );

//    // Вертикальную часть отдаём Rigidbody.
//    // Дальше ею управляет обычная физика и гравитация.
//    Vector3 velocity = rb.velocity;

//    velocity.y += force.y;

//    rb.velocity = velocity;
//}
