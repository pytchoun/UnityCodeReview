using UnityEngine;

public class WeaponFX : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The weapon")]
    [SerializeField] private Weapon _weapon;
    [Tooltip("The muzzle flash particle FX")]
    [SerializeField] private ParticleSystem _muzzleFlash;

    private void Start()
    {
        _weapon.OnShoot += Weapon_OnShoot;
    }

    private void Weapon_OnShoot(object sender, Weapon.OnShootEventArgs e)
    {
        if (e.State)
        {
            _muzzleFlash.Play();
        }
    }
}