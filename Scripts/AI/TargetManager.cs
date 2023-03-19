using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("List of targets in the detector area")]
    [SerializeField] private List<GameObject> _targetsList = new List<GameObject>();
    [Tooltip("The eyes of the GameObject")]
    [SerializeField] private Transform _eyes;

    [Header("Variables")]
    [Tooltip("LayerMask for the target detection")]
    [SerializeField] private LayerMask _targetLayerMask;
    [Tooltip("The target GameObject")]
    [SerializeField] private GameObject _target;
    // Can be used for calculate a new attack position if the target isn't same that the previous target
    private GameObject _previousTarget;
    private Vector3 _previousTargetPosition;
    private Vector3 targetPosition;

    public GameObject GetTarget()
    {
        return _target;
    }

    public Vector3 GetTargetPosition()
    {
        if (_target.TryGetComponent(out ITarget iTarget))
        {
            // We check if the target is not the same as the previous target OR if the target position was changed
            if (_target != _previousTarget || _target.transform.position != _previousTargetPosition)
            {
                targetPosition = iTarget.GetPositionToTarget();
                _previousTargetPosition = _target.transform.position;
            }    
        }

        // The direction to the target
        Vector3 dirToTarget = targetPosition - transform.position;
        Debug.DrawRay(transform.position, dirToTarget, Color.yellow);

        return targetPosition;
    }

    private void Update()
    {
        CheckForTarget();
    }

    private void CheckForTarget()
    {
        // Remove missing GameObject
        _targetsList.RemoveAll(GameObject => GameObject == null);
        // Store the previous target
        if (_previousTarget != _target)
        {
            _previousTarget = _target;
            if (_target)
            {
                _previousTargetPosition = _target.transform.position;
            }
        }

        if (_targetsList.Count > 0)
        {
            SetClosestTarget();
        }
        else
        {
            _target = null;
        }
    }

    private void SetClosestTarget()
    {
        // Reset target in case the current target is not valid anymore and a new one isn't set
        _target = null;
        float minDistance = Mathf.Infinity;

        foreach (var target in _targetsList)
        {
            if (target.TryGetComponent(out ITarget iTarget))
            {
                if (iTarget.GetIsDead())
                {
                    continue;
                }

                // Distance between self and the target
                float distance = Vector3.Distance(transform.position, target.transform.position);

                // Get this target if the distance is closer
                if (distance < minDistance)
                {
                    // Get the center of the target mesh
                    Vector3 targetMeshCenter = iTarget.GetMeshCenter();

                    // Get this target if the target is visible
                    if (IsTheTargetVisible(target, targetMeshCenter))
                    {
                        _target = target;
                        minDistance = distance;
                    }
                }
            }
        }
    }

    private bool IsTheTargetVisible(GameObject target, Vector3 targetMeshCenter)
    {
        // The direction to the target
        Vector3 dirToTarget = targetMeshCenter - _eyes.position;
        // Distance between self and the target
        float distance = Vector3.Distance(_eyes.position, targetMeshCenter);

        Debug.DrawRay(_eyes.position, dirToTarget, Color.red);
        if (Physics.Raycast(_eyes.position, dirToTarget, out RaycastHit hit, distance, _targetLayerMask))
        {
            if (hit.transform.gameObject == target)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for valide layer(s) mask
        if (IsInLayerMask(other.gameObject, _targetLayerMask))
        {
            // Check for duplicate
            if (!_targetsList.Contains(other.gameObject))
            {
                // Add target to the list
                _targetsList.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _targetsList.Remove(other.gameObject);
    }
}