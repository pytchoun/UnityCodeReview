using UnityEngine;

public class ZombieSounds : MonoBehaviour
{
    [SerializeField] private ZombieController _zombieController;
    [SerializeField] private AudioSource _audioSource;

    private void Start()
    {
        _zombieController.OnDead += ZombieController_OnDead;
        _zombieController.OnHit += ZombieController_OnHit;
        _zombieController.OnSpeedChanged += ZombieController_OnSpeedChanged;
        _zombieController.OnAttackStart += ZombieController_OnAttackStart;
    }

    private void ZombieController_OnAttackStart(object sender, ZombieController.OnAttackStartEventArgs e)
    {
        AudioSource.PlayClipAtPoint(_zombieController.GetZombieSO().AttackClip, transform.position, 1f);
    }

    private void ZombieController_OnSpeedChanged(object sender, ZombieController.OnSpeedChangedEventArgs e)
    {
        if (e.Speed > 0f)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }
        else
        {
            _audioSource.Stop();
        }
    }

    private void ZombieController_OnHit(object sender, System.EventArgs e)
    {
        AudioSource.PlayClipAtPoint(_zombieController.GetZombieSO().HitClip, transform.position, 1f);
    }

    private void ZombieController_OnDead(object sender, ZombieController.OnDeadEventArgs e)
    {
        AudioSource.PlayClipAtPoint(_zombieController.GetZombieSO().DeathClip, transform.position, 1f);
        _audioSource.Stop();
    }
}