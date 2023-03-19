using UnityEngine;

[CreateAssetMenu(fileName = "Zombie", menuName = "Enemies/Zombie")]
public class ZombieSO : ScriptableObject
{
    [Header("References")]
    [Tooltip("Sound when zombie is walking")]
    public AudioClip WalkClip;
    [Tooltip("Sound when zombie is dead")]
    public AudioClip DeathClip;
    [Tooltip("Sound when zombie is attacking")]
    public AudioClip AttackClip;
    [Tooltip("Sound when zombie take hit")]
    public AudioClip HitClip;
    [Tooltip("Length of the attack ainmation")]
    public AnimationClip AttackAnimationClip;

    [Header("Variables")]
    [Tooltip("The zombie damage")]
    public float Damage;
    [Tooltip("The attack rate")]
    public float AttackRate;
    [Tooltip("The attack range")]
    public float AttackRange;

    private void OnEnable()
    {
        AttackRate = AttackAnimationClip.length;
    }
}