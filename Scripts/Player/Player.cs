using System;
using UnityEngine;

public class Player : MonoBehaviour, ITarget, IDamageable
{
    // Events
    public event EventHandler<OnDeadEventArgs> OnDead;
    public class OnDeadEventArgs : EventArgs
    {
        public Vector3 ImpactPosition;
    }
    public event EventHandler OnHit;
    public event EventHandler<OnSprintEventArgs> OnSprint;
    public class OnSprintEventArgs : EventArgs
    {
        public bool State;
    }

    [Header("References")]
    [Tooltip("Get a reference to our main camera")]
    [SerializeField] private Camera _camera; 
    [Tooltip("Sphere transform for show the cursor target")]
    [SerializeField] private Transform _cursorTargetTransform;
    [Tooltip("Transform where the player holds the weapon")]
    [SerializeField] private Transform _weaponHandSlot;
    [Tooltip("The player SO")]
    [SerializeField] private PlayerSO _playerSO;
    [Tooltip("The skinned mesh renderer of the character")]
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [Tooltip("Player input")]
    [SerializeField] private Inputs _inputs;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private AnimationRiggingController _animationRiggingController;
    [SerializeField] private HealthSystem _healthSystem;
    [SerializeField] private EnduranceSystem _enduranceSystem;

    [Header("Variables")]
    [Tooltip("LayerMask for the cursor target")]
    [SerializeField] private LayerMask _cursorLayerMask;

    // Variables
    private Vector3 _mouseWorldPosition;

    public PlayerSO GetPlayerSO()
    {
        return _playerSO;
    }

    public bool GetIsDead()
    {
        return _healthSystem.GetIsDead();
    }

