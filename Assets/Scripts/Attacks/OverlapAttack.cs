using Code.Global.Attacks.Gizmos;
using Code.Global.Units.Interfaces;
using System;
using UnityEngine;

public class OverlapAttack : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;

    [Header("Masks")]
    [SerializeField] private LayerMask _searchLayerMask;
    [SerializeField] private LayerMask _obstacleLayerMask;

    [Header("Overlap Area")]
    [SerializeField] private Transform _overlapStartPoint;
    [SerializeField] private Vector3 _offset;
    [SerializeField, Min(0f)] private float _sphereRadius = 1f;
    [SerializeField] private float _cooldownTime = 2f;

    private float _cooldownTimer = 0f;
    //private bool _cooldown;

    [Header("Obstacles")]
    [SerializeField] private bool _isConsiderObstacles;

    [Header("Gizmos")]
    [SerializeField] private Color _gizmosColor = Color.white;

    private readonly Collider[] _overlapResults = new Collider[32];
    private int _overlapResultsCount;




    private void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(1) && _cooldownTimer <= 0f)
        {
            PerformMouseAttack();
            _cooldownTimer = _cooldownTime;
        }
    }


    private void TryAttackEnemies()
    {
        for (int i = 0; i < _overlapResultsCount; i++)
        {
            Collider col = _overlapResults[i];

            if (!col.TryGetComponent<IDamageable>(out var damageable))
                continue;

            if (_isConsiderObstacles)
            {
                Vector3 from = _overlapStartPoint.position;
                Vector3 to = col.transform.position;
                // передаём .value у маски препятствий
                if (Physics.Linecast(from, to, _obstacleLayerMask.value))
                    continue;
            }

            damageable.ApplyDamage(_damage);
        }
    }

    public bool TryPerformAttack()
    {
        if (_cooldownTimer > 0f) return false;
        PerformAttack();
        _cooldownTimer = _cooldownTime;
        return true;
    }
    private void PerformMouseAttack()
    {
        // сканируем область
        TryFindEnemies();

        // ни одного — выходим, ничего не логируем
        if (_overlapResultsCount == 0)
            return;

        for (int i = 0; i < _overlapResultsCount; i++)
        {
            var col = _overlapResults[i];

            // нет интерфейса — пропускаем
            if (!col.TryGetComponent<IDamageable>(out var damageable))
                continue;

            // препятствие — пропускаем
            if (_isConsiderObstacles)
            {
                Vector3 from = _overlapStartPoint.position;
                Vector3 to = col.transform.position;
                if (Physics.Linecast(from, to, _obstacleLayerMask.value))
                    continue;
            }

            // наносим урон и выводим только число
            damageable.ApplyDamage(_damage);
            Debug.Log(_damage);
        }
    }

    public void PerformAttack()
    {
        TryFindEnemies();
        if (_overlapResultsCount > 0)
            TryAttackEnemies();
    }

    private void TryFindEnemies()
    {
        Vector3 worldPos = _overlapStartPoint.TransformPoint(_offset);
        _overlapResultsCount = OverlapSphere(worldPos);
    }



    private int OverlapSphere(Vector3 position)
    {
        return Physics.OverlapSphereNonAlloc(position,_sphereRadius,_overlapResults,_searchLayerMask.value);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = _overlapStartPoint.localToWorldMatrix;
        Gizmos.color = _gizmosColor;
        Gizmos.DrawSphere(_offset, _sphereRadius);
    }
#endif
}
