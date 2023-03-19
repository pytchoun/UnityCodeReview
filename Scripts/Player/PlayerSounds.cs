using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player _player;

    private void Start()
    {
        _player.OnDead += Player_OnDead;
        _player.OnHit += Player_OnHit;
    }

    private void Player_OnHit(object sender, System.EventArgs e)
    {
        AudioSource.PlayClipAtPoint(_player.GetPlayerSO().HitClip, transform.position, 1f);
    }

    private void Player_OnDead(object sender, Player.OnDeadEventArgs e)
    {
        AudioSource.PlayClipAtPoint(_player.GetPlayerSO().DeathClip, transform.position, 1f);
    }
}