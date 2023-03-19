using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The animator of the character")]
    [SerializeField] private Animator _animator;

    // Variables
    private float _transitionInterpolationValue = 13f;

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDShooting;
    private int _animIDReloading;

    private void Start()
    {
        AssignAnimationIDs();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDShooting = Animator.StringToHash("Shooting");
        _animIDReloading = Animator.StringToHash("Reloading");
    }

    public void EnableGrounded(bool state)
    {
        _animator.SetBool(_animIDGrounded, state);
    }

    public void SetSpeedValue(float speedValue)
    {
        _animator.SetFloat(_animIDSpeed, speedValue);
    }

    public void SetMotionSpeedValue(float speedValue)
    {
        _animator.SetFloat(_animIDMotionSpeed, speedValue);
    }

    public void EnableJump(bool state)
    {
        _animator.SetBool(_animIDJump, state);
    }

    public void EnableFreeFall(bool state)
    {
        _animator.SetBool(_animIDFreeFall, state);
    }

    public void SetLayerWeight(int layerIndex, float weight)
    {
        _animator.SetLayerWeight(layerIndex, Mathf.Lerp(_animator.GetLayerWeight(layerIndex), weight, Time.deltaTime * _transitionInterpolationValue));
    }

    public void EnableShootAnimation(bool state)
    {
        _animator.SetBool(_animIDShooting, state);
    }

    public void TriggerReloadAnimation()
    {
        EnableShootAnimation(false);
        _animator.SetTrigger(_animIDReloading);
    }
}