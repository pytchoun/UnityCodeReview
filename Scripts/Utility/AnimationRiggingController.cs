using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationRiggingController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The character rig")]
    [SerializeField] private Rig _rig;
    [Tooltip("The left hand rig target")]
    [SerializeField] private Transform _leftHandRigTarget;
    [Tooltip("The left hand rig hint")]
    [SerializeField] private Transform _leftHandRigHint;
    [Tooltip("The body rig")]
    [SerializeField] private GameObject _bodyAimRig;
    [Tooltip("The right hand rig")]
    [SerializeField] private GameObject _rightHandAimRig;
    [Tooltip("The left hand rig")]
    [SerializeField] private GameObject _leftHandRig;

    [Header("Variables")]
    [Tooltip("Control if the aim rig is active or not")]
    [SerializeField] private bool IsAimRigActive;

    // References
    private MultiAimConstraint _bodyAimConstraint;
    private MultiAimConstraint _rightHandAimConstraint;
    //private TwoBoneIKConstraint _leftHandtwoBoneIKConstraint;

    // Variables
    private float _targetWeight;

    private Vector3 _idleLeftHandRigTargetPosition = new Vector3(0.424f, 0.167f, -0.072f);
    private Vector3 _idleLeftHandRigTargetRotation = new Vector3(-182.6f, 0.0f, -115.26f);

    private Vector3 _idleLeftHandRigHintPosition = new Vector3(0.143f, 0.853f, 0.034f);
    private Vector3 _idleLeftHandRigHintRotation = new Vector3(0.0f, 0.0f, 0.0f);

    private Vector3 _aimingLeftHandRigTargetPosition = new Vector3(0.3004f, 0.0642f, -0.07f);
    private Vector3 _aimingLeftHandRigTargetRotation = new Vector3(-22.763f, 185.623f, -25.24f);

    private Vector3 _aimingLeftHandRigHintPosition = new Vector3(0.064f, 0.327f, -0.01f);
    private Vector3 _aimingLeftHandRigHintRotation = new Vector3(0.0f, 0.0f, 0.0f);

    private float _transitionInterpolationValue = 10f;

    public bool GetIsAimRigActive()
    {
        return IsAimRigActive;
    }

    public void SetIsAimRigActive(bool state)
    {
        IsAimRigActive = state;
    }

    public void EnableRig(bool state)
    {
        //_leftHandtwoBoneIKConstraint.weight = value;
        if (state)
        {
            _rig.weight = Mathf.Lerp(_rig.weight, 1f, Time.deltaTime * _transitionInterpolationValue);
        }
        else
        {
            _rig.weight = 0f;
        }
    }

    private void Awake()
    {
        /*RigBuilder rigBuilder = GetComponent<RigBuilder>();
        _rig = rigBuilder.layers[0].rig.GetComponent<Rig>();*/
        _bodyAimConstraint = _bodyAimRig.GetComponent<MultiAimConstraint>();
        _rightHandAimConstraint = _rightHandAimRig.GetComponent<MultiAimConstraint>();
        //_leftHandtwoBoneIKConstraint = _leftHandRig.GetComponent<TwoBoneIKConstraint>();
    }

    private void Update()
    {
        if (IsAimRigActive)
        {
            _targetWeight = 1f;

            _leftHandRigTarget.localPosition = _aimingLeftHandRigTargetPosition;
            _leftHandRigTarget.localEulerAngles = _aimingLeftHandRigTargetRotation;

            _leftHandRigHint.localPosition = _aimingLeftHandRigHintPosition;
            _leftHandRigHint.localEulerAngles = _aimingLeftHandRigHintRotation;
        }
        else
        {
            _targetWeight = 0f;

            _leftHandRigTarget.localPosition = _idleLeftHandRigTargetPosition;
            _leftHandRigTarget.localEulerAngles = _idleLeftHandRigTargetRotation;

            _leftHandRigHint.localPosition = _idleLeftHandRigHintPosition;
            _leftHandRigHint.localEulerAngles = _idleLeftHandRigHintRotation;
        }

        //_rig.weight = Mathf.Lerp(_rig.weight, _targetWeight, Time.deltaTime * _transitionInterpolationValue);
        _bodyAimConstraint.weight = Mathf.Lerp(_bodyAimConstraint.weight, _targetWeight / 2, Time.deltaTime * _transitionInterpolationValue);
        _rightHandAimConstraint.weight = Mathf.Lerp(_rightHandAimConstraint.weight, _targetWeight, Time.deltaTime * _transitionInterpolationValue);
    }
}