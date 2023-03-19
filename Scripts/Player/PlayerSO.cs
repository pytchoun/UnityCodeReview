using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "Players/Player")]
public class PlayerSO : ScriptableObject
{
    [Header("References")]
    [Tooltip("Sound when player is dead")]
    public AudioClip DeathClip;
    [Tooltip("Sound when player take hit")]
    public AudioClip HitClip;
}