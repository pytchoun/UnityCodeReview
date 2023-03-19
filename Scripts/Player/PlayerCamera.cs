using Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player input")]
    [SerializeField] private Inputs _inputs;

    [Header("Camera")]
    [Tooltip("Camera sensitivity")]
    [SerializeField] private float _sensitivity = 1.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField] private float _cameraAngleOverride = 0.0f;

    [Header("Cinemachine")]
    [Tooltip("Get a reference to our Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private Cinemachine3rdPersonFollow _cinemachineFollow;
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] private GameObject _cinemachineCameraTarget;

    [Header("Zoom")]
    [Tooltip("Max value for zoom in")]
    [SerializeField] private float _zoomInMax = 10f;
    [Tooltip("Max value for zoom out")]
    [SerializeField] private float _zoomOutMax = 30f;

    // Variables
    private const float _threshold = 0.01f;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch = 45f;
    private float _cameraTargetDistance;

    private void Awake()
    {
        _cinemachineFollow = _cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    private void Start()
    {
        _cameraTargetDistance = _cinemachineFollow.CameraDistance;
    }

    private void LateUpdate()
    {
        CameraRotation();
        CameraZoom();
    }

    private void CameraRotation()
    {
        // If there is an input and camera position is not fixed
        if (_inputs.look.sqrMagnitude >= _threshold && !_inputs.lockCamera)
        {
            _cinemachineTargetYaw += _inputs.look.x * Time.deltaTime * _sensitivity;
            //_cinemachineTargetPitch += _inputs.look.y * Time.deltaTime * _sensitivity;

            //float rotateDir = 0f;       
            //if (_inputs.look.x > 0f)
            //{
            //    rotateDir = 1f;
            //}
            //else if (_inputs.look.x < 0f)
            //{
            //    rotateDir = -1f;
            //}
            //float rotateSpeed = 300f;
            //_cinemachineTargetYaw += rotateDir * rotateSpeed * Time.deltaTime * _sensitivity;
        }

        // Clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        //_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private void CameraZoom()
    {
        if (_inputs.zoom > 0f)
        {
            _cameraTargetDistance += 5f;
        }
        if (_inputs.zoom < 0f)
        {
            _cameraTargetDistance -= 5f;
        }
        //_inputs.zoom = 0f;
        _cameraTargetDistance = Mathf.Clamp(_cameraTargetDistance, _zoomInMax, _zoomOutMax);
        float zoomSpeed = 5f;
        _cinemachineFollow.CameraDistance = Mathf.Lerp(_cinemachineFollow.CameraDistance, _cameraTargetDistance, Time.deltaTime * zoomSpeed);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}