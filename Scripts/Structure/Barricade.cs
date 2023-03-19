using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Barricade : MonoBehaviour, ITarget, IDamageable
{
    [Header("References")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private HealthSystem _healthSystem;

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
        return GetRandomMeshPosition();
    }

    public void AnyDamage(float damage, Vector3 impactPosition)
    {
        //Debug.Log("Barricade : Barricade take hit");
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

    private Vector3 GetRandomMeshPosition()
    {
        Vector3 postion = new Vector3(Random.Range(_meshRenderer.bounds.min.x, _meshRenderer.bounds.max.x), _meshRenderer.bounds.min.y, transform.position.z);
        return postion;
    }
}