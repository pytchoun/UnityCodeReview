using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Events
    public event EventHandler<OnShootEventArgs> OnShoot;
    public class OnShootEventArgs : EventArgs
    {
        public bool State;
    }
    public event EventHandler OnReload;
    public event EventHandler<OnAmmoUpdateEventArgs> OnAmmoUpdate;
    public class OnAmmoUpdateEventArgs : EventArgs
    {
        public int CurrentAmmo;
        public int MaxAmmo;
    }

    [Header("References")]
    [Tooltip("The weapon SO")]
    [SerializeField] private WeaponSO _weaponSO;
    [Tooltip("The prefab projectile of the weapon")]
    [SerializeField] private Transform _pfBulletProjectile;
    [Tooltip("Where the projectile will spawn")]
    [SerializeField] private Transform _spawnBulletPosition;

    // Variables
    private float _nextTimeToFire;
    private int _currentAmmo;
    private bool _isReloading;

    public WeaponSO GetWeaponSO()
    {
        return _weaponSO;
    }

    public bool GetIsReloading()
    {
        return _isReloading;
    }

    void Start()
    {
        _currentAmmo = _weaponSO.CurrentAmmo;
    }

    private bool CanShoot()
    {
        if (_isReloading)
        {
            return false;
        }
        if (_currentAmmo <= 0)
        {
            Reload();
            return false;
        }
        return true;
    }

    public void Shoot(Vector3 targetWorldPosition)
    {
        if (!CanShoot())
            return;

        // Delay between each shoot
        if (Time.time > _nextTimeToFire)
        {
            _nextTimeToFire = Time.time + _weaponSO.FireRate;

            // Get the shoot direction
            Vector3 aimDirection = (targetWorldPosition - _spawnBulletPosition.position).normalized;

            // Instantiate the bullet projectile and setup it
            Transform bulletProjectile = Instantiate(_pfBulletProjectile, _spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));
            bulletProjectile.GetComponent<BulletProjectile>().Setup(_weaponSO.Damage);

            OnShoot?.Invoke(this, new OnShootEventArgs
            {
                State = true
            });

            _currentAmmo--;

            OnAmmoUpdate?.Invoke(this, new OnAmmoUpdateEventArgs
            {
                CurrentAmmo = _currentAmmo,
                MaxAmmo = _weaponSO.MaxAmmo
            });
        }
        else
        {
            OnShoot?.Invoke(this, new OnShootEventArgs
            {
                State = false
            });
        }
    }

    public void Reload()
    {
        if (!_isReloading && _currentAmmo != _weaponSO.MaxAmmo)
        {
            _isReloading = true;
            OnReload?.Invoke(this, EventArgs.Empty);

            // We use animation event for control the animation end
        }
    }

    public void OnReloadCompleted()
    {
        _currentAmmo = _weaponSO.MaxAmmo;
        _isReloading = false;

        OnAmmoUpdate?.Invoke(this, new OnAmmoUpdateEventArgs
        {
            CurrentAmmo = _currentAmmo,
            MaxAmmo = _weaponSO.MaxAmmo
        });
    }
}