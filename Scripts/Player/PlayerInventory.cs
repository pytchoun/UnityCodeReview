using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Events
    public event EventHandler OnWeaponEquipped;
    public event EventHandler OnWeaponUnequipped;

    [Header("References")]
    [Tooltip("Where the player holds the weapon")]
    [SerializeField] private Transform _weaponHandSlot;

    // References
    private Weapon _equippedWeapon;

    private void Start()
    {
        EquipWeapon();
    }

    private void Update()
    {
        
    }

    public Weapon GetEquippedWeapon()
    {
        return _equippedWeapon;
    }

    private void EquipWeapon()
    {
        Transform weaponInHand = _weaponHandSlot.GetChild(0);
        if (weaponInHand.TryGetComponent(out _equippedWeapon))
        {
            // Set subscribers
            OnWeaponEquipped?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UnequipWeapon()
    {
        Transform weaponInHand = _weaponHandSlot.GetChild(0);
        if (weaponInHand.TryGetComponent(out _equippedWeapon))
        {
            // Unset subscribers
            OnWeaponUnequipped?.Invoke(this, EventArgs.Empty);
        }
    }
}