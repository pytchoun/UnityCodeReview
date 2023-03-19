using UnityEngine;

public class BasicRigidBodyPush : MonoBehaviour
{
    [Header("Variables")]
    [Tooltip("What layers where push is possible")]
	[SerializeField] private LayerMask _pushLayers;
	[Tooltip("Control if push is possible")]
	[SerializeField] private bool _canPush;
	[Tooltip("The strength of the push")]
	[Range(0.5f, 5f)]
	[SerializeField] private float _strength = 1.1f;

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (_canPush) PushRigidBodies(hit);
	}

	private void PushRigidBodies(ControllerColliderHit hit)
	{
		// https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

		// make sure we hit a non kinematic rigidbody
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic) return;

		// make sure we only push desired layer(s)
		var bodyLayerMask = 1 << body.gameObject.layer;
		if ((bodyLayerMask & _pushLayers.value) == 0) return;

		// We dont want to push objects below us
		//if (hit.moveDirection.y < -0.3f) return;

		// Calculate push direction from move direction, horizontal motion only
		//Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
		//Vector3 pushDir = body.position - transform.position;
		Vector3 pushDir = new Vector3(body.position.x - transform.position.x, 0.0f, body.position.z - transform.position.z);

		// Apply the push and take strength into account
		body.AddForce(pushDir.normalized * _strength, ForceMode.Impulse);
	}
}