    public Vector3 GetMeshCenter()
    {
        return _skinnedMeshRenderer.bounds.center;
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
        PlayerUI.Instance.SetPlayerHealthText(_healthSystem.GetHealth());

        if (!GetIsDead())
        {
            OnHit?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        OnDead?.Invoke(this, new OnDeadEventArgs
        {
            ImpactPosition = Vector3.zero
        });
    }

    private void Weapon_OnShoot(object sender, Weapon.OnShootEventArgs e)
    {
        _playerAnimator.EnableShootAnimation(e.State);
    }

    private void Weapon_OnReload(object sender, EventArgs e)
    {
        _playerAnimator.TriggerReloadAnimation();
    }

    // This method is called from the reload animation when the animation is ended
    private void OnReloadCompleted()
    {
        _playerInventory.GetEquippedWeapon().OnReloadCompleted();
    }

    private void Weapon_OnAmmoUpdate(object sender, Weapon.OnAmmoUpdateEventArgs e)
    {
        PlayerUI.Instance.SetPlayerWeaponAmmoText(e.CurrentAmmo, e.MaxAmmo);
    }

    private void PlayerInventory_OnWeaponEquipped(object sender, EventArgs e)
    {
        // Set subscribers that should be removed when the weapon is unequipped
        _playerInventory.GetEquippedWeapon().OnShoot += Weapon_OnShoot;
        _playerInventory.GetEquippedWeapon().OnReload += Weapon_OnReload;
        _playerInventory.GetEquippedWeapon().OnAmmoUpdate += Weapon_OnAmmoUpdate;
    }

    private void PlayerInventory_OnWeaponUnequipped(object sender, EventArgs e)
    {
        // Unset subscribers when we unequip the weapon
        _playerInventory.GetEquippedWeapon().OnShoot -= Weapon_OnShoot;
        _playerInventory.GetEquippedWeapon().OnReload -= Weapon_OnReload;
        _playerInventory.GetEquippedWeapon().OnAmmoUpdate -= Weapon_OnAmmoUpdate;
    }

    private void EnduranceSystem_OnEnduranceUpdate(object sender, EnduranceSystem.OnEnduranceUpdateEventArgs e)
    {
        PlayerUI.Instance.SetPlayerEnduranceText(e.Endurance);
    }

    private void Awake()
    {
        // On Awake because the event will not trigger on PlayerInventory Start method
        _playerInventory.OnWeaponEquipped += PlayerInventory_OnWeaponEquipped;
        _playerInventory.OnWeaponUnequipped += PlayerInventory_OnWeaponUnequipped;
    }

    private void Start()
    {
        _healthSystem.OnDead += HealthSystem_OnDead;
        _healthSystem.OnDamaged += HealthSystem_OnDamaged;
        _enduranceSystem.OnEnduranceUpdate += EnduranceSystem_OnEnduranceUpdate;
    }

    //private void RiggingTest()
    //{
    //    if (_animationRiggingController.GetIsAimRigActive())
    //    {
    //        //_animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * _transitionInterpolationValue));
    //        _playerAnimator.SetLayerWeight(1, 1f);
    //    }
    //    else
    //    {
    //        //_animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * _transitionInterpolationValue));
    //        _playerAnimator.SetLayerWeight(1, 0f);
    //    }
    //}

    private void Update()
    {
        //RiggingTest();
        ToggleMenu();

        if (GetIsDead())
        {
            // Set the layer weight to the base layer if the player is dead
            _playerAnimator.SetLayerWeight(1, 0f);
            return;
        }

        SetRigState();
        //CursorTargetOld();
        CursorTarget();
        Aim();
        Shoot();
        Reload();
        Sprint();
    }

    private void ToggleMenu()
    {
        if (_inputs.openMenu)
        {
            Menu.Instance.ToggleMenu();
            _inputs.openMenu = false;
        }
    }

    private bool IsReloading()
    {
        return _playerInventory.GetEquippedWeapon().GetIsReloading();
    }

    private void SetRigState()
    {
        if (IsReloading())
        {
            _animationRiggingController.EnableRig(false);
        }
        else
        {
            _animationRiggingController.EnableRig(true);
        }
    }

    private void CursorTarget()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 150f, _cursorLayerMask))
        {
            _mouseWorldPosition = hit.point;
            _cursorTargetTransform.position = hit.point;

            /*if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                //Debug.Log("Target is ennemy");
            }
            else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                //Debug.Log("Target is ground");
            }*/
        }
    }

    //private void CursorTargetOld()
    //{
    //    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit[] hits = Physics.RaycastAll(ray, 150f, _cursorLayerMask).OrderBy(h => h.distance).ToArray();

    //    for (int i = 0; i < hits.Length; i++)
    //    {
    //        RaycastHit hit = hits[i];
    //        //Debug.Log("RaycastAll : " + hit.transform.gameObject.layer);
    //        //Debug.Log(hit.point);
    //        _mouseWorldPosition = hit.point;
    //        _cursorTargetTransform.position = hit.point;

    //        /*if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
    //        {
    //            //Debug.Log("Target is ennemy");
    //            break;
    //        }
    //        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
    //        {
    //            //Debug.Log("Target is ground");
    //            break;
    //        }*/
    //    }
    //}

    private void Aim()
    {
        if (_inputs.aim)
        {
            AimAtTheTarget();
        }
        else if (!_inputs.shoot)
        {
            DisableAim();
        }
    }

    private void AimAtTheTarget()
    {
        _animationRiggingController.SetIsAimRigActive(true);
        _playerController.EnableRotateOnMove(false);
        _playerAnimator.SetLayerWeight(1, 1f);

        Vector3 worldAimTarget = _mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }

    private void DisableAim()
    {
        _animationRiggingController.SetIsAimRigActive(false);
        _playerController.EnableRotateOnMove(true);

        // Check if character is reloading before come back to animator base layer
        if (!IsReloading())
        {
            _playerAnimator.SetLayerWeight(1, 0f);
        }
        else
        {
            _playerAnimator.SetLayerWeight(1, 1f);
        }
    }

    private void Shoot()
    {
        if (_inputs.shoot)
        {
            AimAtTheTarget();
            ShootAtTarget();
        }
        else if (!_inputs.aim)
        {
            DisableAim();
        }
    }

    private void ShootAtTarget()
    {
        _playerInventory.GetEquippedWeapon().Shoot(_mouseWorldPosition);
    }

    private void Reload()
    {
        if (_inputs.reload)
        {
            _playerInventory.GetEquippedWeapon().Reload();
            _inputs.reload = false;
        }
    }

    private void Sprint()
    {
        OnSprint?.Invoke(this, new OnSprintEventArgs
        {
            State = _inputs.sprint
        });
    }
}