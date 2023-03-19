using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _bulletRigidbody;
    [SerializeField] private Transform _vfxHitGreen;
    [SerializeField] private Transform _trail;

    // Variables
    private float _damage;

    public void Setup(float damage)
    {
        _damage = damage;
    }

    private void Start()
    {
        float speed = 200f;
        _bulletRigidbody.velocity = transform.forward * speed;
        //_bulletRigidbody.AddForce(transform.forward * speed, ForceMode.Impulse);
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("BulletProjectile OnCollisionEnter : " + collision.gameObject);
        //_bulletRigidbody.velocity = Vector3.zero;
        _trail.SetParent(null);
        //_trail.GetComponent<TrailRenderer>().Clear();
        Instantiate(_vfxHitGreen, transform.position, Quaternion.identity);
        if (collision.gameObject.TryGetComponent(out IDamageable damageable))
        {
            //ContactPoint contact = collision.contacts[0];
            ContactPoint contact = collision.GetContact(0);
            Vector3 contactPosition = contact.point;
            damageable.AnyDamage(_damage, contactPosition);
        }
        //Debug.Log("BulletProjectile OnCollisionEnter : Destroyed");
        Destroy(gameObject);
    }
}