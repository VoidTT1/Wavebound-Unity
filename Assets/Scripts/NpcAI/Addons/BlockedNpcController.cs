using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedNpcController : MonoBehaviour
{
    private EnemyChase enemyChase;
    private BlockedNPCHealth health;
    private float defSpeed;
    [SerializeField] private float shieldSpeed = 1f;
    private Animator _animator;

    private bool lastShieldState;
    private void Awake()
    {
        enemyChase = GetComponent<EnemyChase>();
        health = GetComponent<BlockedNPCHealth>();
        defSpeed = enemyChase._speed;
        lastShieldState = health.IsShield;
        _animator = GetComponent<Animator>();

        UpdateSpeed();
    }
    private void Update()
    {
        _animator.SetBool("IsShield", lastShieldState);

        if (health.IsShield == lastShieldState)
            return;

        lastShieldState = health.IsShield;
        UpdateSpeed();
    }
    private void UpdateSpeed()
    {
       enemyChase._speed = health.IsShield ? shieldSpeed : defSpeed;
    }

}
