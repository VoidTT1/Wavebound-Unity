using Code.Global.Units.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FootStepAttackAddon : MonoBehaviour
{

    [Header("Effects")]
    [SerializeField] private ParticleSystem _Effect;
    [SerializeField] private AudioClip _soundEffect;

    [Header("Sett")]
    [SerializeField] private float _damage;
    [SerializeField] private float _dist = 6f;                              // Отталкивание
    [SerializeField] private Vector3 _distance = new Vector3(12f, 2f, 12f); // Всё остальное
    [SerializeField] private float _force = 500f;
    [SerializeField] private float _upwardsModifier = 0.3f;
    [SerializeField] private float _playerKnockbackForce = 15f;
    [SerializeField] private Transform _footStepStartPoint;
    [Header("Animator")]
    [SerializeField] private string _footStepTriggerName = "FootStep";


    private Animator animator;
    private EnemyChase enemyChase;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyChase = GetComponent<EnemyChase>();
        //_dist = _distance.magnitude;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            TryStartFootStepAttack();
    }

    public bool TryStartFootStepAttack()
    {
        if (enemyChase == null)
            return false;

        enemyChase.BeginExternalAction();
        StartCoroutine(UpdateAnimations());

        return true;
    }




    //Указать имя метода в animationEvent для его выполнения
    public void FootStepHit()
    {
        Collider[] col = Physics.OverlapBox(_footStepStartPoint.position,_distance * 0.5f);

        HashSet<IDamageable> damageables = new HashSet<IDamageable>();
        HashSet<DeastroyObj> deastroyedObj = new HashSet<DeastroyObj>();
        HashSet<Rigidbody> rbObj = new HashSet<Rigidbody>();

        foreach (Collider colliders in col)
        {
            if (colliders.transform.root.IsChildOf(transform))
                continue;
            IDamageable damageable = colliders.GetComponentInParent<IDamageable>();

            if (damageable != null && damageables.Add(damageable))
            {
                damageable.ApplyDamage(_damage);
            }

            DeastroyObj destroyObj = colliders.GetComponentInParent<DeastroyObj>();
            if (destroyObj != null && deastroyedObj.Add(destroyObj))
            {
                destroyObj.SliseMash();
            }


            Rigidbody rb = colliders.attachedRigidbody;

            if (rb != null && rbObj.Add(rb))
            {
                Mover mover = rb != null ? rb.GetComponent<Mover>() : null;

                if (mover != null)
                {
                    Vector3 direction =
                        rb.worldCenterOfMass - transform.position;

                    float distance = direction.magnitude;
                    direction.y += _upwardsModifier;
                    direction.Normalize();

                    float strength = 1f - Mathf.Clamp01(distance / _dist);

                    Vector3 knockback = direction * _playerKnockbackForce * strength;

                    mover.AddKnockback(knockback);
                }
                else
                {
                    rb.AddExplosionForce(_force, _footStepStartPoint.position, _dist, _upwardsModifier, ForceMode.Impulse);
                }

            }


        }

            PlayEffect();
            PlaySoundEffect();

    }
    private IEnumerator UpdateAnimations()
    {
        animator.SetTrigger(_footStepTriggerName);
        // Ждём, пока Animator действительно войдёт в Spawn.
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(_footStepTriggerName));

        // Ждём завершения Spawn-анимации.
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        enemyChase.EndExternalAction();

    }   


    private void PlayEffect()
    {
        if (_Effect == null)
            return;

        var ef = Instantiate(_Effect, _footStepStartPoint.position, Quaternion.identity);
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
        Gizmos.DrawCube(_footStepStartPoint.position, _distance);
        Gizmos.DrawSphere( transform.position, _dist);
    }


}
