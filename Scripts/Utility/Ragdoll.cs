using System;
using System.Linq;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [Header("References")]
    [Tooltip("All rigidbodies of the ragdoll")]
    [SerializeField] private Rigidbody[] _ragdollBodies;
    [Tooltip("All colliders of the ragdoll")]
    [SerializeField] private Collider[] _ragdollColliders;
    [Tooltip("The animator of the character")]
    [SerializeField] private Animator _animator;
    [Tooltip("The skinned mesh renderer of the character")]
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    private void Awake()
    {
        //_ragdollBodies = GetComponentsInChildren<Rigidbody>();
        //_ragdollColliders = GetComponentsInChildren<Collider>();
        _ragdollBodies = GetComponentsInChildren<Rigidbody>().Where(rb => rb.gameObject != gameObject && rb.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast")).ToArray();
        _ragdollColliders = GetComponentsInChildren<Collider>().Where(collider => collider.gameObject != gameObject && collider.GetType() != typeof(MeshCollider) && collider.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast")).ToArray();
    }

    private void Start()
    {
        ToggleRagdoll(false);

        if (TryGetComponent(out Player player))
        {
            player.OnDead += Player_OnDead;
        }
        else if (TryGetComponent(out ZombieController zombieController))
        {
            zombieController.OnDead += ZombieController_OnDead;
        }
    }

    private void Player_OnDead(object sender, Player.OnDeadEventArgs e)
    {
        EnableRagdoll(e.ImpactPosition);
    }

    private void ZombieController_OnDead(object sender, ZombieController.OnDeadEventArgs e)
    {
        EnableRagdoll(e.ImpactPosition);
    }

    public void EnableRagdoll(Vector3 impactPosition)
    {
        ToggleRagdoll(true);
        _skinnedMeshRenderer.updateWhenOffscreen = true;

        //TODO: Add force on the impact point
        /*foreach (Rigidbody rb in _ragdollBodies)
        {
            //rb.AddExplosionForce(107f, impactPosition, 5f, 0f, ForceMode.Impulse);
            //rb.AddForce(impactPosition * 20f);
            //rb.AddForceAtPosition(impactPosition, transform.position);
            //rb.AddRelativeForce(impactPosition * 20f);
        }*/
    }

    private void ToggleRagdoll(bool state)
    {
        _animator.enabled = !state;

        foreach (Rigidbody rb in _ragdollBodies)
        {
            /*if (rb.gameObject != gameObject)
            {
                rb.isKinematic = !state;
            }*/
            rb.isKinematic = !state;
        }

        foreach (Collider collider in _ragdollColliders)
        {
            /*if (collider.gameObject != gameObject)
            {
                collider.enabled = state;
            }*/
            collider.enabled = state;
        }
        //_ragdollColliders[0].enabled = !state;
    }
}