using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZombieController _zombieController;
    [Tooltip("The animator of the character")]
    [SerializeField] private Animator _animator;

    // Variables
    private float _transitionInterpolationValue = 13f;
    private bool _isPlayingAttackAnimation;
    private float _nextTimeToAttack;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDAttacking;
    //private int _animIDDead;

    private void Start()
    {
        AssignAnimationIDs();
        _zombieController.OnSpeedChanged += ZombieController_OnSpeedChanged;
        _zombieController.OnAttackStart += ZombieController_OnAttackStart;
    }

    private void Update()
    {
        AnimatorLayer();
    }

    private void AnimatorLayer()
    {
        if (_isPlayingAttackAnimation)
        {
            // Enable attack animator layer
            SetLayerWeight(1, 1f);
            if (Time.time > _nextTimeToAttack)
            {
                _isPlayingAttackAnimation = false;
            }
        }
        else
        {
            SetLayerWeight(1, 0f);
        }
    }

    private void ZombieController_OnAttackStart(object sender, ZombieController.OnAttackStartEventArgs e)
    {
        TriggerAttackAnimation();
        _isPlayingAttackAnimation = true;
        _nextTimeToAttack = e.NextTimeToAttack;
    }

    private void ZombieController_OnSpeedChanged(object sender, ZombieController.OnSpeedChangedEventArgs e)
    {
        SetSpeedValue(e.Speed);
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDAttacking = Animator.StringToHash("Attacking");
        //_animIDDead = Animator.StringToHash("Dead");
    }

    public void SetLayerWeight(int layerIndex, float weight)
    {
        _animator.SetLayerWeight(layerIndex, Mathf.Lerp(_animator.GetLayerWeight(layerIndex), weight, Time.deltaTime * _transitionInterpolationValue));
    }

    public void SetSpeedValue(float speedValue)
    {
        _animator.SetFloat(_animIDSpeed, speedValue);
    }

    public void TriggerAttackAnimation()
    {
        _animator.SetTrigger(_animIDAttacking);
    }

    //public void TriggerDeadAnimation()
    //{
    //    _animator.SetTrigger(_animIDDead);
    //}
}