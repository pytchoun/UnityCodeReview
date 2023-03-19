using UnityEngine;

public class WeaponSounds : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The weapon")]
    [SerializeField] private Weapon _weapon;
    [Tooltip("The audio source for playing the audio")]
    [SerializeField] private AudioSource _audioSource;

    private void Start()
    {
        _weapon.OnShoot += Weapon_OnShoot;
        _weapon.OnReload += Weapon_OnReload;
    }

    private void Weapon_OnReload(object sender, System.EventArgs e)
    {
        _audioSource.clip = _weapon.GetWeaponSO().ReloadClip;
        _audioSource.pitch = Random.Range(1.4f, 1.8f);
        _audioSource.Play();
    }

    private void Weapon_OnShoot(object sender, Weapon.OnShootEventArgs e)
    {
        if (e.State)
        {
            _audioSource.clip = _weapon.GetWeaponSO().ShootClip[Random.Range(0, _weapon.GetWeaponSO().ShootClip.Length)];
            _audioSource.pitch = Random.Range(1.4f, 1.8f);
            _audioSource.Play();
        }
    }
}