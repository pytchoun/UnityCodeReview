using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieController : MonoBehaviour, IDamageable
{
    // Events
    public event EventHandler<OnDeadEventArgs> OnDead;
    public class OnDeadEventArgs : EventArgs
    {
        public Vector3 ImpactPosition;
    }
    public static event EventHandler OnAnyDead;
    //new public static void ResetStaticData()
    //{
    //    OnAnyDead = null;
    //}
    public event EventHandler OnHit;
    public event EventHandler<OnSpeedChangedEventArgs> OnSpeedChanged;
    public class OnSpeedChangedEventArgs : EventArgs
    {
        public float Speed;
    }
    public event EventHandler<OnAttackStartEventArgs> OnAttackStart;
    public class OnAttackStartEventArgs : EventArgs
    {
        public float NextTimeToAttack;
    }

    [Header("References")]
    [Tooltip("The zombie SO")]
    [SerializeField] private ZombieSO _zombieSO;
    [Tooltip("The zombie hand transform used for attack hit colliders")]
    [SerializeField] private Transform _zombieHandTransform;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private TargetManager _targetManager;
    [SerializeField] private NavMeshObstacle _navMeshObstacle;
    [SerializeField] private HealthSystem _healthSystem;

    [Header("Variables")]
    [Tooltip("What layers where push is possible")]
    [SerializeField] private LayerMask _pushLayers;
    [Tooltip("The strength of the push")]
    [Range(0.5f, 5f)]
    [SerializeField] private float _strength = 1.1f;

    // Variables
    private Vector3 _impactPosition;
    private float _nextTimeToAttack;
    private float _zombieHandRadius = 0.07f;
    private Transform _waypoint;

    private State _state;
    private enum State
    {
        Idle,
        Wander,
        Chasing,
        Attacking
    }

    public void Setup(Transform waypoint)
    {
        _waypoint = waypoint;
    }

    public bool GetIsDead()
    {
        return _healthSystem.GetIsDead();
    }

    public ZombieSO GetZombieSO()
    {
        return _zombieSO;
    }

    public void AnyDamage(float damage, Vector3 impactPosition)
    {
        _impactPosition = impactPosition;
        _healthSystem.Damage((int)damage);
    }

    // This method is called from the attack animation when the attack should hit the target
    private void OnAttackHit()
    {
        Damage();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_zombieHandTransform.position, _zombieHandRadius);
    }

    private void HealthSystem_OnDamaged(object sender, EventArgs e)
    {
        //Debug.Log("ZombieController : Zombie take hit / Impact position = " + _impactPosition);
        if (!GetIsDead())
        {
            // Play hit sound
            OnHit?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        // Enable ragdoll + Play dead sound
        OnDead?.Invoke(this, new OnDeadEventArgs
        {
            ImpactPosition = _impactPosition
        });
        // Can be used for remove the zombie from the spawner list
        OnAnyDead?.Invoke(this, EventArgs.Empty);

        // Stop the agent
        if (_navMeshAgent.enabled)
        {
            _navMeshAgent.isStopped = true;
        }

        // Destroy gameObject
        Destroy(gameObject, 5f);

        // Disable collider
        gameObject.GetComponent<CapsuleCollider>().enabled = false;

        // Set DeadBodies layer
        SwitchToDeadBodiesLayer();
    }

    private void SwitchToDeadBodiesLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("DeadBodies");
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
            {
                child.gameObject.layer = LayerMask.NameToLayer("DeadBodies");
            }
        }
    }

    private void Awake()
    {
        _state = State.Idle;
    }

    private void Start()
    {
        _healthSystem.OnDead += HealthSystem_OnDead;
        _healthSystem.OnDamaged += HealthSystem_OnDamaged;
    }

    private void Update()
    {
        if (GetIsDead())
        {
            return;
        }

        HandleState();
        AgentState();

        // Set blend tree speed variable (Example of use)
        OnSpeedChanged?.Invoke(this, new OnSpeedChangedEventArgs
        {
            Speed = _navMeshAgent.velocity.magnitude
        });
    }

    private void HandleState()
    {
        // If i have a target
        if (_targetManager.GetTarget())
        {
            // If i have a target in attack range
            if (Vector3.Distance(_targetManager.GetTargetPosition(), transform.position) <= _zombieSO.AttackRange)
            {
                _state = State.Attacking;
            }
            // If i have a target but this target isn't in range
            else if (Vector3.Distance(_targetManager.GetTargetPosition(), transform.position) >= _zombieSO.AttackRange)
            {
                _state = State.Chasing;
            }
        }
        // If i haven't a target
        else
        {
            _state = State.Idle;
        }
    }

    private void AgentState()
    {
        //Debug.Log("ZombieController State : " + _state);
        switch (_state)
        {
            case State.Idle:
                IdleState();
                break;
            case State.Wander:
                WanderState();
                break;
            case State.Chasing:
                ChasingState();
                break;
            case State.Attacking:
                AttackingState();
                break;
            default:
                break;
        }
    }

    private void EnableNavMeshObstacle(bool state)
    {
        // Both can't be true at the same time
        if (state)
        {
            _navMeshAgent.enabled = !state;
            _navMeshObstacle.enabled = state;
        }
        else
        {
            _navMeshObstacle.enabled = state;
            _navMeshAgent.enabled = !state;
        }
    }

    private void IdleState()
    {
        //EnableNavMeshObstacle(true);
        if (_waypoint)
        {
            CheckDistanceToWaypoint();
        }
    }

    // Move the zombie to the waypoint for don't be idle
    private void CheckDistanceToWaypoint()
    {
        float distance = Vector3.Distance(transform.position, _waypoint.position);
        float minDistance = 5f;
        if (distance > minDistance)
        {
            _state = State.Wander;
            WanderState();
        }
        else
        {
            EnableNavMeshObstacle(true);
        }
    }

    private void WanderState()
    {
        if (_waypoint)
        {
            Wander();
        }
    }

    private void Wander()
    {
        EnableNavMeshObstacle(false);

        Vector3 position = _waypoint.position;
        if (!_navMeshAgent.pathPending)
        {
            float maxDistance = 5f;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, _navMeshAgent.areaMask))
            {
                //Debug.Log(_navMeshAgent.remainingDistance);
                _navMeshAgent.SetDestination(hit.position);
            }
        }
    }

    private void ChasingState()
    {
        Chase();
    }

    private void Chase()
    {
        EnableNavMeshObstacle(false);

        if (!_navMeshAgent.pathPending)
        {
            float maxDistance = 5f;
            if (NavMesh.SamplePosition(_targetManager.GetTargetPosition(), out NavMeshHit hit, maxDistance, _navMeshAgent.areaMask))
            {
                //Debug.Log(_navMeshAgent.remainingDistance);
                _navMeshAgent.SetDestination(hit.position);
            }
        }
    }

    private void AttackingState()
    {
        Attack();
    }

    private void Attack()
    {
        transform.LookAt(_targetManager.GetTargetPosition());
        if (_navMeshAgent.enabled)
        {
            _navMeshAgent.SetDestination(transform.position);
        }

        EnableNavMeshObstacle(true);

        // Delay between each attack
        if (Time.time > _nextTimeToAttack)
        {
            _nextTimeToAttack = Time.time + _zombieSO.AttackRate;

            OnAttackStart?.Invoke(this, new OnAttackStartEventArgs
            {
                NextTimeToAttack = _nextTimeToAttack
            });
        }
    }

    private void Damage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_zombieHandTransform.position, _zombieHandRadius);
        foreach (var hitCollider in hitColliders)
        {
            //Debug.Log("ZombieController hitCollider : " + hitCollider.gameObject);
            if (hitCollider.TryGetComponent(out IDamageable damageable))
            {
                Vector3 contactPosition = hitCollider.ClosestPoint(_zombieHandTransform.position);
                damageable.AnyDamage(_zombieSO.Damage, contactPosition);
                break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("ZombieController : OnCollisionEnter -> " + collision.gameObject);
        PushRigidBodies(collision);
    }

    private void PushRigidBodies(Collision hit)
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
        //Debug.Log("ZombieController : AddForce -> " + body.gameObject);
    }
}