using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("References")]
    [Tooltip("The shoots audio clip")]
    public AudioClip[] ShootClip;
    [Tooltip("The reload audio clip")]
    public AudioClip ReloadClip;
    [Tooltip("The weapon reload animation clip")]
    public AnimationClip ReloadAnimationClip; // Not used anymore, use animation event for control clip length and do stuff

    [Header("Variables")]
    [Tooltip("The weapon damage")]
    public float Damage;
    [Tooltip("The weapon fire rate")]
    public float FireRate;
    [Tooltip("The weapon maximum ammo capacity")]
    public int MaxAmmo;
    [Tooltip("The weapon current ammo")]
    public int CurrentAmmo;
    [Tooltip("The weapon reload time")]
    public float ReloadTime; // Not used anymore, use clip length

    private void OnEnable()
    {
        CurrentAmmo = MaxAmmo;
    }
}