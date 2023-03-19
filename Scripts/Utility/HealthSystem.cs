using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    // Events
    public event EventHandler OnDead;
    public event EventHandler OnDamaged;

    [Header("Variables")]
    [Tooltip("The health of the character")]
    [SerializeField] private int _health = 100;
    private int _healthMax;

    private void Awake()
    {
        _healthMax = _health;
    }

    public bool GetIsDead()
    {
        return _health <= 0;
    }

    public void Damage(int damageAmount)
    {
        _health -= damageAmount;

        if (_health < 0)
        {
            _health = 0;
        }

        OnDamaged?.Invoke(this, EventArgs.Empty);

        if (_health == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)_health / _healthMax;
    }

    public int GetHealth()
    {
        return _health;
    }
}