using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour, IDamageable, ITarget
{
    [Header("References")]
    [Tooltip("The turret swivel")]
    [SerializeField] private GameObject _swivel;
    [Tooltip("The turret target")]
    [SerializeField] private GameObject _target;
    [Tooltip("The prefab projectile of the weapon")]
    [SerializeField] private Transform _pfBulletProjectile;
    [Tooltip("Where the projectile will spawn")]
    [SerializeField] private Transform _spawnBulletPosition;
    [Tooltip("List of targets in the turret detector")]
    [SerializeField] private List<GameObject> _targetList = new List<GameObject>();
    [Tooltip("LayerMask for the target detection")]
    [SerializeField] private LayerMask _targetLayerMask;
    [Tooltip("LayerMask for the target obstruction")]
    [SerializeField] private LayerMask _obstructionLayerMask;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private HealthSystem _healthSystem;

    [Header("Variables")]
    [Tooltip("Rotate direction")]
    [SerializeField] private bool _rotateToRight;
    [Tooltip("Duration for doing a rotation")]
    [SerializeField] private float _rotationDuration;
    [Tooltip("The turret angle detector")]
    [SerializeField] private float _angle;
    [Tooltip("The turret distance detector")]
    [SerializeField] private float _viewDistance;

    // Variables
    private bool _isTurning;
    private float _nextTimeToFire;
    private float _fireRate = 0.2f;
    private float _damage = 10f;

    private void OnDrawGizmos()
    {
        DrawAngle();
    }

    private void DrawAngle()
    {
        Vector3 viewAngle01 = DirectionFromAngle(transform.eulerAngles.y, -_angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(transform.eulerAngles.y, _angle / 2);

        Debug.DrawLine(transform.position, transform.position + viewAngle01 * _viewDistance, Color.blue);
        Debug.DrawLine(transform.position, transform.position + viewAngle02 * _viewDistance, Color.blue);
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public bool GetIsDead()
    {
        return _healthSystem.GetIsDead();
    }

    public Vector3 GetMeshCenter()
    {
        return _meshRenderer.bounds.center;
    }

    public Vector3 GetPositionToTarget()
    {
        return transform.position;
    }

    public void AnyDamage(float damage, Vector3 impactPosition)
    {
        _healthSystem.Damage((int)damage);
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        //if (!GetIsDead())
        //{

        //}
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        _healthSystem.OnDead += HealthSystem_OnDead;
        _healthSystem.OnDamaged += HealthSystem_OnDamaged;
    }

    private void Update()
    {
        //_swivel.transform.eulerAngles = new Vector3(0f, Mathf.PingPong(Time.time / 3f, 1f) * 90f - 45f, 0f);
        TryToFindTarget();
        HandleTurretState();
    }

    private void HandleTurretState()
    {
        if (!_target)
        {
            if (_targetList.Count > 0)
            {
                GetClosestTarget();
            }
            else
            {
                TurretRotation();
            }
        }
        else
        {
            StopAllCoroutines();
            _isTurning = false;
            Shoot();
        }
    }

    private void TryToFindTarget()
    {
        // Reset the list
        _targetList.Clear();
        // Get all targets in the range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _viewDistance, _targetLayerMask);
        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, hitCollider.gameObject.transform.position);
            if (distance < _viewDistance)
            {
                // Target inside viewDistance
                Vector3 dirToTarget = (hitCollider.gameObject.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, dirToTarget) < _angle / 2)
                {
                    // Check for not obstruction
                    if (!Physics.Raycast(transform.position, dirToTarget, out RaycastHit obstructionHit, distance, _obstructionLayerMask))
                    {
                        // Target inside field of view
                        if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit hit, distance, _targetLayerMask))
                        {
                            Debug.DrawLine(transform.position, hit.transform.position, Color.green);
                            // Hit something
                            if (hit.transform.gameObject.TryGetComponent(out ZombieController zombieController))
                            {
                                // Check for duplicate
                                if (!_targetList.Contains(hit.transform.gameObject))
                                {
                                    // Add target to the list
                                    _targetList.Add(hit.transform.gameObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.DrawLine(transform.position, obstructionHit.transform.position, Color.yellow);
                    }
                }
            }
        }
        // Check that the current target is on the list
        CheckCurrentTarget();
    }

    private void CheckCurrentTarget()
    {
        if (!_targetList.Contains(_target))
        {
            _target = null;
        }
    }

    private void GetClosestTarget()
    {
        GameObject go = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (var target in _targetList)
        {
            float distance = Vector3.Distance(target.transform.position, currentPosition);
            if (distance < minDistance)
            {
                go = target;
                minDistance = distance;
            }
        }

        _target = go;
    }

    private void Shoot()
    {
        Vector3 targetWorldPosition = _target.GetComponentInChildren<Renderer>().bounds.center;
        _swivel.transform.LookAt(targetWorldPosition);
        Debug.DrawLine(_spawnBulletPosition.position, targetWorldPosition, Color.red);

        // Delay between each shoot
        if (Time.time > _nextTimeToFire)
        {
            _nextTimeToFire = Time.time + _fireRate;

            // Get the shoot direction
            Vector3 aimDirection = (targetWorldPosition - _spawnBulletPosition.position).normalized;

            // Instantiate the bullet projectile and setup it
            Transform bulletProjectile = Instantiate(_pfBulletProjectile, _spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));
            bulletProjectile.GetComponent<BulletProjectile>().Setup(_damage);
        }
    }

    // Turret swivel rotate around the angle
    private void TurretRotation()
    {
        if (!_isTurning)
        {
            StartCoroutine(TurretRotation(_rotateToRight ? _angle / 2 : -_angle / 2));
        }
    }

    private IEnumerator TurretRotation(float rotationValue)
    {
        float time = 0f;
        _isTurning = true;
        Quaternion startValue = _swivel.transform.rotation;
        Quaternion endValue = Quaternion.Euler(0f, rotationValue, 0f);

        while (time < _rotationDuration)
        {
            _swivel.transform.rotation = Quaternion.Lerp(startValue, endValue, time / _rotationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        _swivel.transform.rotation = endValue;
        _isTurning = false;
        _rotateToRight = !_rotateToRight;
    }
}